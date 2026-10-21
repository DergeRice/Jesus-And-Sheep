const {onRequest} = require('firebase-functions/v2/https');
const {initializeApp, getApps} = require('firebase-admin/app');
const {getAuth} = require('firebase-admin/auth');
const {getFirestore, FieldValue} = require('firebase-admin/firestore');
const express = require('express');
const crypto = require('node:crypto');
const domain = require('./domain');
if (!getApps().length) initializeApp();
const db = getFirestore();
const app = express();
app.disable('x-powered-by');
app.use(express.json({limit: '220kb'}));
app.use(express.urlencoded({extended: false, limit: '10kb'}));
app.use(async (req, res, next) => {
  const match = /^Bearer ([^\s]+)$/.exec(req.get('Authorization') || '');
  if (!match) return res.status(401).json({error: 'Authentication required'});
  try {
    req.uid = (await getAuth().verifyIdToken(match[1], true)).uid;
    next();
  } catch { res.status(401).json({error: 'Invalid session'}); }
});
const route = (method, path, handler) => app[method](path, (req, res, next) => Promise.resolve(handler(req, res)).catch(next));
const player = uid => db.collection('players').doc(uid);
const publicUser = (data, id = 0) => ({id, nickname: data.nickname || '', profileindex: data.profileindex || 0, highscore: data.highscore || 0});
const requirePlayer = async uid => {
  const doc = await player(uid).get();
  if (!doc.exists) throw Object.assign(new Error('Register a nickname first'), {status: 409});
  return doc;
};
route('post', '/versionCheck', async (req, res) => {
  const config = await db.doc('configuration/client').get();
  // A missing configuration does not strand the first release offline.
  const versions = config.data()?.supportedVersions;
  res.json({isCompatible: !Array.isArray(versions) || versions.includes(req.body.version)});
});
route('get', '/me', async (req, res) => {
  const doc = await player(req.uid).get();
  const wallet = await player(req.uid).collection("private").doc("giftWallet").get();
  res.json({registered: doc.exists, user: doc.exists ? publicUser(doc.data()) : null, claimedHeartCount: wallet.data()?.claimedHeartCount || 0});
});
route('get', '/check', async (req, res) => {
  const n = domain.nickname(req.query.nickname);
  const owner = await db.collection('nicknames').doc(n.key).get();
  res.json({available: !owner.exists || owner.data().uid === req.uid});
});
route('post', '/enroll/', async (req, res) => {
  const n = domain.nickname(req.body.nickname);
  const profileindex = domain.profileIndex(req.body.profileindex);
  const ref = player(req.uid), alias = db.collection('nicknames').doc(n.key);
  const result = await db.runTransaction(async tx => {
    const [existing, owner] = await Promise.all([tx.get(ref), tx.get(alias)]);
    if (owner.exists && owner.data().uid !== req.uid) throw Object.assign(new Error('Nickname already taken'), {status: 409});
    const old = existing.data();
    if (old?.nicknameKey && old.nicknameKey !== n.key) throw Object.assign(new Error('Nickname changes are not enabled'), {status: 409});
    const data = {nickname: n.name, nicknameKey: n.key, profileindex, highscore: old?.highscore || 0};
    tx.set(alias, {uid: req.uid});
    tx.set(ref, {...data, updatedAt: FieldValue.serverTimestamp()}, {merge: true});
    return publicUser(data);
  });
  res.json(result);
});
route('get', '/getRanking', async (req, res) => {
  const docs = await db.collection('leaderboards').doc(domain.month()).collection('entries').orderBy('highscore', 'desc').limit(100).get();
  res.json(docs.docs.map((doc, i) => publicUser(doc.data(), i + 1)));
});
route('post', '/update', async (req, res) => {
  const highscore = domain.score(req.body.highscore);
  const profile = player(req.uid);
  const rank = db.collection('leaderboards').doc(domain.month()).collection('entries').doc(req.uid);
  await db.runTransaction(async tx => {
    const [p, r] = await Promise.all([tx.get(profile), tx.get(rank)]);
    if (!p.exists) throw Object.assign(new Error('Register first'), {status: 409});
    tx.update(profile, {highscore: Math.max(p.data().highscore || 0, highscore)});
    tx.set(rank, {...publicUser(p.data()), highscore: Math.max(r.data()?.highscore || 0, highscore), updatedAt: FieldValue.serverTimestamp()});
  });
  res.json({success: true});
});
route('post', '/sendHeart', async (req, res) => {
  const target = domain.nickname(req.body.targetNickName);
  const alias = await db.collection('nicknames').doc(target.key).get();
  if (!alias.exists) throw Object.assign(new Error('User not found'), {status: 404});
  const targetUid = alias.data().uid;
  if (targetUid === req.uid) throw Object.assign(new Error('Cannot send to yourself'), {status: 400});
  const cooldown = player(req.uid).collection('heartCooldowns').doc(targetUid);
  const gift = player(targetUid).collection('gifts').doc(crypto.randomUUID());
  await db.runTransaction(async tx => {
    const [sender, recipient, last] = await Promise.all([tx.get(player(req.uid)), tx.get(player(targetUid)), tx.get(cooldown)]);
    if (!sender.exists || !recipient.exists) throw Object.assign(new Error('Register first'), {status: 409});
    const now = Date.now();
    if (last.exists && now - last.data().sentAt.toMillis() < 6 * 3600000) throw Object.assign(new Error('Heart cooldown'), {status: 429});
    tx.set(cooldown, {sentAt: FieldValue.serverTimestamp()});
    tx.set(gift, {senderNickName: sender.data().nickname, targetNickName: recipient.data().nickname, claimed: false, createdAt: FieldValue.serverTimestamp()});
  });
  res.json({success: true});
});
route('get', '/checkMyHeartList', async (req, res) => {
  const gifts = await player(req.uid).collection('gifts').where('claimed', '==', false).limit(100).get();
  res.json(gifts.docs.map(d => ({heartHash: d.id, senderNickName: d.data().senderNickName, targetNickName: d.data().targetNickName})));
});
route('post', '/receiveHeart', async (req, res) => {
  if (typeof req.body.heartHash !== 'string' || !/^[a-zA-Z0-9-]{1,80}$/.test(req.body.heartHash)) throw Object.assign(new Error('Invalid gift'), {status: 400});
  const ref = player(req.uid).collection('gifts').doc(req.body.heartHash);
  const wallet = player(req.uid).collection('private').doc('giftWallet');
  const claimedHeartCount = await db.runTransaction(async tx => {
    const [gift, balance] = await Promise.all([tx.get(ref), tx.get(wallet)]);
    if (!gift.exists || gift.data().claimed) throw Object.assign(new Error('Gift unavailable or already claimed'), {status: 409});
    tx.update(ref, {claimed: true, claimedAt: FieldValue.serverTimestamp()});
    const total = (balance.data()?.claimedHeartCount || 0) + 1;
    tx.set(wallet, {claimedHeartCount: total});
    return total;
  });
  res.json({success: true, claimedHeartCount});
});
route('get', '/save', async (req, res) => {
  const doc = await player(req.uid).collection('private').doc('save').get();
  res.json({exists: doc.exists, snapshot: doc.data()?.snapshot || null});
});
route('post', '/save', async (req, res) => {
  await requirePlayer(req.uid);
  const snapshot = domain.save(req.body.snapshot);
  await player(req.uid).collection('private').doc('save').set({snapshot, updatedAt: FieldValue.serverTimestamp()});
  res.json({success: true});
});
app.use((err, req, res, next) => {
  if (!err.status) console.error('Game API failure', err.message);
  res.status(err.status || 500).json({error: err.status ? err.message : 'Server error'});
});
exports.gameApi = onRequest({region: 'asia-northeast3', minInstances: 0, maxInstances: 2, memory: '256MiB', timeoutSeconds: 30}, app);
if (process.env.FIRESTORE_EMULATOR_HOST) module.exports.app = app;

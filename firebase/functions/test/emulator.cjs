const assert = require('node:assert/strict');
const {initializeApp} = require('firebase-admin/app');
const {getFirestore} = require('firebase-admin/firestore');
initializeApp({projectId: 'demo-jesus-sheep'});
const {app} = require('../index');
const server = app.listen(0);
const base = `http://127.0.0.1:${server.address().port}`;
let checks = 0;
async function auth() {
  const r = await fetch('http://127.0.0.1:9099/identitytoolkit.googleapis.com/v1/accounts:signUp?key=fake', {method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify({returnSecureToken:true})});
  return r.json();
}
async function call(token, method, path, body, status = 200) {
  const r = await fetch(base + path, {method, headers:{'Content-Type':'application/json', ...(token ? {Authorization:`Bearer ${token}`} : {})}, ...(body ? {body:JSON.stringify(body)} : {})});
  assert.equal(r.status, status, `${method} ${path}: ${await r.clone().text()}`); checks++;
  return r.json();
}
(async () => {
  const a = await auth(), b = await auth();
  await call(null, 'GET','/me',null,401);
  assert.equal((await call(a.idToken,'GET','/me')).registered,false); checks++;
  await call(a.idToken,'POST','/enroll/',{nickname:'Alice',profileindex:1,highscore:99999});
  assert.equal((await call(a.idToken,'GET','/me')).user.highscore,0); checks++;
  await call(b.idToken,'POST','/enroll/',{nickname:'alice'},409);
  await call(b.idToken,'POST','/enroll/',{nickname:'Bobby',profileindex:2});
  await call(a.idToken,'POST','/update',{nickname:'Bobby',highscore:123});
  assert.equal((await call(b.idToken,'GET','/me')).user.highscore,0); checks++;
  await call(a.idToken,'POST','/update',{highscore:12});
  assert.equal((await call(a.idToken,'GET','/getRanking'))[0].highscore,123); checks++;
  await call(a.idToken,'POST','/sendHeart',{senderNickName:'FORGED',targetNickName:'Bobby'});
  await call(a.idToken,'POST','/sendHeart',{targetNickName:'Bobby'},429);
  const gifts = await call(b.idToken,'GET','/checkMyHeartList?nickname=Alice');
  assert.equal(gifts[0].senderNickName,'Alice'); checks++;
  await call(a.idToken,'POST','/receiveHeart',{heartHash:gifts[0].heartHash},409);
  const claims = await Promise.all([fetch(base+'/receiveHeart',{method:'POST',headers:{'Content-Type':'application/json',Authorization:`Bearer ${b.idToken}`},body:JSON.stringify({heartHash:gifts[0].heartHash})}),fetch(base+'/receiveHeart',{method:'POST',headers:{'Content-Type':'application/json',Authorization:`Bearer ${b.idToken}`},body:JSON.stringify({heartHash:gifts[0].heartHash})})]);
  assert.deepEqual(claims.map(r=>r.status).sort(),[200,409]); checks++;
  assert.equal((await getFirestore().doc(`players/${b.localId}/private/giftWallet`).get()).data().claimedHeartCount,1); checks++;
  await call(a.idToken,'POST','/save',{snapshot:{hearts:3,settings:{gold:'17'}}});
  assert.equal((await call(b.idToken,'GET','/save')).exists,false); checks++;
  const refreshed = await fetch('http://127.0.0.1:9099/securetoken.googleapis.com/v1/token?key=fake',{method:'POST',headers:{'Content-Type':'application/x-www-form-urlencoded'},body:`grant_type=refresh_token&refresh_token=${encodeURIComponent(a.refreshToken)}`}).then(r=>r.json());
  assert.equal((await call(refreshed.id_token,'GET','/save')).snapshot.settings.gold,'17'); checks++;
  // The auth emulator accepts a synthetic Google ID token for account linking tests.
  const jwt = [Buffer.from(JSON.stringify({alg:'none'})).toString('base64url'),
    Buffer.from(JSON.stringify({sub:'google-test-user',email:'sheep@example.test',email_verified:true,
      aud:'test-web-client',iss:'https://accounts.google.com',iat:Math.floor(Date.now()/1000),exp:Math.floor(Date.now()/1000)+3600})).toString('base64url'),''].join('.');
  const linked = await fetch('http://127.0.0.1:9099/identitytoolkit.googleapis.com/v1/accounts:signInWithIdp?key=fake', {
    method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({idToken:a.idToken,
      requestUri:'http://localhost',postBody:`providerId=google.com&id_token=${encodeURIComponent(jwt)}`,returnSecureToken:true})
  }).then(r=>r.json());
  assert.equal(linked.localId,a.localId,JSON.stringify(linked)); checks++;
  assert.equal((await call(linked.idToken,'GET','/save')).snapshot.settings.gold,'17'); checks++;
  const googleAgain = await fetch('http://127.0.0.1:9099/identitytoolkit.googleapis.com/v1/accounts:signInWithIdp?key=fake', {
    method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({requestUri:'http://localhost',
      postBody:`providerId=google.com&id_token=${encodeURIComponent(jwt)}`,returnSecureToken:true})
  }).then(r=>r.json());
  assert.equal(googleAgain.localId,a.localId); checks++;
  assert.equal((await call(googleAgain.idToken,'GET','/save')).snapshot.settings.gold,'17'); checks++;
  const denied = await fetch(`http://127.0.0.1:8080/v1/projects/demo-jesus-sheep/databases/(default)/documents/players/${a.localId}`, {headers:{Authorization:`Bearer ${a.idToken}`}});
  assert.equal(denied.status,403); checks++;
  console.log(`Firebase integration checks passed: ${checks}`);
})().catch(e=>{console.error(e);process.exitCode=1;}).finally(async()=>{server.close();await getFirestore().terminate();});

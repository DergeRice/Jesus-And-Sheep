const crypto = require('node:crypto');
function nickname(value) {
  if (typeof value !== 'string') throw Object.assign(new Error('Invalid nickname'), {status: 400});
  const name = value.trim().normalize('NFKC');
  if (!/^[a-zA-Z0-9가-힣 ]{2,20}$/.test(name)) throw Object.assign(new Error('Nickname must contain 2–20 letters or digits'), {status: 400});
  return {name, key: crypto.createHash('sha256').update(name.toLowerCase()).digest('hex')};
}
function score(value) {
  const n = typeof value === 'string' && /^\d+$/.test(value) ? Number(value) : value;
  if (!Number.isSafeInteger(n) || n < 0 || n > 1000000) throw Object.assign(new Error('Invalid score'), {status: 400});
  return n;
}
function month(now = new Date()) { return now.toISOString().slice(0, 7); }
function profileIndex(value = 0) {
  if (!Number.isInteger(value) || value < 0 || value > 1000) throw Object.assign(new Error('Invalid profile'), {status: 400});
  return value;
}
function save(value) {
  if (!value || typeof value !== 'object' || Array.isArray(value)) throw Object.assign(new Error('Invalid save'), {status: 400});
  if (Buffer.byteLength(JSON.stringify(value)) > 200000) throw Object.assign(new Error('Save too large'), {status: 413});
  // Private backup only. Never use client saves to authorize purchases, gifts or ranked scores.
  return value;
}
module.exports = {nickname, score, month, profileIndex, save};

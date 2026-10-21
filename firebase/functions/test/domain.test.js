const {test} = require('node:test');
const assert = require('node:assert/strict');
const d = require('../domain');
test('nickname canonical ownership', () => {
  assert.equal(d.nickname(' Alice ').key, d.nickname('alice').key);
  assert.throws(() => d.nickname('../other'));
});
test('scores reject invalid submissions', () => {
  for (const x of [-1, 1.2, 'abc', '1e5', 1000001, undefined]) assert.throws(() => d.score(x));
  assert.equal(d.score('123'), 123);
});
test('monthly board changes without erasing profile', () => {
  assert.equal(d.month(new Date('2026-09-30T23:59:59Z')), '2026-09');
  assert.equal(d.month(new Date('2026-10-01T00:00:00Z')), '2026-10');
});
test('private backup has a size limit', () => {
  assert.throws(() => d.save({data: 'x'.repeat(200001)}));
});

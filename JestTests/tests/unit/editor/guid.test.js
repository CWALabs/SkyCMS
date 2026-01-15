const path = require('path');
const vm = require('vm');
const fs = require('fs');
const { describe, test, expect } = require('@jest/globals');

function loadGuid() {
  const filePath = path.join(__dirname, '../../../..', 'Editor/wwwroot/lib/cosmos/guid.js');
  const code = fs.readFileSync(filePath, 'utf8');
  const context = { window: {}, globalThis: {}, console };
  vm.createContext(context);
  vm.runInContext(code, context);
  return context.window.ccmsGenerateGuid || context.ccmsGenerateGuid || context.globalThis.ccmsGenerateGuid;
}

describe('shared GUID generator', () => {
  test('produces uuid-like string and uniqueness', () => {
    const gen = loadGuid();
    const a = gen();
    const b = gen();
    expect(a).toMatch(/^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/);
    expect(a).not.toBe(b);
  });
});

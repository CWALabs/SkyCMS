const fs = require('fs');
const path = require('path');
const vm = require('vm');
const { describe, test, expect, beforeEach } = require('@jest/globals');

function loadDuplicator() {
  const filePath = path.join(__dirname, '../../../..', 'Editor/wwwroot/lib/cosmos/dublicator/dublicator.js');
  let code = fs.readFileSync(filePath, 'utf8');

  // Inject exposure of onCreate from inside the IIFE before the return statement.
  code = code.replace('    return {', '    globalThis.__onCreate = onCreate;\n    return {');

  // Expose internals for testing
  code += '\n;globalThis.__exports = { Duplicator, __onCreate: globalThis.__onCreate, ccmsGenerateGuid };';

  const context = {
    window: global.window,
    document: global.document,
    console,
    ccmsGenerateGuid: () => 'guid-' + Math.random().toString(16).slice(2),
    createCkEditor: () => {},
    ccms___setupImageWidget: () => {}
  };

  vm.createContext(context);
  vm.runInContext(code, context);
  return context;
}

describe('Duplicator editable region handling', () => {
  let ctx;

  beforeEach(() => {
    ctx = loadDuplicator();
  });

  test('reassigns GUIDs for cloned editable children', () => {
    const container = document.createElement('div');
    container.setAttribute('data-ccms-cloned', 'true');

    const child1 = document.createElement('section');
    child1.setAttribute('data-ccms-ceid', 'orig-1');
    const child2 = document.createElement('div');
    child2.setAttribute('data-ccms-ceid', 'orig-2');
    container.appendChild(child1);
    container.appendChild(child2);

    ctx.__exports.__onCreate(container);

    const ids = [child1.getAttribute('data-ccms-ceid'), child2.getAttribute('data-ccms-ceid')];
    expect(ids[0]).not.toBe('orig-1');
    expect(ids[1]).not.toBe('orig-2');
    expect(container.hasAttribute('data-ccms-cloned')).toBe(false);
  });
});

const fs = require('fs');
const path = require('path');
const vm = require('vm');
const { describe, test, expect, beforeEach } = require('@jest/globals');

function buildFilePondStub() {
  const instances = [];
  const create = jest.fn((input) => {
    const pond = {
      editorElement: null,
      editorId: null,
      inputElement: input,
      setOptions: jest.fn(),
      on: jest.fn(),
      destroy: jest.fn()
    };
    instances.push(pond);
    return pond;
  });

  const find = jest.fn(() => instances[0]);

  return { create, find, registerPlugin: jest.fn(), _instances: instances };
}

function loadImageWidget() {
  const filePath = path.join(__dirname, '../../../..', 'Editor/wwwroot/lib/cosmos/image-widget/image-widget.js');
  let code = fs.readFileSync(filePath, 'utf8');

  // Expose internals for testing
  code += '\n;globalThis.__exports = { ccms___setupImageWidget, ccmsGenerateGuid };';

  const filePondStub = buildFilePondStub();

  // Capture DOMContentLoaded handler to avoid auto-running initialization during eval
  let domReadyHandler = null;
  const originalAddEventListener = document.addEventListener.bind(document);
  document.addEventListener = (evt, cb) => {
    if (evt === 'DOMContentLoaded') {
      domReadyHandler = cb;
    } else {
      originalAddEventListener(evt, cb);
    }
  };

  const context = {
    window: global.window,
    document: global.document,
    console,
    FilePond: filePondStub,
    FilePondPluginFileMetadata: {},
    fetch: jest.fn()
  };

  vm.createContext(context);
  vm.runInContext(code, context);

  // Restore addEventListener
  document.addEventListener = originalAddEventListener;

  return { ctx: context, domReadyHandler, filePondStub };
}

describe('image-widget setup', () => {
  let env;

  beforeEach(() => {
    env = loadImageWidget();
  });

  test('assigns GUID when data-ccms-new is present', () => {
    const el = document.createElement('div');
    el.setAttribute('data-editor-config', 'image-widget');
    el.setAttribute('data-ccms-new', 'true');

    env.ctx.__exports.ccms___setupImageWidget(el);

    expect(el.getAttribute('data-ccms-ceid')).toBeTruthy();
    expect(el.hasAttribute('data-ccms-new')).toBe(false);
  });
});

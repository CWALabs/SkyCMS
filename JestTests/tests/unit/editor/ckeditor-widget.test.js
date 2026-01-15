const fs = require('fs');
const path = require('path');
const vm = require('vm');
const { describe, test, expect, beforeEach } = require('@jest/globals');

function loadCkeditorWidget() {
  const filePath = path.join(__dirname, '../../../..', 'Editor/wwwroot/lib/cosmos/ckeditor/ckeditor-widget.301.js');
  let code = fs.readFileSync(filePath, 'utf8');

  // Strip import lines to allow vm execution without CKEditor packages
  code = code.replace(/^import[^;]+;\s*/gm, '');

  // Expose internals for testing
  code += '\n;globalThis.__exports = { ccms___createEditor, ccms___createEditors, ccmsGenerateGuid, EditorConfig, TitleEditorConfig };';

  const context = {
    window: global.window,
    document: global.document,
    console,
    parent: {
      enableAutoSave: false,
      saveEditorRegion: jest.fn(),
      ccms_setBannerImage: jest.fn()
    },
    articleNumber: 123,
    ccms_editors: [],
    focusedEditor: null,
    // Stub InlineEditor
    InlineEditor: {
      create: jest.fn(() => Promise.resolve({
        plugins: { get: () => ({ on: jest.fn() }) },
        editing: { view: { document: { on: jest.fn() } } },
        sourceElement: null
      }))
    }
  };

  // Stub CKEditor plugin constructors referenced in plugin arrays
  const pluginNames = [
    'Autoformat','AutoImage','Autosave','BalloonToolbar','BlockQuote','Bookmark','Bold','CodeBlock','Essentials','Heading','ImageBlock','ImageCaption','ImageInline','ImageInsert','ImageInsertViaUrl','ImageResize','ImageStyle','ImageTextAlternative','ImageToolbar','ImageUpload','Indent','IndentBlock','Italic','Link','LinkImage','List','ListProperties','MediaEmbed','Paragraph','PasteFromOffice','SimpleUploadAdapter','Table','TableCaption','TableCellProperties','TableColumnResize','TableProperties','TableToolbar','TextTransformation','TodoList','Underline','FileLink','InsertImage','PageLink','VsCodeEditor','SignalR'
  ];
  pluginNames.forEach(name => {
    context[name] = function Plugin() {};
  });

  vm.createContext(context);
  vm.runInContext(code, context);
  return context;
}

describe('ckeditor-widget configuration', () => {
  let ctx;

  beforeEach(() => {
    ctx = loadCkeditorWidget();
    jest.clearAllMocks();
  });

  test('assigns GUID when data-ccms-new is present', async () => {
    const el = document.createElement('div');
    el.setAttribute('data-ccms-new', 'true');

    ctx.__exports.ccms___createEditor(el);
    await Promise.resolve();

    expect(el.getAttribute('data-ccms-ceid')).toBeTruthy();
    expect(el.hasAttribute('data-ccms-new')).toBe(false);
  });

  test('uses TitleEditorConfig for headings and default for divs', async () => {
    const h1 = document.createElement('h1');
    h1.setAttribute('data-ccms-ceid', 'abc');
    const div = document.createElement('div');
    div.setAttribute('data-ccms-ceid', 'def');

    ctx.__exports.ccms___createEditor(h1);
    ctx.__exports.ccms___createEditor(div);
    await Promise.resolve();

    const calls = ctx.InlineEditor.create.mock.calls;
    expect(calls).toHaveLength(2);
    expect(calls[0][1]).toBe(ctx.__exports.TitleEditorConfig);
    expect(calls[1][1]).toBe(ctx.__exports.EditorConfig);
  });
});

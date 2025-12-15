
# Cosmos CMS Image Widget (Developer Notes)

The Image Widget powers inline image upload and management within the Editor. It integrates FilePond for uploads and exposes a small set of per‑widget attributes and a global configuration.

## Alt/Title Editor

The widget ships with an optional Alt/Title editor for accessibility and SEO.

- Global toggle: `CCMS_IMAGE_WIDGET_CONFIG.enableAltTextEditor` (default: `true`). Controls whether the Edit button appears and whether the editor can auto‑prompt after upload.
- Per‑widget activation: Add `data-ccms-enable-alt-editor="true"` on the widget container to enable the modal editor for that specific instance.
  - If the attribute is absent or not exactly `"true"`, the modal will not open.
  - When the edit button is clicked while disabled, the widget shows a brief info hint explaining how to enable it.

### Example

```html
<!-- Existing widget with a fixed id -->
<div data-editor-config="image-widget" data-ccms-ceid="img-123" data-ccms-enable-alt-editor="true">
  <img src="/pub/images/photo.jpg" class="ccms-img-widget-img" alt="A scenic photo" />
</div>

<!-- New widget: id is auto-generated on first init -->
<div data-editor-config="image-widget" data-ccms-new="true" data-ccms-enable-alt-editor="true"></div>
```

## Events

The widget publishes a single event via `window.CCMSImageWidgetEvents`:

- `imageChanged` — fired after an image is uploaded or deleted.
  - Payload: `{ type: 'uploaded' | 'deleted', id, element, imageSrc? }`

```js
window.CCMSImageWidgetEvents.on('imageChanged', (info) => {
  // info.type: 'uploaded' | 'deleted'
  // info.id: widget id (data-ccms-ceid)
  // info.element: widget container HTMLElement
  // info.imageSrc: provided for 'uploaded'
});
```

## Notes

- Default alt text after upload falls back to the filename (without extension) when no previous alt text exists.
- Upload destination is derived from `articleNumber` when available: `/pub/articles/{articleNumber}/`, otherwise `/pub/images/`.

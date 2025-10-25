# Cosmos CMS Image Widget

Interactive image upload and management widget used by the Sky Editor. It provides drag‑and‑drop uploads (via FilePond), an image library picker, in-place replacement, and an alt-text editor, then saves the widget’s HTML back to the editor region.

- Location (source): `Editor/wwwroot/lib/cosmos/image-widget/`
  - `image-widget.js`
  - `image-widget.css`
- Auto-init: On `DOMContentLoaded`, any `<div data-editor-config="image-widget">` on the page is initialized automatically.

## Features

- Drag‑and‑drop upload with progress (FilePond)
- Replace image via upload or library picker
- Alt text and title editor modal
- Delete image and reset back to upload state
- Automatic save callbacks into the parent editor

## Dependencies

Make sure these are loaded on pages using the widget:

- FilePond core
  - `Editor/wwwroot/lib/filepond/filepond.min.css`
  - `Editor/wwwroot/lib/filepond/filepond.min.js`
- FilePond File Metadata plugin
  - `Editor/wwwroot/lib/filepond-plugin-file-metadata/dist/filepond-plugin-file-metadata.min.js`
- Image widget CSS/JS
  - `Editor/wwwroot/lib/cosmos/image-widget/image-widget.css`
  - `Editor/wwwroot/lib/cosmos/image-widget/image-widget.js`
- Styling libraries (recommended in the Editor UI and used by the widget’s markup)
  - Bootstrap 5 (alerts, buttons, progress styles)
  - Font Awesome 6 (toolbar icons). If not present, replace the icon HTML in `CCMS_IMAGE_WIDGET_CONFIG`.

## Server endpoints and defaults

The widget posts uploads to your server and expects back a string URL to the uploaded file.

- Upload endpoint: `/FileManager/UploadImage`
- Image library endpoint: `/FileManager/GetImageAssets?path=...`
- Accepted file types: `.png, .jpg, .jpeg, .webp, .gif`
- Max file size: 25 MB (server should enforce)
- Upload path resolution:
  - If a global `articleNumber` exists: `/pub/articles/{articleNumber}/`
  - Otherwise: `/pub/images/`

These values are defined in `CCMS_IMAGE_WIDGET_CONFIG` inside `image-widget.js`.

## Parent page integration (save contract)

The widget calls functions on the parent window (useful when the editor runs in an iframe). If you’re not using an iframe, `parent` resolves to `window`.

Required callbacks:

- `parent.saveEditorRegion(html, elementId)` — called after a successful upload or when properties change
- `parent.saveChanges(html, elementId)` — called when the image is removed

Optional:

- `parent.saving()` — called when an upload starts
- `parent.saveInProgress` — boolean flag the widget sets while uploading
- `window.articleNumber` — if set, directs uploads to `/pub/articles/{articleNumber}/`

## Markup API

Add a container div that the widget will manage. Include the container class for proper toolbar visibility.

Required attributes/classes:

- `data-editor-config="image-widget"`
- One of:
  - `data-ccms-ceid="your-stable-id"` — use a fixed ID you manage
  - `data-ccms-new="true"` — widget will generate a new GUID and replace this attribute
- `class="ccms-img-widget-container"` — enables toolbar visibility via CSS

Pre-populated state (show an existing image): place an `<img>` inside. The widget will attach actions (edit alt, replace, library, delete).

## Simple HTML example

This example shows a page that loads required assets, wires the minimal save callbacks, and renders a new image widget that generates its own ID.

```html
<!doctype html>
<html lang="en">
  <head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Cosmos Image Widget Demo</title>

    <!-- Optional (recommended in Editor UI) -->
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@6.5.2/css/all.min.css" />

    <!-- FilePond -->
    <link rel="stylesheet" href="/lib/filepond/filepond.min.css" />

    <!-- Image Widget styles -->
    <link rel="stylesheet" href="/lib/cosmos/image-widget/image-widget.css" />
  </head>
  <body class="p-4">
    <h1 class="h4 mb-3">Image Widget</h1>

    <!-- New widget: auto-generates data-ccms-ceid -->
    <div class="ccms-img-widget-container"
         data-editor-config="image-widget"
         data-ccms-new="true"></div>

    <!-- Scripts (order matters) -->
    <script src="/lib/filepond/filepond.min.js"></script>
    <script src="/lib/filepond-plugin-file-metadata/dist/filepond-plugin-file-metadata.min.js"></script>
    <script src="/lib/cosmos/image-widget/image-widget.js"></script>

    <script>
      // Minimal parent integration for demo purposes
      window.saveEditorRegion = (html, id) => console.log('saveEditorRegion', id, html);
      window.saveChanges = (html, id) => console.log('saveChanges', id, html);
      window.saving = () => console.log('saving…');

      // Optional: route uploads under articles/{articleNumber}
      // window.articleNumber = 123;
    </script>
  </body>
</html>
```

## Pre-populated example

If you already have an image and want the widget to attach controls to it:

```html
<div class="ccms-img-widget-container"
     data-editor-config="image-widget"
     data-ccms-ceid="img-123">
  <img src="/pub/images/sample.jpg" class="ccms-img-widget-img" alt="Sample image" />
</div>
```

## How it works (lifecycle)

1. On page load, the script finds all `div[data-editor-config="image-widget"]`.
2. If no `<img>` is inside, it initializes a FilePond uploader.
3. During upload, it sets `parent.saveInProgress = true` and optionally calls `parent.saving()`.
4. On success, it replaces the uploader with an `<img>` and calls `parent.saveEditorRegion(html, elementId)`.
5. Hovering the image shows a toolbar with: Edit alt/title, Replace, Library, Delete.
6. Delete removes the image, calls `parent.saveChanges`, and returns to the upload state.

## Configuration notes

`image-widget.js` contains a `CCMS_IMAGE_WIDGET_CONFIG` constant with defaults (endpoints, file types, icons, etc.). If you need different endpoints or behavior, update this config in the script.

## Troubleshooting

- Icons not visible: ensure Font Awesome is loaded or replace the icon HTML in `CCMS_IMAGE_WIDGET_CONFIG`.
- Toolbar not appearing: ensure the container has `class="ccms-img-widget-container"` and that the widget found an `<img>`.
- Upload fails: verify `/FileManager/UploadImage` is reachable and returns the image URL as the response body. Check max size/type on server.
- Library empty: confirm `/FileManager/GetImageAssets?path=...` returns a JSON array of image URLs for the resolved path.
- Nothing happens on save: implement `saveEditorRegion` and `saveChanges` on the parent page (or window) as described above.

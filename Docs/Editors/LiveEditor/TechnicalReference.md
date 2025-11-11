# Live Editor Technical Reference

## Overview

This document provides technical details about the SkyCMS Live Editor implementation for developers and system administrators.

## Architecture

### Technology Stack

- **Frontend Editor**: CKEditor 5 (Balloon Block Editor build)
- **Editor Version**: CKEditor 5.44.1
- **License**: GPL
- **Code Editor**: Monaco Editor (VS Code engine)
- **Real-time Communication**: SignalR
- **Framework**: ASP.NET Core MVC
- **View Engine**: Razor

### Key Components

#### Server-Side

**EditorController.cs** (`Editor/Controllers/EditorController.cs`)

Main controller methods:
- `Edit(int id)` - GET: Loads the Live Editor interface
- `Edit(HtmlEditorPostViewModel model)` - POST: Saves page properties and content
- `EditSaveRegion(EditorRegionViewModel model)` - POST: Quick save for single editable region
- `EditSaveBody(EditorRegionViewModel model)` - POST: Saves entire page body
- `CcmsContent(int id)` - GET: Renders the editable content in iframe

**Key Features**:
- Auto-save support with encryption
- Version control
- Permission validation
- SignalR hub integration for real-time collaboration
- Backup/recovery using localStorage

#### Client-Side

**Edit.cshtml** (`Editor/Views/Editor/Edit.cshtml`)

Main editor view containing:
- Iframe for content display
- Modal dialogs (VS Code editor, image cropper, preview)
- SignalR connection setup
- Save handlers and encryption
- Collaboration notifications

**CcmsContent.cshtml** (`Editor/Views/Editor/CcmsContent.cshtml`)

Rendered inside the iframe:
- Editable content with `data-ccms-ceid` attributes
- CKEditor widget initialization
- Import maps for ES modules
- Tippy.js tooltips for editable regions
- FilePond for file uploads

### CKEditor Configuration

**Location**: `Editor/wwwroot/lib/cosmos/ckeditor/main.js`

**Build Type**: Inline Editor with Balloon Toolbar

**Plugins Included**:
- Core: Autoformat, Autosave, Essentials, Paragraph, PasteFromOffice
- Text Formatting: Bold, Italic, Underline, Heading
- Block Elements: BlockQuote, CodeBlock
- Lists: List, ListProperties, TodoList
- Images: ImageBlock, ImageCaption, ImageInline, ImageInsert, ImageResize, ImageStyle, ImageToolbar, ImageUpload
- Media: MediaEmbed
- Tables: Table, TableCaption, TableCellProperties, TableColumnResize, TableProperties, TableToolbar
- Utilities: Indent, IndentBlock, Link, Mention, TextTransformation
- Custom: PageLink (SkyCMS)

**Configuration Options**:

```javascript
{
  toolbar: {
    items: [
      'heading', '|', 'bold', 'italic', 'underline', '|',
      'link', 'pageLink', 'insertImage', 'mediaEmbed',
      'insertTable', 'blockQuote', 'codeBlock', '|',
      'bulletedList', 'numberedList', 'todoList',
      'outdent', 'indent'
    ]
  },
  balloonToolbar: [
    'bold', 'italic', '|', 'pageLink', 'link',
    'insertImage', '|', 'bulletedList', 'numberedList'
  ],
  // Additional configuration...
}
```

## Custom CKEditor Plugins

### 1. PageLink Plugin

**Location**: `Editor/wwwroot/lib/cosmos/ckeditor/pagelink/`

**Purpose**: Insert links to internal pages within the CMS

**Features**:
- Modal dialog with searchable page list
- Link properties configuration
- Automatic URL formatting

### 2. InsertImage Plugin

**Location**: `Editor/wwwroot/lib/cosmos/ckeditor/insertimage/`

**Purpose**: Custom image insertion with file manager integration

**Features**:
- File manager modal
- Image cropping
- Upload support via FilePond

### 3. FileLink Plugin

**Location**: `Editor/wwwroot/lib/cosmos/ckeditor/filelink/`

**Purpose**: Insert links to downloadable files

**Features**:
- File browser integration
- Automatic link text generation
- File metadata support

### 4. VsCodeEditor Plugin

**Location**: `Editor/wwwroot/lib/cosmos/ckeditor/vscodeeditor/`

**Purpose**: Open Monaco code editor for HTML editing

**Features**:
- Syntax highlighting
- Emmet support
- Live preview integration

### 5. SignalR Plugin

**Location**: `Editor/wwwroot/lib/cosmos/ckeditor/signalr/`

**Purpose**: Real-time collaboration features

**Features**:
- Broadcast editor events
- Lock detection
- User presence indicators

## Data Flow

### Editing Cycle

1. **Load**
   - User navigates to `/Editor/Edit/{articleNumber}`
   - Controller loads article data
   - View renders iframe pointing to `/Editor/CcmsContent/{articleNumber}`
   - CKEditor widgets initialize on elements with `data-ccms-ceid` attributes

2. **Edit**
   - User clicks editable region
   - CKEditor activates on that element
   - Balloon/block toolbars appear
   - User makes changes

3. **Save**
   - Auto-save triggers after idle period
   - OR user presses Ctrl+S / clicks Save
   - Content is encrypted using CryptoJS
   - POST to `/Editor/EditSaveRegion` or `/Editor/Edit`
   - Server validates and saves
   - SignalR broadcasts update to other users
   - Response updates version number and timestamp

### Real-time Collaboration

**SignalR Hub**: `Sky.Cms.Hubs.LiveEditorHub`

**Connection URL**: `/___cwps_hubs_live_editor`

**Events**:
- `JoinArticleGroup(articleNumber)` - User joins editing session
- `broadcastMessage(data)` - Server broadcasts changes

**Commands**:
- `saved` - Content saved successfully
- `keydown`, `mousedown`, `focus` - User started editing region
- `blur` - User stopped editing region
- `save` - Update editor with new content
- `PropertiesSaved` - Page properties updated
- `SavePageProperties` - Broadcast property changes

## Security

### Encryption

Content is encrypted before transmission using CryptoJS:

**Algorithm**: AES encryption

**Implementation**:
- Encryption key stored in database settings table
- Retrieved via `/Editor/GetEncryptionKey` endpoint
- Applied to content data before POST
- Server decrypts using `CryptoJsDecryption.Decrypt()`

### Authentication & Authorization

**Required Roles**:
- Authors (can edit, cannot publish)
- Team Members (can edit)
- Editors (can edit and publish)
- Administrators (full access)

**Controller Attribute**:
```csharp
[Authorize(Roles = "Reviewers, Administrators, Editors, Authors")]
```

### Validation

**Server-side Validation**:
- Nested editable region detection
- Permission checks
- Title uniqueness
- Model state validation

**Client-side Validation**:
- Permission checks before save
- Region lock detection
- Conflict prevention

## Data Models

### HtmlEditorViewModel

Properties:
- `Id` (Guid) - Article version ID
- `ArticleNumber` (int) - Article identifier
- `VersionNumber` (int) - Version number
- `Title` (string) - Page title
- `Content` (string) - HTML content
- `BannerImage` (string) - Banner image URL
- `UrlPath` (string) - Page URL path
- `Published` (DateTimeOffset?) - Publication date
- `Updated` (DateTimeOffset?) - Last update
- `ArticlePermissions` (List<ArticlePermission>) - Access control

### HtmlEditorPostViewModel

Properties:
- Inherits from HtmlEditorViewModel
- `EditorId` (string) - ID of edited region
- `Data` (string) - Encrypted content
- `UserId` (string) - Current user ID
- `Command` (string) - SignalR command
- `Offset` (int) - Not used
- `IsFocused` (bool) - Focus state

### EditorRegionViewModel

Properties:
- `ArticleNumber` (int) - Article identifier
- `EditorId` (string) - Region ID
- `Data` (string) - Encrypted HTML content

## Database Schema

### Articles Table

Relevant columns:
- `Id` (Guid, PK) - Version identifier
- `ArticleNumber` (int) - Article identifier
- `VersionNumber` (int) - Version sequence
- `Title` (string) - Page title
- `Content` (string) - HTML content
- `HeaderJavaScript` (string) - Head scripts
- `FooterJavaScript` (string) - Footer scripts
- `UrlPath` (string) - URL path
- `Published` (DateTimeOffset?) - Publication date
- `Updated` (DateTimeOffset) - Modification date
- `StatusCode` (int) - Status (Active=0, Inactive=1, Deleted=2)
- `UserId` (string) - Last editor

### ArticleCatalog Table

Aggregated view for quick lookups:
- Article permissions
- Latest version info
- Publication status

## Performance Considerations

### Auto-save Throttling

- Debounce period prevents excessive saves
- Status tracking prevents concurrent saves
- Local backup provides safety net

### SignalR Connection Management

- Automatic reconnection on disconnect
- Connection pooling
- Message compression

### Iframe Optimization

- Single iframe per page
- Height auto-adjustment
- Event delegation for efficiency

## Customization

### Adding Custom Toolbar Buttons

1. Create custom CKEditor plugin in `Editor/wwwroot/lib/cosmos/ckeditor/`
2. Import plugin in `main.js`
3. Add to plugin array
4. Add button to toolbar configuration

### Modifying Editor Configuration

Edit `Editor/wwwroot/lib/cosmos/ckeditor/main.js`:

```javascript
const editorConfig = {
  // Your custom configuration
  toolbar: {
    items: [/* custom items */]
  },
  // ...
};
```

### Adding Custom Save Handlers

In `Edit.cshtml`, add to save chain:

```javascript
function customPreSave() {
  // Custom logic before save
}

function saveChanges(html, editorId) {
  customPreSave();
  // Existing save logic
}
```

## Debugging

### Enable Console Logging

**SignalR Logging**:
```javascript
.configureLogging(signalR.LogLevel.Debug)
```

**CKEditor Logging**:
Add to editor configuration:
```javascript
{
  debug: true
}
```

### Common Debug Points

1. **Editor initialization**: Check browser console for CKEditor errors
2. **Save failures**: Network tab for failed POST requests
3. **SignalR issues**: Check connection status in console
4. **Encryption problems**: Verify encryption key retrieval

### Browser DevTools

- **F12** - Open DevTools
- **Console** - JavaScript errors and logs
- **Network** - Monitor AJAX requests
- **Application** - Check localStorage for backups

## API Endpoints

### GET Endpoints

- `/Editor/Edit/{id}` - Load Live Editor
- `/Editor/CcmsContent/{id}` - Render editable content
- `/Editor/GetEncryptionKey` - Get encryption key
- `/Editor/GetArticleList` - Get page list for linking

### POST Endpoints

- `/Editor/Edit` - Save page properties and content
- `/Editor/EditSaveRegion` - Quick save single region
- `/Editor/EditSaveBody` - Save entire body

### SignalR Hub

- Hub URL: `/___cwps_hubs_live_editor`
- Methods: `JoinArticleGroup`, `SendCoreAsync`

## File Structure

```
Editor/
├── Controllers/
│   └── EditorController.cs
├── Views/
│   └── Editor/
│       ├── Edit.cshtml
│       └── CcmsContent.cshtml
├── wwwroot/
│   └── lib/
│       ├── cosmos/
│       │   └── ckeditor/
│       │       ├── main.js
│       │       ├── pagelink/
│       │       ├── insertimage/
│       │       ├── filelink/
│       │       ├── vscodeeditor/
│       │       └── signalr/
│       ├── ckeditor/
│       │   └── ckeditor5.js
│       └── monaco-editor/
└── Hubs/
    └── LiveEditorHub.cs
```

## Dependencies

### NPM Packages

- `ckeditor5` - Core editor
- `monaco-editor` - Code editor
- `@microsoft/signalr` - Real-time communication
- `tippy.js` - Tooltips
- `cropperjs` - Image cropping
- `filepond` - File uploads

### NuGet Packages

- `Microsoft.AspNetCore.SignalR`
- `HtmlAgilityPack`
- Various Cosmos.* packages

## Testing

### Manual Testing Checklist

- [ ] Editor loads correctly
- [ ] Editable regions activate
- [ ] Toolbar appears on selection
- [ ] Save functionality works
- [ ] Auto-save triggers
- [ ] SignalR connection establishes
- [ ] Multiple users can collaborate
- [ ] Recovery modal works after crash
- [ ] Images upload successfully
- [ ] Links create properly
- [ ] Tables format correctly
- [ ] Code editor opens and applies changes

### Automated Testing

Consider testing:
- Controller actions
- SignalR hub methods
- JavaScript save functions
- Encryption/decryption
- Permission validation

## Deployment Notes

### Production Checklist

- [ ] Set proper CKEditor license key
- [ ] Configure SignalR scaling (if using multiple servers)
- [ ] Set up backup storage connection string
- [ ] Enable SSL/TLS for SignalR WebSocket
- [ ] Configure CORS if needed
- [ ] Set appropriate cache headers
- [ ] Minify JavaScript files
- [ ] Test auto-save frequency

### Environment Variables

- `BackupStorageConnectionString` - For database backups
- `BlobPublicUrl` - Public URL for media files

## Support

For issues or questions:
1. Check browser console for errors
2. Review server logs in `/Editor/Logs`
3. Examine SignalR connection status
4. Verify database connectivity
5. Check file permissions

## References

- [CKEditor 5 Documentation](https://ckeditor.com/docs/ckeditor5/)
- [CKEditor 5 Balloon Editor](https://ckeditor.com/docs/ckeditor5/latest/examples/builds/balloon-block-editor.html)
- [Monaco Editor Documentation](https://microsoft.github.io/monaco-editor/)
- [SignalR Documentation](https://docs.microsoft.com/aspnet/core/signalr/)

---

*Last Updated: October 2025*

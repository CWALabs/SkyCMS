{% include nav.html %}

# File Management in SkyCMS

The SkyCMS File Manager provides a comprehensive web-based interface for managing your website's files and folders. This powerful tool allows you to upload, organize, edit, and manage all your website assets including images, HTML files, CSS, JavaScript, and other web resources.

## Table of Contents

- [Overview](#overview)
- [Accessing the File Manager](#accessing-the-file-manager)
- [User Interface](#user-interface)
- [File and Folder Operations](#file-and-folder-operations)
- [Uploading Files](#uploading-files)
- [Editing Files](#editing-files)
- [Image Management](#image-management)
- [Advanced Features](#advanced-features)
- [Keyboard Shortcuts](#keyboard-shortcuts)
- [Troubleshooting](#troubleshooting)

## Overview

The SkyCMS File Manager is designed to give content creators and administrators full control over their website's file storage. With an intuitive interface similar to desktop file explorers, you can efficiently manage your web assets without needing FTP or command-line access.

### Key Features

- **Drag-and-drop file uploads** with chunked upload support for large files
- **Built-in code editor** with syntax highlighting for HTML, CSS, JavaScript, JSON, and XML
- **Advanced image editing** capabilities using the Filerobot image editor
- **Bulk operations** for moving, copying, and deleting multiple files
- **Thumbnail previews** for images
- **Breadcrumb navigation** for easy folder traversal
- **Pagination and sorting** for large file collections
- **Copy URL to clipboard** for quick file linking

### User Roles and Permissions

The File Manager is accessible to users with the following roles:
- Administrators
- Editors
- Authors
- Team Members

> **Note:** Reviewers have read-only access and cannot make changes through the File Manager.

## Accessing the File Manager

To access the File Manager:

1. Log in to the SkyCMS Editor
2. Navigate to your article or page
3. Click the **Files** button in the editor toolbar
4. The File Manager will open, starting in the `/pub` directory

Alternatively, you can access the File Manager directly through the main navigation menu if you have appropriate permissions.

## User Interface

### Main Components

#### 1. Navigation Breadcrumbs
At the top of the File Manager, you'll see breadcrumb navigation showing your current location in the folder structure:

```
Home > pub > articles > 123 > images
```

Each breadcrumb is clickable, allowing you to quickly navigate to parent folders.

#### 2. New Folder Creation
Next to the breadcrumbs, you'll find a text field to create new folders:
- Enter the desired folder name
- Click the **Create** button
- The folder will be created in your current location

#### 3. File Upload Area
Below the navigation, you'll see the file upload zone:
- **Drag and drop** files directly into this area, or
- **Click** to browse and select files from your computer
- Progress indicators show upload status for each file
- Large files are automatically split into chunks for reliable uploads

#### 4. Action Buttons

**Primary Actions:**
- **New file** - Create a new text-based file (HTML, CSS, JS, etc.)
- **Rename** - Rename a selected file or folder
- **Delete** - Permanently delete selected items
- **Copy/Move** - Copy or move selected items to a different folder

**Secondary Actions:**
- **Clear Selected** - Deselect all currently selected items
- **Show Selected** - View a list of all selected items
- **Show image thumbnails** - Toggle between list view and thumbnail view for images

#### 5. File and Folder Grid

The main content area displays your files and folders in either:

**List View:**
- Checkbox for selection
- Type icon (folder or file)
- Action buttons (copy URL, edit, image editor)
- File/folder name
- Last modified date

**Thumbnail View (Images):**
- Card-based layout showing image previews
- File information and metadata
- Quick action buttons for each image

#### 6. Pagination Controls
At the top and bottom of the file list, pagination controls allow you to:
- Navigate through pages of files
- Change the number of items displayed per page
- Sort by name, type, date, or size

## File and Folder Operations

### Creating Folders

1. Navigate to the parent folder where you want to create a new folder
2. In the breadcrumb area, enter the new folder name in the text field
3. Click **Create**
4. The folder appears immediately in the current directory

> **Important:** Folder names should not contain special characters or spaces. Use hyphens (-) or underscores (_) instead.

### Creating New Files

1. Click the **New file** button
2. Enter the file name with one of the supported extensions:
   - `.js` - JavaScript files
   - `.css` - Cascading Style Sheets
   - `.html` or `.htm` - HTML files
   - `.json` - JSON data files
   - `.xml` - XML files
   - `.txt` - Plain text files
3. Click **Create**
4. The file editor will open automatically

### Selecting Files and Folders

**Single Selection:**
- Click the checkbox next to any file or folder

**Multiple Selection:**
- Click checkboxes for multiple items
- Use **Show Selected** to view your selection list

**Directory-Only Mode:**
- In some contexts, only folders can be selected
- File checkboxes will be disabled

### Renaming Files or Folders

1. Select a **single** item by checking its checkbox
2. Click the **Rename** button
3. A dialog appears showing:
   - The current name
   - A text field for the new name
4. Enter the new name
5. Click **Change** to confirm

> **Warning:** If you change a file extension during rename, you'll see a warning message. Make sure the extension matches the file content type.

### Deleting Files and Folders

1. Select one or more items using checkboxes
2. Click the **Delete** button
3. A confirmation dialog appears
4. Click **Yes** to permanently delete the items

> **Caution:** Deletion is permanent and cannot be undone. Always verify your selection before confirming deletion.

### Copying Files and Folders

1. Select the items you want to copy
2. Click **Copy/Move**
3. Navigate to the destination folder
4. The interface shows special **Copy to here** and **Move to here** buttons
5. Click **Copy to here**
6. Items are duplicated to the current folder

### Moving Files and Folders

1. Select the items you want to move
2. Click **Copy/Move**
3. Navigate to the destination folder
4. Click **Move to here**
5. Items are relocated to the current folder

> **Note:** The `/pub` folder is the root for all publicly accessible files. You cannot create or modify files outside this directory.

### Downloading Files

To download a file:
- Click the file name in the list view, or
- Click **Download** button on an image card

The file downloads immediately to your browser's default download location.

## Uploading Files

### Drag and Drop Upload

The easiest way to upload files:

1. Navigate to the target folder
2. Drag files from your desktop or file explorer
3. Drop them into the upload zone
4. Watch the progress indicators as files upload

### Browse and Upload

Alternatively:

1. Click in the upload zone
2. A file browser dialog opens
3. Select one or more files
4. Click **Open**
5. Files begin uploading automatically

### Upload Features

**Chunked Uploads:**
- Large files are automatically split into 5MB chunks
- Ensures reliable uploads even with slow connections
- If an upload is interrupted, it can resume from the last successful chunk

**Multiple File Upload:**
- Upload multiple files simultaneously
- Progress shown for each file individually
- Total upload progress displayed at the top

**File Size Limits:**
- Individual files can be up to 25MB when using the simple upload
- Larger files use chunked upload automatically
- Images are processed for dimensions during upload

**Automatic Folder Creation:**
- If you upload files with folder paths, folders are created automatically
- Maintains directory structure from your local system

### Supported File Types

You can upload any file type, but the following have special handling:

**Code Files:**
- `.html`, `.htm` - Open in code editor
- `.css` - Open in code editor
- `.js` - Open in code editor
- `.json` - Open in code editor
- `.xml` - Open in code editor

**Images:**
- `.jpg`, `.jpeg` - Preview and edit
- `.png` - Preview and edit
- `.gif` - Preview and edit
- `.webp` - Preview and edit
- `.apng` - Preview support
- `.avif` - Preview support
- `.svg` - Preview support

## Editing Files

SkyCMS provides two powerful built-in editors for different types of content:

### Code Editor (Monaco/VS Code Editor)

The code editor is perfect for editing text-based files with syntax highlighting and advanced features.

#### Opening the Code Editor

1. Locate a supported file (`.html`, `.css`, `.js`, `.json`, `.xml`, `.txt`)
2. Click the **Monaco/VS Code editor** icon (looks like a code bracket)
3. The editor opens in a new view

#### Editor Features

**Syntax Highlighting:**
- Automatically detected based on file extension
- HTML, CSS, JavaScript, JSON, and XML supported
- Dark theme optimized for readability

**Code Assistance:**
- Emmet abbreviations for HTML
- Auto-completion suggestions
- Bracket matching and auto-closing
- Code folding

**Editing:**
- Full text editing capabilities
- Find and replace
- Multi-cursor editing
- Undo/redo support

#### Saving Changes

**Manual Save:**
- Click the **Save** button in the toolbar
- Changes are immediately saved to the server

**Keyboard Shortcut:**
- Press `Ctrl+S` (Windows/Linux) or `Cmd+S` (Mac)
- Quick save without using the mouse

**Auto-Save:**
- Enable auto-save in the editor settings
- Changes are automatically saved after a brief pause in typing
- Visual indicator shows when auto-save is active

### Image Editor (Filerobot)

The integrated Filerobot image editor provides professional-level image editing capabilities without leaving the CMS.

#### Opening the Image Editor

1. Locate an image file (`.jpg`, `.png`, `.gif`, `.webp`)
2. Click the **Filerobot image editor** icon
3. The editor opens with your image loaded

#### Editor Features

**Adjustment Tools:**
- Brightness and contrast
- Exposure and saturation
- Hue and vibrance
- Shadows and highlights
- Warmth adjustment

**Annotation Tools:**
- Text overlays with custom fonts and colors
- Shapes (rectangles, circles, polygons)
- Arrows and lines
- Free drawing with pen tool
- Stickers and emojis

**Filters:**
- Vintage, sepia, grayscale effects
- Blur and sharpen
- Instagram-style filters
- Custom filter adjustments

**Fine-Tuning:**
- Individual RGB channel adjustments
- Gamma correction
- Advanced color curves

**Resize:**
- Custom dimensions
- Preset sizes for social media
- Aspect ratio locking
- Smart cropping

**Crop:**
- Freeform cropping
- Preset aspect ratios (4:3, 21:9, etc.)
- Social media presets (Facebook, Instagram, etc.)

**Watermark:**
- Add text watermarks
- Upload image watermarks
- Position and opacity control

#### Saving Edited Images

1. Click the **Save** button in the image editor
2. The modified image is saved to the server
3. Original image is replaced with edited version
4. Click **Close** to return to the File Manager

> **Tip:** Always keep a backup of original images if you plan to make extensive edits.

## Image Management

### Viewing Images

**Thumbnail View:**
- Click **Show image thumbnails** to switch to gallery view
- Images display as cards with previews
- Folders are still shown for navigation
- Quick access to image editing tools

**List View:**
- Traditional file list showing all files
- Click **Show file list** to return to this view
- Compact display with more items per page

### Image Thumbnails

In list view, hovering over image names may show thumbnail previews depending on your browser settings.

To get a full-size thumbnail:
- The File Manager automatically generates 120x120px thumbnails
- Used in card view for faster loading
- Preserves aspect ratio with smart cropping

### Copying Image URLs

To get the URL of an image for use in your content:

1. Locate the image in the File Manager
2. Click the **clipboard icon** next to the image
3. The full URL is copied to your clipboard
4. Paste the URL into your content editor or code

The copied URL format is:
```
https://yourdomain.com/pub/images/your-image.jpg
```

### Image Optimization Tips

- **Use appropriate formats:**
  - JPEG for photographs
  - PNG for graphics with transparency
  - WebP for modern browsers (smaller file sizes)
  - SVG for logos and icons

- **Resize before upload:**
  - Upload images at their display size
  - Avoid uploading massive images that will be displayed small

- **Compress images:**
  - Use tools to compress images before upload
  - Balance quality and file size

## Advanced Features

### Bulk Operations

Select multiple items and perform operations on all of them at once:

1. **Bulk Delete:** Select multiple items and click Delete to remove them all
2. **Bulk Move:** Select items, click Copy/Move, navigate to destination, and move all at once
3. **Bulk Copy:** Same as move, but creates duplicates instead

### Clipboard Integration

The File Manager integrates with your system clipboard:
- **Copy URLs** directly to clipboard with one click
- **Paste** into your content without manual typing
- Success notification appears when URL is copied

### Session Management

The File Manager maintains your selection state:
- **Persistent Selection:** Your selected files remain selected as you navigate
- **Show Selected:** View all selected items across different folders
- **Clear Selection:** One-click to deselect everything

### Sorting and Filtering

Customize your file view:

**Sort by:**
- Name (alphabetical)
- Type (folders first, then by file type)
- Modified date (newest or oldest first)
- Size (largest or smallest first)

**Sort Order:**
- Ascending (A-Z, oldest-newest, smallest-largest)
- Descending (Z-A, newest-oldest, largest-smallest)

Click column headers to change sort field and order.

### Pagination

For folders with many files:
- Choose items per page (10, 20, 50, 100)
- Navigate using page numbers
- Jump to first/last page
- Current page is highlighted

### Article and Template Context

When editing an article or template:
- File Manager automatically navigates to the article/template folder
- Breadcrumb shows the article title instead of the article number
- Quick access to article-specific assets

### Image Selection Mode

In certain contexts, the File Manager operates in image selection mode:
- Only image files can be selected
- Automatically switches to thumbnail view
- Used when inserting images into content
- Select an image and it's automatically inserted

## Keyboard Shortcuts

Speed up your workflow with these keyboard shortcuts:

### Code Editor
- `Ctrl+S` / `Cmd+S` - Save file
- `Ctrl+F` / `Cmd+F` - Find
- `Ctrl+H` / `Cmd+H` - Find and replace
- `Ctrl+Z` / `Cmd+Z` - Undo
- `Ctrl+Y` / `Cmd+Y` - Redo
- `Alt+Up/Down` - Move line up/down
- `Ctrl+/` / `Cmd+/` - Toggle comment

### File Manager
- `Ctrl+Click` - Select multiple non-consecutive items (if supported by context)
- `Shift+Click` - Select range of items (if supported by context)

## Troubleshooting

### Upload Issues

**Problem:** Files won't upload
- **Check file size** - Ensure files are under 25MB for simple uploads
- **Check connection** - Verify your internet connection is stable
- **Check permissions** - Make sure you have upload rights in the current folder
- **Try chunked upload** - Large files automatically use chunked upload

**Problem:** Upload stalls at a certain percentage
- **Wait** - Large files may pause between chunks
- **Check browser console** - Look for error messages
- **Retry** - Cancel and try uploading again
- **Check server storage** - Ensure sufficient storage space is available

### Editor Issues

**Problem:** Code editor won't load
- **Clear browser cache** - Old cached scripts may cause issues
- **Check browser compatibility** - Use a modern browser (Chrome, Firefox, Edge)
- **Disable extensions** - Browser extensions may interfere
- **Try incognito mode** - Test without extensions

**Problem:** Can't save changes in code editor
- **Check connection** - Verify you're still logged in
- **Check permissions** - Ensure you have write access
- **Check file lock** - Someone else may be editing the file
- **Refresh and retry** - Reload the page and try again

### Image Editor Issues

**Problem:** Image editor won't open
- **Check file size** - Very large images may fail to load
- **Check format** - Ensure image format is supported
- **Clear cache** - Browser cache may have corrupt data
- **Try another browser** - Test with a different browser

**Problem:** Can't save edited image
- **Check storage quota** - Ensure sufficient space available
- **Check permissions** - Verify write access to the folder
- **Reduce file size** - Very large images may timeout
- **Save again** - Sometimes a retry works

### Navigation Issues

**Problem:** Breadcrumbs not showing correctly
- **Refresh page** - Simple refresh often fixes navigation issues
- **Clear session** - Log out and log back in
- **Report issue** - If persistent, contact your administrator

**Problem:** Can't navigate to certain folders
- **Check permissions** - You may not have access to that folder
- **Check path** - Ensure the folder exists
- **Use breadcrumbs** - Navigate using the breadcrumb trail instead

### Selection Issues

**Problem:** Can't select files
- **Check mode** - You may be in copy/move mode
- **Cancel operation** - Click Cancel to exit special modes
- **Refresh page** - Reload to reset the interface
- **Clear session storage** - Browser session storage may be corrupted

**Problem:** Selected items disappear
- **Check session** - Your session may have expired
- **Use Show Selected** - View all selected items
- **Re-select items** - Select items again if needed

### Performance Issues

**Problem:** File Manager is slow
- **Reduce page size** - Display fewer items per page
- **Use list view** - Thumbnail view is more resource-intensive
- **Clear browser data** - Remove old cached data
- **Close other tabs** - Free up browser resources

**Problem:** Thumbnails won't load
- **Wait** - Thumbnails generate on-demand
- **Check network** - Verify good internet connection
- **Switch to list view** - Use list view as an alternative
- **Clear cache** - Browser cache may have corrupt thumbnails

### Getting Help

If you continue experiencing issues:

1. **Check documentation** - Review this guide thoroughly
2. **Contact support** - Reach out to your administrator
3. **Report bugs** - Provide detailed information about the issue
4. **Include details:**
   - What you were trying to do
   - What happened instead
   - Browser and version
   - Any error messages
   - Steps to reproduce the issue

---

## Best Practices

### Organization
- **Use descriptive folder names** - Make it easy to find content later
- **Create logical hierarchies** - Group related files together
- **Follow naming conventions** - Use consistent naming patterns
- **Avoid spaces** - Use hyphens or underscores in names

### File Management
- **Regular cleanup** - Delete unused files periodically
- **Keep backups** - Download important files as backups
- **Optimize before upload** - Compress and resize images
- **Use appropriate formats** - Choose the right file type for the content

### Security
- **Don't upload sensitive data** - Public folders are web-accessible
- **Check file permissions** - Understand who can access your files
- **Rename default files** - Avoid using predictable file names
- **Monitor file changes** - Review file modifications regularly

### Performance
- **Optimize images** - Smaller files load faster
- **Use CDN when available** - Content delivery networks speed up access
- **Minimize file count** - Combine small files when possible
- **Clean up old versions** - Remove outdated files

---

This documentation is maintained as part of the SkyCMS project. For updates and additional information, please visit the [SkyCMS GitHub repository](https://github.com/CWALabs/SkyCMS).

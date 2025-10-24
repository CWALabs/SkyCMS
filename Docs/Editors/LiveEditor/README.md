# SkyCMS Live Editor User Guide

## Table of Contents

1. [Introduction](#introduction)
2. [Getting Started](#getting-started)
3. [Understanding the Live Editor Interface](#understanding-the-live-editor-interface)
4. [Editing Content](#editing-content)
5. [Using the Toolbar](#using-the-toolbar)
6. [Advanced Features](#advanced-features)
7. [Saving Your Work](#saving-your-work)
8. [Keyboard Shortcuts](#keyboard-shortcuts)
9. [Tips and Best Practices](#tips-and-best-practices)
10. [Troubleshooting](#troubleshooting)

---

## Introduction

The **SkyCMS Live Editor** is a powerful, intuitive WYSIWYG (What You See Is What You Get) content editor that allows you to edit web pages directly in their published context. Built on CKEditor 5's Balloon Block Editor configuration, it provides a seamless editing experience where you can modify content inline while viewing your page exactly as it will appear to visitors.

### Key Features

- **Inline Editing**: Edit content directly on your page without switching to a separate editor interface
- **Balloon Toolbar**: A context-sensitive toolbar that appears only when you need it
- **Block Toolbar**: Quick access to paragraph and block-level formatting options
- **Real-time Collaboration**: See when other users are editing the same page
- **Auto-save**: Your changes are automatically saved as you work
- **Rich Media Support**: Insert images, videos, tables, and links with ease
- **Custom SkyCMS Plugins**: Special tools for page links, file management, and code editing

---

## Getting Started

### Accessing the Live Editor

1. Log in to your SkyCMS administrative interface
2. Navigate to the page catalog (Editor → Index)
3. Locate the page you want to edit
4. Click the "Edit" button for that page
5. If the page uses the Live Editor (has editable regions marked with `data-ccms-ceid` attributes), you'll be taken to the Live Editor interface

### System Requirements

- Modern web browser (Chrome, Firefox, Edge, Safari - latest versions)
- JavaScript enabled
- User role: Authors, Editors, Administrators, or Team Members
- Internet connection for auto-save functionality

---

## Understanding the Live Editor Interface

### Main Components

#### 1. **Navigation Bar** (Top)
The top navigation bar contains:
- **Page Title**: Displays the current page you're editing
- **Version Number**: Shows which version you're working on
- **Save Button**: Manual save option (though auto-save is enabled)
- **Preview Button**: View your page as visitors will see it
- **Close Button**: Exit the editor (will prompt to save if changes exist)
- **Other Pages**: Quick navigation to other pages
- **Versions**: Access version history
- **Code Editor**: Switch to Monaco code editor for advanced HTML editing
- **GrapesJS Editor**: Switch to the visual designer

#### 2. **Editable Content Areas**
Pages contain one or more editable regions marked by:
- Subtle dashed borders when hovering
- A tooltip showing "Editable" status
- Blue borders when another user is editing the same region

#### 3. **Content Frame**
The main editing area displays your page in an iframe, allowing you to:
- See the page in its published layout
- Edit content in context with surrounding elements
- Navigate through the page structure

#### 4. **Toast Notifications**
Small popup messages appear in the top-right corner to inform you about:
- Link clicks (which are disabled during editing)
- Save status
- Collaboration notifications

---

## Editing Content

### Starting to Edit

1. **Click on any editable region** - The region will become active, indicated by:
   - The content becoming selectable
   - A block toolbar button appearing on the left margin
   - The balloon toolbar appearing when you select text

2. **Select text** to format it or perform other text operations

3. **Click the block toolbar button** (⊕) to:
   - Change paragraph types
   - Insert new blocks
   - Access block-level formatting

### Text Formatting

The Live Editor supports standard text formatting options:

- **Bold** (Ctrl/Cmd + B): Make text bold
- **Italic** (Ctrl/Cmd + I): Italicize text
- **Underline** (Ctrl/Cmd + U): Underline text
- **Headings**: Convert paragraphs to headings (H1-H6)
- **Lists**: Create bulleted, numbered, or todo lists
- **Block Quotes**: Format text as quotations
- **Code Blocks**: Insert formatted code snippets

### Working with Links

The Live Editor includes two types of link insertion:

#### Standard Links
1. Select the text you want to link
2. Click the **Link** button in the balloon toolbar
3. Enter the URL
4. Optionally check "Open in a new tab"
5. Click Save

#### Page Links (SkyCMS Custom)
1. Click where you want to insert a link or select existing text
2. Click the **Page Link** button in the toolbar
3. A modal will open with a searchable list of pages on your website
4. Select the page you want to link to
5. Configure link properties:
   - Link text
   - Open in new window (optional)
   - CSS class (optional)
   - Inline styles (optional)
6. Click Insert

**Note**: All links are disabled while editing to prevent accidental navigation away from the editor.

---

## Using the Toolbar

### Balloon Toolbar

The **balloon toolbar** appears when you select text and includes:

- **Bold** / **Italic** - Basic text formatting
- **Link** - Insert or edit hyperlinks
- **Page Link** - SkyCMS custom plugin for internal page links
- **Insert Image** - Add images to your content
- **Lists** - Bulleted and numbered lists

**Behavior**:
- Appears above selected text
- Disappears when you click away
- Position adjusts automatically to stay visible

### Block Toolbar

The **block toolbar button** (⊕) appears in the left margin when you hover over or click into a block-level element:

- **Click it** to open the block toolbar menu
- **Select block type**: Paragraph, Heading 1-6
- **Insert elements**: Tables, images, media, code blocks
- **Change formatting**: Convert to quote, list, etc.

### Main Toolbar

A persistent toolbar at the top provides access to:

1. **Heading** - Paragraph style dropdown
2. **Bold / Italic / Underline** - Text formatting
3. **Link / Page Link** - Hyperlink tools
4. **Insert Image** - Image insertion
5. **Media Embed** - Embed videos and media
6. **Insert Table** - Create data tables
7. **Block Quote** - Format quotations
8. **Code Block** - Insert code snippets
9. **Bulleted List / Numbered List / Todo List** - List creation
10. **Outdent / Indent** - List and paragraph indentation

---

## Advanced Features

### Image Insertion and Management

The Live Editor provides multiple ways to work with images:

#### Inserting Images

1. **Via Upload**:
   - Click the **Insert Image** button
   - A file manager modal opens
   - Browse your files or upload new images
   - Select an image to insert it

2. **Via URL**:
   - Some configurations allow inserting images by URL
   - Click Insert Image → Insert by URL
   - Paste the image URL

#### Image Editing

Once an image is inserted:
- **Resize**: Drag the corners to resize
- **Add Caption**: Click below the image
- **Alternative Text**: Right-click → Image properties
- **Style**: Choose inline, wrapped, or break text styles
- **Alignment**: Use the image toolbar

#### Image Cropping

For uploaded images:
1. After selecting an image, a crop option may appear
2. Adjust the crop area
3. Click "Crop" to apply

### Table Management

Create and edit tables with extensive formatting options:

1. **Insert Table**:
   - Click the **Insert Table** button
   - Select grid size (rows × columns)
   - Click to insert

2. **Table Toolbar** (appears when table is selected):
   - Add/remove columns and rows
   - Merge/split cells
   - Set table properties (borders, colors, alignment)
   - Set cell properties

3. **Table Properties**:
   - Border style and color
   - Background color
   - Width and height
   - Alignment
   - Cell padding and spacing

### Media Embedding

Embed videos and other media:

1. Click **Media Embed** button
2. Paste the media URL (YouTube, Vimeo, etc.)
3. The editor will automatically create an embed
4. Adjust size using the resize handles

### Code Blocks

For displaying formatted code:

1. Click **Code Block** button
2. Paste or type your code
3. The code will be displayed with monospace font
4. Language syntax highlighting may be available

### File Links (SkyCMS Custom)

Insert links to downloadable files:

1. Click the **File Link** button in the toolbar
2. The file manager modal opens
3. Browse to or upload the file
4. Select the file to insert a link

### VS Code Editor Integration

For advanced HTML editing:

1. Click the **Edit Code** button (appears as a code icon)
2. The Monaco editor (VS Code's editor engine) opens in a modal
3. Edit the HTML source directly with:
   - Syntax highlighting
   - Auto-completion
   - Emmet abbreviations
4. Click **Apply** to update the content
5. Changes are automatically saved

---

## Saving Your Work

### Auto-Save

The Live Editor includes automatic saving:

- **Triggers**: Changes are saved automatically after you stop typing (with a slight delay)
- **Indicator**: Watch the top navigation bar for save status
- **Backup**: A local backup is maintained in your browser's localStorage

### Manual Save

You can also save manually:

- **Keyboard**: Press `Ctrl+S` (Windows/Linux) or `Cmd+S` (Mac)
- **Save Button**: Click the Save button in the navigation bar

### Save Behavior

When you save:
1. The content is encrypted before transmission (for security)
2. The page version number is updated
3. Other users editing the same page are notified
4. The last modified timestamp is updated
5. A success notification appears

### Version Control

- Each save creates a new version
- Access previous versions via the **Versions** button
- Compare versions to see changes
- Restore previous versions if needed

### Backup Recovery

If the browser closes unexpectedly:
1. When you reopen the editor, a recovery modal appears
2. Click **Restore** to recover unsaved changes
3. Click **Discard** to start fresh

---

## Keyboard Shortcuts

### Common Shortcuts

| Action | Windows/Linux | Mac |
|--------|---------------|-----|
| **Save** | Ctrl + S | Cmd + S |
| **Bold** | Ctrl + B | Cmd + B |
| **Italic** | Ctrl + I | Cmd + I |
| **Underline** | Ctrl + U | Cmd + U |
| **Undo** | Ctrl + Z | Cmd + Z |
| **Redo** | Ctrl + Y / Ctrl + Shift + Z | Cmd + Y / Cmd + Shift + Z |
| **Select All** | Ctrl + A | Cmd + A |
| **Copy** | Ctrl + C | Cmd + C |
| **Cut** | Ctrl + X | Cmd + X |
| **Paste** | Ctrl + V | Cmd + V |

### Editor-Specific Shortcuts

- **Ctrl/Cmd + K**: Insert/edit link
- **Tab**: Indent list item or table cell navigation
- **Shift + Tab**: Outdent list item or reverse table cell navigation
- **Enter**: Create new paragraph
- **Shift + Enter**: Line break within paragraph

---

## Tips and Best Practices

### Content Editing Tips

1. **Preview Frequently**: Use the Preview button to see how your page will look to visitors

2. **Use Semantic Headings**: Structure your content with proper heading hierarchy (H1 → H2 → H3)

3. **Optimize Images**: Upload appropriately sized images to improve page load times

4. **Write Descriptive Alt Text**: Always add alternative text for images for accessibility

5. **Use Lists**: Break up text with bulleted or numbered lists for better readability

6. **Link Descriptively**: Use descriptive link text instead of "click here"

### Collaboration Tips

1. **Check for Active Editors**: Red borders indicate someone else is editing that region

2. **Coordinate with Team**: Communicate with team members when editing shared pages

3. **Watch for Notifications**: Pay attention to toast notifications about other users

4. **Save Regularly**: Even with auto-save, manual saves ensure your work is preserved

### Performance Tips

1. **Limit Simultaneous Editors**: Too many users editing the same region can cause conflicts

2. **Close Unused Editor Tabs**: Each open editor maintains a connection

3. **Use Modern Browsers**: Older browsers may have performance issues

4. **Clear Browser Cache**: If the editor seems slow, try clearing your browser cache

---

## Troubleshooting

### Common Issues and Solutions

#### Editor Not Loading

**Problem**: The page loads but editable regions don't activate

**Solutions**:
- Check that JavaScript is enabled in your browser
- Verify you have appropriate permissions (Authors, Editors, Administrators, or Team Members role)
- Try refreshing the page (F5 or Ctrl/Cmd + R)
- Clear browser cache and cookies
- Check browser console for error messages (F12 → Console tab)

#### Cannot Save Changes

**Problem**: Save button doesn't work or changes aren't persisting

**Solutions**:
- Check your internet connection
- Verify you're still logged in (session may have expired)
- Check if someone else is editing the same region
- Try manual save with Ctrl/Cmd + S
- Check for browser extensions that might block requests
- Look for error messages in toast notifications

#### Content Appears Different After Saving

**Problem**: Formatting changes after save

**Solutions**:
- Use the preview function to check appearance before saving
- Avoid pasting content from Microsoft Word (use Paste from Office feature)
- Check for conflicting CSS styles
- Use the Code Editor to inspect HTML if needed

#### Images Not Uploading

**Problem**: Image upload fails

**Solutions**:
- Check file size (may have limits)
- Verify file format is supported (JPG, PNG, GIF, WebP)
- Check available storage space
- Try a different image
- Check permissions for the upload directory

#### Toolbar Not Appearing

**Problem**: Balloon toolbar doesn't show when selecting text

**Solutions**:
- Click directly on the editable region first
- Try selecting text again
- Check if the region is actually editable (look for tooltip)
- Refresh the page
- Try a different browser

#### Multiple Users Editing Conflict

**Problem**: Changes from multiple users are conflicting

**Solutions**:
- Coordinate with team members to edit different regions
- Wait for others to finish if they're editing the same region
- Use version history to recover if changes are overwritten
- Refresh to see latest changes before editing

#### Recovery Modal Issues

**Problem**: Recovery modal appears but shouldn't, or doesn't appear when it should

**Solutions**:
- If unwanted: Click "Discard" to clear the recovery data
- If missing: Check browser localStorage settings
- If corrupted: Clear browser localStorage for the site

### Getting Help

If you encounter issues not covered here:

1. **Check Version History**: Go to Versions to see if your changes were saved
2. **Contact Administrator**: Your system administrator may have additional insights
3. **Review Logs**: Administrators can check the Editor logs for errors
4. **Browser Console**: Press F12 and check the Console tab for JavaScript errors
5. **Report Issues**: Document the issue with steps to reproduce and screenshots

---

## Additional Resources

### CKEditor 5 Documentation

The SkyCMS Live Editor is built on CKEditor 5's Balloon Block Editor:
- [CKEditor 5 Balloon Block Editor Documentation](https://ckeditor.com/docs/ckeditor5/latest/examples/builds/balloon-block-editor.html)
- [CKEditor 5 Features Guide](https://ckeditor.com/docs/ckeditor5/latest/features/index.html)

### SkyCMS Resources

- **Editor Controller**: `Editor/Controllers/EditorController.cs` - Server-side logic
- **View Template**: `Editor/Views/Editor/Edit.cshtml` - Main editor view
- **Content View**: `Editor/Views/Editor/CcmsContent.cshtml` - Editable content rendering
- **Configuration**: `Editor/wwwroot/lib/cosmos/ckeditor/main.js` - CKEditor configuration

### Custom Plugins

SkyCMS includes custom CKEditor plugins located in:
- `Editor/wwwroot/lib/cosmos/ckeditor/pagelink/` - Internal page linking
- `Editor/wwwroot/lib/cosmos/ckeditor/insertimage/` - Image management
- `Editor/wwwroot/lib/cosmos/ckeditor/filelink/` - File linking
- `Editor/wwwroot/lib/cosmos/ckeditor/vscodeeditor/` - Code editor integration
- `Editor/wwwroot/lib/cosmos/ckeditor/signalr/` - Real-time collaboration

---

## Conclusion

The SkyCMS Live Editor provides a powerful yet intuitive way to edit your website content. By editing directly in context and using the balloon toolbar interface, you can create and modify content efficiently while seeing exactly how it will appear to your visitors.

Take time to explore the various features, practice with the keyboard shortcuts, and don't hesitate to use the preview function to verify your changes. With auto-save protecting your work and version control allowing you to revert changes, you can edit with confidence.

Happy editing!

---

*Last Updated: October 2025*  
*SkyCMS Version: Latest*  
*CKEditor Version: 5 (Balloon Block Editor)*

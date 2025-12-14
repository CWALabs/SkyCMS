{% include nav.html %}

# SkyCMS File Management Documentation

Welcome to the SkyCMS File Management documentation. This collection of guides will help you master the File Manager and its powerful editing capabilities.

## Documentation Overview

### [Quick Start Guide](Quick-Start.md)
**New to the File Manager?** Start here for a quick introduction to the essential features.

- Basic file operations
- Quick upload guide
- Common tasks
- Essential keyboard shortcuts

**Time to complete:** 5-10 minutes

### [Complete File Management Guide](README.md)
**Comprehensive reference** covering all File Manager features in detail.

Topics covered:
- User interface walkthrough
- All file and folder operations
- Upload methods and features
- Navigation and organization
- Advanced features
- Troubleshooting

**Best for:** Users who want to understand all capabilities

### [Code Editing Guide](Code-Editing.md)
**Master the code editor** with this detailed guide to editing HTML, CSS, JavaScript, and more.

Topics covered:
- Editor interface and features
- Syntax highlighting
- Code completion (IntelliSense)
- Emmet abbreviations
- Multi-cursor editing
- Find and replace
- Keyboard shortcuts
- Best practices

**Best for:** Developers and content creators who edit code

### [Image Editing Guide](Image-Editing.md)
**Professional image editing** right in your browser with the Filerobot editor.

Topics covered:
- Image adjustments (brightness, contrast, etc.)
- Annotations and text overlays
- Filters and effects
- Cropping and resizing
- Watermarks
- Common editing tasks
- Tips for professional results

**Best for:** Content creators working with images

## Quick Reference

### Supported File Types

**Code Files:**
- `.html`, `.htm` - HTML documents
- `.css` - Stylesheets
- `.js` - JavaScript
- `.json` - JSON data
- `.xml` - XML documents
- `.txt` - Plain text

**Image Files:**
- `.jpg`, `.jpeg` - JPEG images
- `.png` - PNG images
- `.gif` - GIF images
- `.webp` - WebP images
- `.svg` - SVG graphics
- `.apng`, `.avif` - Modern formats

### Essential Keyboard Shortcuts

**File Manager:**
- Navigate with breadcrumbs
- Drag and drop to upload
- Checkbox to select items

**Code Editor:**
- `Ctrl+S` / `Cmd+S` - Save
- `Ctrl+F` / `Cmd+F` - Find
- `Ctrl+/` / `Cmd+/` - Toggle comment
- `Ctrl+Z` / `Cmd+Z` - Undo

**Image Editor:**
- Use tabs to access different tools
- Click Save when finished editing
- Close to return to File Manager

### Common Tasks

| Task | Guide | Section |
|------|-------|---------|
| Upload files | [Quick Start](Quick-Start.md) | Upload a File |
| Create folders | [Quick Start](Quick-Start.md) | Create a New Folder |
| Edit HTML/CSS/JS | [Code Editing](Code-Editing.md) | Common Editing Tasks |
| Edit images | [Image Editing](Image-Editing.md) | Common Image Editing Tasks |
| Copy file URLs | [Quick Start](Quick-Start.md) | Copy a File URL |
| Move/copy files | [README](README.md) | Moving Files and Folders |
| Delete files | [README](README.md) | Deleting Files and Folders |
| Rename items | [README](README.md) | Renaming Files or Folders |

## Getting Started

### First Time Users

1. **Start with the [Quick Start Guide](Quick-Start.md)**
   - Learn the basics in just a few minutes
   - Practice with simple operations
   - Get comfortable with the interface

2. **Try basic operations**
   - Upload a file
   - Create a folder
   - Edit a simple text file

3. **Explore specific features**
   - [Code Editing Guide](Code-Editing.md) for code files
   - [Image Editing Guide](Image-Editing.md) for images

4. **Reference the complete guide as needed**
   - [README](README.md) has all the details
   - Use it as a reference when you need specific information

### Experienced Users

- Jump directly to [Code Editing Guide](Code-Editing.md) or [Image Editing Guide](Image-Editing.md)
- Check out the Advanced Features section in [README](README.md)
- Review keyboard shortcuts for efficiency

## User Roles and Permissions

The File Manager is available to users with these roles:

| Role | Permissions |
|------|-------------|
| **Administrators** | Full access to all features |
| **Editors** | Full access to all features |
| **Authors** | Full access to all features |
| **Team Members** | Full access to all features |
| **Reviewers** | Read-only access |

> **Note:** Reviewers can view files but cannot upload, edit, delete, or move them.

## File Storage Structure

All publicly accessible files must be stored under the `/pub` directory:

```
/pub
  /articles
    /{article-number}
      /images
      /scripts
      /styles
  /templates
    /{template-id}
      /assets
  /images
  /css
  /js
  /uploads
```

- **`/pub/articles/`** - Article-specific files
- **`/pub/templates/`** - Template assets
- **`/pub/images/`** - Shared images
- **`/pub/css/`** - Global stylesheets
- **`/pub/js/`** - Global JavaScript files

## Tips for Success

### Organization
1. **Use logical folder structures** - Group related files together
2. **Follow naming conventions** - Consistent names make files easier to find
3. **Clean up regularly** - Delete unused files to keep things tidy

### Performance
1. **Optimize images** - Resize and compress before uploading
2. **Minimize file sizes** - Smaller files load faster
3. **Use appropriate formats** - Choose the right file type for your content

### Security
1. **Don't upload sensitive data** - Files in `/pub` are publicly accessible
2. **Keep backups** - Download important files locally
3. **Monitor changes** - Review file modifications regularly

## Troubleshooting

Having issues? Check these resources:

- **[README - Troubleshooting Section](README.md#troubleshooting)** - General File Manager issues
- **[Code Editing - Troubleshooting](Code-Editing.md#troubleshooting)** - Editor-specific problems
- **[Image Editing - Troubleshooting](Image-Editing.md#troubleshooting)** - Image editor issues

### Common Issues

**Can't upload files:**
- Check file size (max 25MB for simple upload)
- Verify you're in the `/pub` directory or subdirectory
- Ensure you have proper permissions

**Editor won't open:**
- Clear browser cache
- Try a different browser
- Check internet connection

**Changes not appearing:**
- Clear browser cache
- Wait for CDN cache to clear (if applicable)
- Verify the file was actually saved

## Additional Resources

### SkyCMS Documentation
- [Main Documentation](../README.md)
- [Editor Documentation](../Editors/)
- [Template Documentation](../Templates/)
- [Layout Documentation](../Layouts/)

### External Resources
- [Monaco Editor Documentation](https://microsoft.github.io/monaco-editor/)
- [Filerobot Image Editor](https://scaleflex.github.io/filerobot-image-editor/)
- [Emmet Cheat Sheet](https://docs.emmet.io/cheat-sheet/)

## Feedback and Support

### Found an Issue?
If you encounter bugs or have suggestions:
1. Check the troubleshooting sections
2. Search existing GitHub issues
3. [Report a new issue](https://github.com/CWALabs/SkyCMS/issues)

### Need Help?
- Review this documentation thoroughly
- Contact your system administrator
- Consult the SkyCMS community

## Contributing

This documentation is part of the open-source SkyCMS project. Contributions are welcome!

- [SkyCMS GitHub Repository](https://github.com/CWALabs/SkyCMS)
- Fork, improve, and submit pull requests
- Help improve documentation for all users

---

**Version Information:**
- Last Updated: 2025
- SkyCMS Version: Current
- Documentation Maintained By: SkyCMS Community

For more information about SkyCMS, visit the [project homepage](https://github.com/CWALabs/SkyCMS).

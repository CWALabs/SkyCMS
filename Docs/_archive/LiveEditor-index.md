
# SkyCMS Live Editor Documentation

Welcome to the SkyCMS Live Editor documentation. This collection of guides will help you understand and use the Live Editor effectively, whether you're a content editor, administrator, or developer.

## Documentation Structure

### For End Users

#### [Complete User Guide](README.md)
The comprehensive guide to using the Live Editor. Covers all features, tools, and workflows in detail.

**Topics include:**
- Introduction and key features
- Getting started and system requirements
- Understanding the interface
- Content editing techniques
- Using toolbars and formatting options
- Advanced features (images, tables, media, code editing)
- Saving and version control
- Keyboard shortcuts
- Tips and best practices
- Troubleshooting common issues

**Best for:** Content editors, authors, and anyone who will be creating or editing web pages.

#### [Quick Start Guide](QuickStart.md)
Get up and running in 5 minutes with this condensed guide.

**Topics include:**
- Access and login
- Basic editing workflow
- Common formatting tasks
- Essential keyboard shortcuts
- Quick reference for common problems

**Best for:** New users who need to start editing quickly, or experienced users who need a quick reference.

#### [Visual Guide](VisualGuide.md)
Visual reference showing the Live Editor's interface components and layouts.

**Topics include:**
- Interface layout diagrams
- Toolbar components with visual representations
- Editable region indicators
- Modal dialogs
- Save status indicators
- Step-by-step visual workflows

**Best for:** Visual learners who want to see what interface elements look like and how they work.

### For Developers and Administrators

#### [Technical Reference](TechnicalReference.md)
Detailed technical documentation for developers and system administrators.

**Topics include:**
- Architecture and technology stack
- Server-side and client-side components
- CKEditor configuration
- Custom plugin development
- Data flow and real-time collaboration
- Security and encryption
- Data models and database schema
- Performance considerations
- API endpoints
- Debugging and testing
- Deployment notes

**Best for:** Developers, system administrators, and technical staff who need to customize, maintain, or troubleshoot the system.

## What is the Live Editor?

The **SkyCMS Live Editor** is an inline, WYSIWYG (What You See Is What You Get) content editor built on CKEditor 5's Balloon Block Editor. It allows users to edit web pages directly in their published context, making content management intuitive and efficient.

### Key Features

- **Inline Editing**: Edit content directly on the page without switching views
- **Balloon Toolbar**: Context-sensitive toolbar that appears only when needed
- **Real-time Collaboration**: See when others are editing and avoid conflicts
- **Auto-save**: Changes are saved automatically as you work
- **Rich Media**: Insert and manage images, videos, tables, and more
- **Custom Plugins**: SkyCMS-specific tools for page linking and file management
- **Code Editor**: Switch to Monaco editor for advanced HTML editing
- **Version Control**: Track changes and restore previous versions

## Quick Links

### Getting Started
- [How to access the Live Editor](README.md#accessing-the-live-editor)
- [Understanding the interface](README.md#understanding-the-live-editor-interface)
- [First steps](QuickStart.md#getting-started-in-5-minutes)

### Common Tasks
- [Text formatting](README.md#text-formatting)
- [Inserting images](README.md#image-insertion-and-management)
- [Creating tables](README.md#table-management)
- [Adding links](README.md#working-with-links)
- [Embedding videos](README.md#media-embedding)

### Advanced Features
- [Code editing](README.md#vs-code-editor-integration)
- [Real-time collaboration](TechnicalReference.md#real-time-collaboration)
- [Custom plugins](TechnicalReference.md#custom-ckeditor-plugins)

### Administration
- [Security and permissions](TechnicalReference.md#security)
- [Customization](TechnicalReference.md#customization)
- [Deployment](TechnicalReference.md#deployment-notes)

## Technology

The Live Editor is built on:

- **CKEditor 5** - Modern, feature-rich text editor
  - Configuration: Balloon Block Editor
  - Version: 5.44.1
  - License: GPL

- **Monaco Editor** - Advanced code editor (VS Code engine)
  - Used for HTML/JavaScript editing
  - Syntax highlighting and IntelliSense

- **SignalR** - Real-time web functionality
  - Enables collaborative editing
  - Provides live updates

- **ASP.NET Core MVC** - Server framework
  - Razor views
  - Entity Framework Core

## Browser Compatibility

The Live Editor works with modern browsers:

- ✅ Chrome (latest)
- ✅ Firefox (latest)
- ✅ Edge (latest)
- ✅ Safari (latest)

**Requirements:**
- JavaScript must be enabled
- Cookies must be enabled (for authentication)
- WebSocket support (for real-time features)

## User Roles

Access to the Live Editor is controlled by user roles:

| Role | Access Level |
|------|-------------|
| **Administrators** | Full access - edit, publish, manage permissions |
| **Editors** | Edit and publish pages |
| **Authors** | Edit pages, cannot publish |
| **Team Members** | Edit pages |
| **Reviewers** | View only (no direct editing) |

## Support and Resources

### Documentation
- [Full User Guide](README.md) - Complete documentation
- [Quick Start](QuickStart.md) - Get started fast
- [Technical Reference](TechnicalReference.md) - Developer documentation

### External Resources
- [CKEditor 5 Documentation](https://ckeditor.com/docs/ckeditor5/latest/)
- [CKEditor 5 Balloon Block Editor](https://ckeditor.com/docs/ckeditor5/latest/examples/builds/balloon-block-editor.html)
- [Monaco Editor](https://microsoft.github.io/monaco-editor/)

### Getting Help

1. **Check the documentation** - Most questions are answered here
2. **Review troubleshooting guides** - See [Troubleshooting](README.md#troubleshooting)
3. **Check browser console** - Press F12 to see error messages
4. **Contact your administrator** - For permission or technical issues
5. **Review server logs** - Administrators can check `/Editor/Logs`

## Recent Updates

*October 2025*
- Initial documentation created
- Based on CKEditor 5.44.1
- Includes custom SkyCMS plugins

## Contributing

If you're a developer working on SkyCMS:

1. Review the [Technical Reference](TechnicalReference.md)
2. Understand the [architecture and data flow](TechnicalReference.md#architecture)
3. Follow [customization guidelines](TechnicalReference.md#customization)
4. Test thoroughly before deploying

## License

The Live Editor uses CKEditor 5 under the GPL license. See [CKEditor 5 Licensing](https://ckeditor.com/legal/ckeditor-oss-license) for details.

SkyCMS is licensed under the GNU Public License, Version 3.0.

---

## Document Index

- **[README.md](README.md)** - Complete User Guide (end users)
- **[QuickStart.md](QuickStart.md)** - Quick Start Guide (new users)
- **[VisualGuide.md](VisualGuide.md)** - Visual Guide (interface reference)
- **[TechnicalReference.md](TechnicalReference.md)** - Technical Reference (developers)
- **index.md** - This document (overview and navigation)

---

*Last Updated: October 2025*  
*SkyCMS Live Editor Documentation v1.0*


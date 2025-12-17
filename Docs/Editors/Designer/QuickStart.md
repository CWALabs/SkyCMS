---
title: Design Editor Quick Start Guide
description: Quick reference for getting started with the Design Editor (GrapesJS) in SkyCMS
keywords: design-editor, GrapesJS, quick-start, visual-editor
audience: [content-creators]
---

# Design Editor Quick Start Guide

This is a quick reference guide to get you started with the Design Editor (GrapesJS) in SkyCMS. For complete documentation, see [README.md](README.md).

## What is the Design Editor?

A visual drag-and-drop web page builder that lets you create layouts, templates, and articles without writing code.

## Where to Access

- **Layouts**: Layouts → Designer
- **Templates**: Templates → Designer  
- **Articles**: Editor → Designer

## 5-Minute Quick Start

### Creating Your First Layout

1. Navigate to **Layouts** → **Designer**
2. Drag a **Navbar** block into the header area
3. Drag a **Footer** section into the footer area
4. Style elements using the **Style Manager** on the right
5. Press **Ctrl+S** to save
6. Click **Publish** to make it active

### Creating Your First Template

1. Navigate to **Templates** → **Designer**
2. Drag a **Section** block onto the canvas
3. Drag a **Heading** and **Text** block into the section
4. Style your content
5. Press **Ctrl+S** to save
6. Name your template

### Creating Your First Article

1. Navigate to **Editor** → **Designer**
2. Drag content blocks into the canvas
3. Add text, images, and other components
4. Style as needed
5. Press **Ctrl+S** to save
6. Click **Publish** when ready

## Essential Panels

### Blocks Panel (Left)
Pre-built components you can drag onto the page:
- Text, images, videos
- Forms and buttons
- Sections and containers
- Framework-specific components (Bootstrap, etc.)

### Style Manager (Right)
Visual styling controls for:
- Dimensions (width, height, margins, padding)
- Typography (fonts, sizes, colors)
- Backgrounds and borders
- Flexbox layout
- Responsive properties

### Layer Manager (Right)
Tree view of all page elements:
- Click to select elements
- Drag to reorder
- Show/hide elements
- Lock/unlock elements

### Asset Manager (Right)
Media management:
- Upload images and files
- Browse existing assets
- Drag images onto canvas
- Auto-loads from `/pub` directories

## Common Tasks

### Add Text
1. Drag **Text** block from Blocks panel
2. Double-click to edit content
3. Style using Style Manager

### Add Image
1. Drag **Image** block from Blocks panel
2. Click the image placeholder
3. Select from Asset Manager or upload new
4. Adjust size via Style Manager

### Create Two Columns
1. Drag **Section** block
2. Drag two **Container** blocks inside
3. Set each container to 50% width
4. Add content to each container

### Add a Button
1. Drag **Link** or **Button** block
2. Enter button text
3. Set href in Traits panel
4. Style in Style Manager

### Make Responsive
1. Click device icons in toolbar (Desktop, Tablet, Mobile)
2. Adjust styles for each breakpoint
3. Test how design adapts

## Keyboard Shortcuts

| Action | Windows/Linux | Mac |
|--------|---------------|-----|
| Save | Ctrl+S | Cmd+S |
| Undo | Ctrl+Z | Cmd+Z |
| Redo | Ctrl+Shift+Z | Cmd+Shift+Z |
| Delete | Delete | Delete |
| Copy | Ctrl+C | Cmd+C |
| Paste | Ctrl+V | Cmd+V |
| Cut | Ctrl+X | Cmd+X |

## Tips for Beginners

### 1. Start Simple
Don't try to create complex designs immediately. Start with:
- Basic sections and containers
- Simple text and images
- Standard layouts

### 2. Use Pre-built Blocks
Take advantage of built-in components:
- Framework blocks (Bootstrap, Tailwind)
- Form components
- Navigation elements

### 3. Learn the Layer Manager
The Layer Manager is your friend:
- Use it to select nested elements
- Organize your page structure
- Rename elements for clarity

### 4. Save Frequently
- Use Ctrl+S regularly
- Auto-save triggers after you stop editing
- Don't navigate away immediately after changes

### 5. Test Responsive Design
Always check how your design looks on:
- Desktop
- Tablet
- Mobile portrait
- Mobile landscape

### 6. Use the Style Manager
Most styling can be done visually:
- No need to write CSS
- Real-time preview
- Organized by category

## Common Pitfalls to Avoid

### Don't:
- Nest editable regions inside editable regions
- Use excessive inline styles
- Create overly complex nested structures
- Forget to test on mobile devices
- Navigate away without saving

### Do:
- Use semantic HTML elements
- Add CSS classes for reusable styles
- Keep structure organized in Layer Manager
- Test responsive behavior
- Save your work frequently
- Use framework components when available

## Framework Support

### Bootstrap 5/4
If your layout includes Bootstrap:
- Bootstrap blocks automatically appear
- Use grid system components
- Pre-styled cards, buttons, navbars available

### Tailwind CSS
If your layout includes Tailwind:
- Tailwind blocks automatically appear
- Use utility classes via Traits panel
- Apply responsive utilities

### No Framework
Default blocks include:
- Basic HTML elements
- Flexible grid system
- Form components
- Custom code blocks

## Getting Help

### In the Interface
- Hover tooltips on buttons
- Component settings in right panel
- Layer Manager for structure view

### Documentation
- Full documentation: [README.md](README.md)
- Code Editor docs: [../CodeEditor/README.md](../CodeEditor/README.md)
- Official GrapesJS docs: [https://grapesjs.com/docs/](https://grapesjs.com/docs/)

### Community
- SkyCMS GitHub: [https://github.com/CWALabs/SkyCMS](https://github.com/CWALabs/SkyCMS)
- Video tutorials: YouTube playlist (coming soon)
- Slack channel: [Sky-cms.slack.com](https://Sky-cms.slack.com/)

## Next Steps

Once you're comfortable with the basics:

1. **Explore Advanced Components**
   - Custom code blocks
   - Image editor integration
   - Countdown timers
   - Tabs and accordions

2. **Learn Flexbox/Grid**
   - Use the Flex panel in Style Manager
   - Create flexible layouts
   - Master responsive design

3. **Integrate with Code Editor**
   - Switch between visual and code editing
   - Add custom CSS classes
   - Insert JavaScript functionality

4. **Create Reusable Templates**
   - Design templates for different content types
   - Use consistent layouts
   - Build a template library

5. **Master Responsive Design**
   - Design mobile-first
   - Use relative units (%, rem, em)
   - Test all breakpoints

## Cheat Sheet

### Basic Workflow
```
1. Plan → 2. Structure → 3. Content → 4. Style → 5. Test → 6. Save
```

### Essential Blocks
- **Section** - Page sections
- **Container** - Content containers
- **Text** - Text content
- **Image** - Images
- **Link/Button** - Interactive elements
- **Form** - Forms and inputs

### Most Used Style Properties
- Width/Height
- Margin/Padding
- Font size/family
- Colors (text, background)
- Flexbox (display: flex)
- Position

### Device Breakpoints
- Desktop: 1200px+
- Tablet: 768px - 1199px
- Mobile: 320px - 767px

## Troubleshooting Quick Fixes

| Problem | Solution |
|---------|----------|
| Can't select element | Use Layer Manager |
| Styles not applying | Check specificity, use !important if needed |
| Changes not saving | Press Ctrl+S, wait for confirmation |
| Image not appearing | Check Asset Manager, verify file path |
| Layout breaking on mobile | Set responsive widths, test breakpoints |
| Component disappeared | Press Ctrl+Z to undo |

## Ready to Create!

You now have the essentials to start creating with the Design Editor. Remember:

- **Experiment** - Try different blocks and styles
- **Save often** - Don't lose your work
- **Test responsive** - Check all devices
- **Use resources** - Documentation and community are here to help

For detailed information on any topic, refer to the [complete documentation](README.md).

Happy designing! 🎨

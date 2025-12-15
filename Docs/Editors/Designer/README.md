
# Design Editor (GrapesJS)

The Design Editor in SkyCMS is a powerful visual drag-and-drop editor built on [GrapesJS](https://grapesjs.com/), an open-source web builder framework. This editor allows you to create and design professional web layouts, page templates, and article content without writing code, making it accessible to both technical and non-technical users.

## What is GrapesJS?

[GrapesJS](https://grapesjs.com/) is a modern, open-source web builder framework that provides a complete solution for building HTML templates through an intuitive drag-and-drop interface. It combines:

- **Visual Canvas** - WYSIWYG editing with real-time preview
- **Component Blocks** - Pre-built elements you can drag onto the page
- **Style Manager** - Visual controls for CSS styling
- **Layer Manager** - Hierarchical view of page structure
- **Asset Manager** - Built-in media management

## Where to Use the Design Editor

The Design Editor is available in three main areas of SkyCMS:

### 1. Layouts
Access via: **Layouts → Designer**

Create and edit site-wide layouts that define the overall structure of your website, including:
- Header content
- Footer content
- Navigation bars
- Global styling

The Layout Designer allows you to design the header and footer sections that appear on every page, while leaving space for page-specific content in the middle.

### 2. Page Templates
Access via: **Templates → Designer**

Design reusable page templates that define the structure and initial content for new pages:
- Landing pages
- Blog post layouts
- Contact forms
- Product pages

Templates inherit the active layout and allow you to design the main content area.

### 3. Articles
Access via: **Editor → Designer**

Create and edit individual article/page content using the visual designer:
- Blog posts
- News articles
- Custom pages
- Marketing content

Articles use the selected template and layout as a framework.

## Key Features

### 1. Drag-and-Drop Interface

The Design Editor provides an intuitive canvas where you can:
- Drag components from the blocks panel
- Drop them anywhere on the page
- Rearrange elements by dragging
- Resize elements visually

No coding required—simply drag, drop, and adjust to create your design.

### 2. Pre-Built Component Blocks

The editor includes extensive libraries of ready-to-use components:

#### Basic Blocks
- **Text** - Paragraphs, headings, and text blocks
- **Image** - Responsive images with styling
- **Video** - Embedded video players
- **Map** - Google Maps integration
- **Link** - Hyperlinks and buttons
- **Sections** - Container elements for layout structure

#### Form Components
- **Input fields** - Text, email, password, number inputs
- **Textarea** - Multi-line text input
- **Select** - Dropdown menus
- **Checkbox** - Multiple choice options
- **Radio buttons** - Single choice options
- **Buttons** - Submit, reset, and action buttons
- **Form** - Complete form containers

#### Advanced Components
- **Custom Code** - Insert custom HTML/CSS/JavaScript
- **Countdown Timer** - Dynamic countdown displays
- **Typed Text** - Animated typing effects
- **Tabs** - Tabbed content sections
- **Navbar** - Navigation menu components
- **Rich Text Editor** - CKEditor integration for formatted text

#### Framework-Specific Blocks

The Design Editor automatically detects which CSS framework your layout uses and loads appropriate component libraries:

**Bootstrap 5 Blocks** (if Bootstrap 5 detected):
- Grid system components
- Cards and panels
- Alerts and badges
- Modal dialogs
- Carousels
- Navbars and navigation

**Bootstrap 4 Blocks** (if Bootstrap 4 detected):
- Legacy Bootstrap 4 components
- Compatible grid system
- Bootstrap 4 styling

**Tailwind CSS Blocks** (if Tailwind detected):
- Utility-first components
- Tailwind-styled elements

**Basic Blocks** (default):
- Framework-agnostic components
- Flexible grid system
- Universal form components

### 3. Style Manager

The Style Manager provides visual controls for styling elements without writing CSS:

#### General Properties
- **Float** - Left, right, or none
- **Display** - Block, inline, flex, grid, etc.
- **Position** - Static, relative, absolute, fixed
- **Position offsets** - Top, right, bottom, left values

#### Dimensions
- **Width/Height** - Pixel or percentage values
- **Min/Max dimensions** - Minimum and maximum size constraints
- **Margin** - Outer spacing around elements
- **Padding** - Inner spacing within elements
- **Flex width** - Flexible box sizing for flex containers

#### Typography
- **Font family** - Choose from available fonts
- **Font size** - Text size with units
- **Font weight** - Light, normal, bold, etc.
- **Letter spacing** - Space between characters
- **Text color** - Text color picker
- **Line height** - Vertical spacing between lines
- **Text align** - Left, center, right, justify
- **Text decoration** - Underline, strikethrough
- **Text shadow** - Shadow effects on text

#### Decorations
- **Opacity** - Element transparency (0-100%)
- **Border radius** - Rounded corners
- **Border** - Border width, style, and color
- **Box shadow** - Drop shadow effects
- **Background** - Colors, gradients, and images

#### Advanced Styling
- **Transitions** - Smooth animations between states
- **Perspective** - 3D transformation perspective
- **Transform** - Rotate, scale, skew, translate

#### Flexbox Controls
- **Flex container** - Enable/disable flex layout
- **Flex direction** - Row, column, reverse
- **Justify content** - Main axis alignment
- **Align items** - Cross axis alignment
- **Flex wrap** - Enable wrapping of flex items

### 4. Layer Manager

The Layer Manager shows a hierarchical tree view of all components on your page:

- **Visual hierarchy** - See parent-child relationships
- **Click to select** - Select any element from the tree
- **Drag to reorder** - Rearrange elements in the structure
- **Eye icon** - Show/hide elements temporarily
- **Lock icon** - Prevent accidental editing

Use the Layer Manager when:
- You need to select deeply nested elements
- The visual canvas is too cluttered
- You want to understand the page structure
- You need to reorder elements precisely

### 5. Asset Manager

The Asset Manager provides integrated media management:

#### Features
- **Upload files** - Drag and drop or browse to upload
- **Image library** - Browse all uploaded images
- **Auto-loading** - Automatically loads images from `/pub` and `/pub/articles`
- **Drag to use** - Drag images from the manager onto the canvas
- **Image preview** - Thumbnail previews of all assets

#### Supported Upload Types
- Images (PNG, JPG, GIF, WebP, SVG)
- Videos (MP4, WebM, OGV)
- Documents (PDF)

The Asset Manager integrates with SkyCMS's file storage system, so all uploaded files are automatically available across your entire site.

### 6. Component Settings Panel

When you select any component, the settings panel appears on the right side, showing:

#### General Tab
- Component-specific properties
- Content settings
- Attributes and IDs
- CSS classes

#### Style Tab
- All Style Manager options
- Visual property editors
- Real-time preview

#### Traits Tab
- HTML attributes (ID, class, title, etc.)
- Data attributes
- Custom properties
- ARIA attributes for accessibility

### 7. Responsive Design Tools

The Design Editor includes built-in responsive design capabilities:

#### Device Preview Modes
- **Desktop** - Full-width view (default)
- **Tablet** - Medium screen preview
- **Mobile Portrait** - Phone vertical view
- **Mobile Landscape** - Phone horizontal view

Switch between device modes using the toolbar buttons at the top of the canvas to see how your design adapts to different screen sizes.

#### Responsive Styling
- Apply different styles for different devices
- Create responsive breakpoints
- Test mobile-first or desktop-first approaches
- Hide/show elements based on screen size

### 8. Import/Export Templates

The Design Editor supports importing and exporting HTML/CSS:

#### Import Template
1. Click the **Import** button in the toolbar
2. Paste your HTML and CSS code
3. Click **Import** to load the template
4. The editor parses and displays your content

Use this to:
- Import designs from other tools
- Migrate existing HTML pages
- Start from HTML templates
- Restore previous versions

#### Export Template
1. Click the **Export** button in the toolbar
2. Choose your export format:
   - **HTML + CSS** - Complete webpage code
   - **HTML only** - Just the markup
   - **CSS only** - Just the styles
3. Copy the generated code

Use this to:
- Export designs to other platforms
- Create backups of your work
- Generate code for external use
- Share templates with others

### 9. Custom Code Blocks

For advanced users, the Design Editor includes a Custom Code block that allows you to:

- Insert raw HTML, CSS, and JavaScript
- Embed third-party widgets (social media, analytics, etc.)
- Add custom functionality
- Include advanced interactions

The custom code is preserved exactly as entered and integrated seamlessly with visual components.

### 10. Image Editing Integration

The Design Editor integrates with **Toast UI Image Editor** for in-editor image manipulation:

#### Available Tools
- **Crop** - Crop images to specific dimensions
- **Flip** - Flip horizontally or vertically
- **Rotate** - Rotate images by any angle
- **Draw** - Add free-hand drawings
- **Shape** - Add geometric shapes
- **Icon** - Insert icon overlays
- **Text** - Add text overlays
- **Mask** - Apply image masks
- **Filter** - Apply photo filters (grayscale, sepia, blur, sharpen, etc.)

To edit an image:
1. Select any image component
2. Click the **Edit Image** button in the toolbar
3. Make your edits in the image editor
4. Click **Apply** to save changes

### 11. Real-Time Preview

The Design Editor provides real-time WYSIWYG (What You See Is What You Get) preview:

- **Live editing** - See changes instantly as you work
- **Canvas scripts** - JavaScript and CSS from your layout execute in the canvas
- **Framework support** - Bootstrap, Tailwind, and other frameworks render correctly
- **Font loading** - Custom fonts display properly
- **Responsive preview** - Switch devices to see responsive behavior

The canvas loads all stylesheets and scripts defined in your layout's HEAD section, ensuring accurate preview.

## Getting Started with the Design Editor

### For Layouts

1. Navigate to **Layouts** in the main menu
2. Either:
   - Click **Create New Layout** to start fresh
   - Select an existing layout and click **Designer**
3. The Design Editor opens with editable header and footer sections
4. Design your header and footer content
5. Click **Save** (or press Ctrl+S) to save changes
6. Click **Publish** to make it the active layout

**Note**: When editing layouts, the middle section shows a placeholder for page content and cannot be edited—this space is reserved for templates and articles.

### For Page Templates

1. Navigate to **Templates** in the main menu
2. Either:
   - Click **Create New Template** to start fresh
   - Select an existing template and click **Designer**
3. The Design Editor opens with the active layout's header/footer and an editable content area
4. Drag components into the content area to build your template
5. Style components using the Style Manager
6. Click **Save** to preserve your template
7. Use this template when creating new articles

### For Articles

1. Navigate to **Editor** in the main menu
2. Either:
   - Click **Create New Article**
   - Select an existing article and click **Designer**
3. The Design Editor opens with the article's template and layout
4. Edit the content area to create your article
5. Add text, images, forms, and other components
6. Style as needed
7. Click **Save** to save as a draft
8. Click **Publish** to make the article live

## Working with the Design Editor

### Basic Workflow

1. **Plan your layout** - Sketch or outline your design before starting
2. **Add structure** - Drag section and container blocks to create layout structure
3. **Add content** - Drag content blocks (text, images, etc.) into sections
4. **Style components** - Use the Style Manager to adjust appearance
5. **Refine structure** - Use the Layer Manager to organize elements
6. **Test responsive** - Switch device modes to verify responsive behavior
7. **Save frequently** - Use auto-save or manual save (Ctrl+S)
8. **Preview** - Click the preview button to see the final result

### Best Practices

#### 1. Start with Structure
- Begin with containers and sections before adding content
- Use proper semantic HTML (headers, sections, articles)
- Create a clear hierarchy of elements

#### 2. Use Classes for Styling
- Add CSS classes to components for easier styling
- Reuse classes for consistent styling across similar elements
- Use framework classes (Bootstrap, Tailwind) when available

#### 3. Organize Layers
- Give components meaningful names in the Layer Manager
- Group related elements in containers
- Keep the layer tree organized and logical

#### 4. Keep It Simple
- Don't over-complicate designs with too many nested elements
- Use built-in components when possible
- Test frequently during design

#### 5. Responsive Design
- Design for mobile first, then scale up
- Test all device breakpoints
- Use percentage widths for fluid layouts
- Consider touch targets for mobile

#### 6. Performance
- Optimize images before uploading
- Minimize the number of components when possible
- Avoid excessive nesting of elements
- Use efficient CSS rather than inline styles

#### 7. Accessibility
- Use semantic HTML elements
- Add alt text to images
- Ensure sufficient color contrast
- Include ARIA labels where appropriate
- Test keyboard navigation

### Keyboard Shortcuts

The Design Editor supports keyboard shortcuts for efficiency:

#### Editing
- **Ctrl+S** (Cmd+S on Mac) - Save changes
- **Ctrl+Z** (Cmd+Z) - Undo last action
- **Ctrl+Shift+Z** (Cmd+Shift+Z) - Redo action
- **Delete** - Delete selected component
- **Ctrl+C** (Cmd+C) - Copy selected component
- **Ctrl+V** (Cmd+V) - Paste component
- **Ctrl+X** (Cmd+X) - Cut component

#### Navigation
- **Arrow keys** - Move selected component
- **Tab** - Navigate between components
- **Esc** - Deselect component

#### Canvas Control
- **Ctrl+Alt+0** - Full-screen preview
- **Mouse wheel** - Zoom canvas (hold Ctrl)

### Common Tasks

#### Creating a Two-Column Layout

1. Drag a **Section** block onto the canvas
2. Inside the section, drag two **Container** blocks side-by-side
3. Adjust the width of each container to 50%
4. Add content to each container
5. Use the Style Manager to add padding and spacing

#### Adding a Hero Section

1. Drag a **Section** block onto the canvas
2. Set the section height (e.g., 500px)
3. Add a background image via Style Manager → Decorations → Background
4. Drag a **Heading** block into the section
5. Drag a **Text** block for description
6. Drag a **Button** block for call-to-action
7. Center-align content using flexbox or text-align

#### Creating a Contact Form

1. Drag a **Form** block onto the canvas
2. Inside the form, drag:
   - **Input** blocks for name and email
   - **Textarea** for message
   - **Button** for submit
3. Set input placeholders via the Traits panel
4. Add required attributes to inputs
5. Set form action URL in the form traits
6. Style form elements using the Style Manager

#### Adding Navigation

1. Drag a **Navbar** block onto the canvas (if using Bootstrap)
2. Or manually create:
   - A container with flexbox enabled
   - Multiple **Link** blocks inside
3. Style the navbar:
   - Background color
   - Link colors and hover states
   - Padding and spacing
4. Make it sticky: Set position to fixed and top to 0

#### Embedding Custom HTML

1. Drag a **Custom Code** block onto the canvas
2. Click on the block to open the code editor
3. Enter your HTML, CSS, or JavaScript
4. Click **Save** to apply
5. The custom code integrates with the page

#### Responsive Image Gallery

1. Create a **Section** block
2. Enable flexbox on the section
3. Set flex-wrap to wrap
4. Add multiple **Image** blocks
5. Set each image width to 33.33% (for 3 columns)
6. Add media queries:
   - Tablet: 50% width (2 columns)
   - Mobile: 100% width (1 column)

## Framework Integration

### Bootstrap 5 / Bootstrap 4

When the Design Editor detects Bootstrap in your layout, it automatically loads Bootstrap-specific components:

- Pre-built Bootstrap cards
- Grid system components
- Navigation bars
- Button groups
- Modal dialogs
- Accordion components
- Carousels
- Alerts and badges

These components include Bootstrap's CSS classes and behavior out-of-the-box.

**To use Bootstrap components:**
1. Ensure your layout includes Bootstrap CSS and JS in the HEAD
2. The Design Editor automatically detects Bootstrap
3. Bootstrap blocks appear in the blocks panel
4. Drag and drop Bootstrap components as needed

### Tailwind CSS

When Tailwind CSS is detected, the Design Editor loads Tailwind-specific blocks:

- Utility-first components
- Tailwind-styled buttons and forms
- Pre-configured spacing and typography

**To use Tailwind:**
1. Include Tailwind CSS in your layout HEAD
2. The Design Editor detects the Tailwind CDN
3. Tailwind blocks become available
4. Apply utility classes via the Traits panel

### Custom Frameworks

If you're using a custom CSS framework:

1. Include your framework's CSS in the layout HEAD
2. Use the Custom Code block to add framework-specific HTML
3. Add framework classes via the Traits panel
4. Use the Style Manager for additional styling

## Understanding Editable Regions

SkyCMS uses special HTML comments to define editable regions:

```html
<!--CCMS--START--EDITABLE--REGION-->
Your editable content here
<!--CCMS--END--EDITABLE--REGION-->
```

### In Layouts
- **Header section**: Between `<!--CCMS--START--HEADER-->` and `<!--CCMS--END--HEADER-->`
- **Footer section**: Between `<!--CCMS--START--FOOTER-->` and `<!--CCMS--END--FOOTER-->`
- **Content area**: Reserved for templates and articles (not editable in layout designer)

### In Templates and Articles
- **Main content**: The entire body is editable
- **Multiple regions**: You can create multiple editable regions
- **Nested regions**: Cannot have editable regions inside editable regions (validation prevents this)

The Design Editor automatically handles these markers, so you don't need to add them manually. However, understanding them helps when troubleshooting or working with the Code Editor.

## Auto-Save Feature

The Design Editor includes intelligent auto-save:

- **Automatic saving** - Changes are saved automatically every few seconds after you stop editing
- **Save indicator** - A "Saving..." message appears during auto-save
- **Manual save** - You can also press Ctrl+S to force save immediately
- **Version tracking** - Each save creates a new version (for layouts)

**Important**: Auto-save only triggers after you stop interacting with the editor. If you make changes and immediately navigate away, your changes might not be saved. Always wait for the "Saved" confirmation or manually press Ctrl+S.

## Troubleshooting

### Components not appearing on canvas
- **Check console** - Press F12 and look for JavaScript errors
- **Refresh the page** - Sometimes a refresh resolves loading issues
- **Clear browser cache** - Cached files might be outdated
- **Check layout HEAD** - Ensure required CSS/JS files are loaded

### Styles not applying
- **Check specificity** - More specific CSS rules override less specific ones
- **Inspect element** - Use browser DevTools to see applied styles
- **Framework conflicts** - Framework CSS might override your custom styles
- **Clear inline styles** - Remove conflicting inline styles from the Traits panel

### Can't select an element
- **Use Layer Manager** - Select the element from the layer tree instead
- **Check for overlays** - Another element might be positioned on top
- **Unlock layers** - Ensure the element isn't locked in the Layer Manager
- **Check Z-index** - Elements with lower z-index might be behind others

### Changes not saving
- **Wait for auto-save** - Don't navigate away immediately after editing
- **Manual save** - Press Ctrl+S to force save
- **Check permissions** - Ensure you have edit permissions
- **Browser console** - Check for error messages
- **Network issues** - Verify your internet connection

### Responsive preview not working
- **Check breakpoints** - Ensure breakpoints are defined in your CSS
- **Use percentage widths** - Fixed pixel widths don't adapt
- **Test media queries** - Verify media queries are correctly written
- **Framework responsive** - If using Bootstrap/Tailwind, use their responsive classes

### Images not loading in Asset Manager
- **Check file path** - Verify the `/pub` directory exists and is accessible
- **Permissions** - Ensure the application has read permissions on the storage
- **File size** - Very large files might fail to load
- **Supported formats** - Only supported image formats appear (PNG, JPG, GIF, WebP, SVG)

## Advanced Topics

### Using Custom Fonts

To use custom fonts in the Design Editor:

1. Add font CSS to your layout HEAD:
   ```html
   <link href="https://fonts.googleapis.com/css2?family=Roboto:wght@400;700&display=swap" rel="stylesheet">
   ```

2. The font becomes available in the Style Manager under Typography → Font Family

3. Apply the font to components via the Style Manager

### Adding Custom CSS

For site-wide custom CSS:

1. Edit the layout using the **Code Editor**
2. Add `<style>` tags in the HEAD section
3. Write your custom CSS
4. Save the layout

For component-specific CSS:

1. Select the component in the Design Editor
2. Open the **Traits** panel
3. Add a unique CSS class to the component
4. Use the Code Editor to add styles for that class

### JavaScript Integration

To add JavaScript functionality:

1. Use the **Custom Code** block for inline scripts
2. Or add `<script>` tags to the layout footer in the Code Editor
3. Reference elements by ID or class
4. Ensure scripts load after the DOM is ready

### Creating Reusable Components

To create reusable design components:

1. Design the component in the Design Editor
2. Select the component in the Layer Manager
3. Copy the component (Ctrl+C)
4. Paste it wherever needed (Ctrl+V)
5. Or save the component HTML using the Export feature
6. Import it into other templates or articles

### Working with Forms

Forms in the Design Editor can:

- **POST to SkyCMS endpoints** - For contact forms and submissions
- **POST to external services** - For newsletter signups, etc.
- **Use AJAX** - For asynchronous form submission
- **Include validation** - Using HTML5 validation attributes

To configure a form:

1. Select the form component
2. Open the **Traits** panel
3. Set the **action** attribute to your form handler URL
4. Set the **method** attribute (GET or POST)
5. Add **name** attributes to all input fields
6. Add **required** attributes for validation

### Integration with CKEditor

For rich text editing within the Design Editor:

1. Drag an **Article Block** component (SkyCMS custom component)
2. This creates an editable region that integrates with CKEditor
3. Double-click the block to edit with CKEditor
4. Rich text content is preserved within the design

This allows mixing visual design with rich text editing.

## Tips for Different Use Cases

### Marketing Landing Pages
- Use hero sections with large images or videos
- Add clear call-to-action buttons
- Include social proof (testimonials, logos)
- Keep forms short and simple
- Optimize for conversion

### Blog Post Templates
- Include featured image section
- Add author bio section
- Include social sharing buttons
- Add related posts section
- Ensure readable typography (line height, font size)

### Corporate Websites
- Professional, clean design
- Consistent branding
- Clear navigation
- Contact information prominent
- Accessibility considerations

### E-commerce Pages
- Product image galleries
- Clear product descriptions
- Prominent "Add to Cart" buttons
- Trust indicators (security badges, reviews)
- Mobile-optimized checkout

### Portfolio Sites
- Visual focus (large images)
- Grid layouts for projects
- Lightbox/modal for detailed views
- Minimal navigation
- Contact form

## Differences from Code Editor

The Design Editor and Code Editor serve different purposes:

### Use the Design Editor when:
- You want visual, drag-and-drop editing
- You're not comfortable with HTML/CSS
- You want to see immediate visual feedback
- You're designing layouts and templates
- You need responsive design tools

### Use the Code Editor when:
- You need precise control over HTML/CSS
- You're adding complex JavaScript functionality
- You're debugging or troubleshooting code
- You need to edit the HEAD section extensively
- You prefer writing code directly

You can switch between editors for the same content. Changes made in one editor are reflected in the other.

## Best Practices for Performance

### Optimize Images
- Compress images before uploading
- Use appropriate image formats (WebP for photos, SVG for icons)
- Specify image dimensions to prevent layout shift
- Use lazy loading for below-the-fold images

### Minimize Components
- Don't use excessive nested elements
- Combine similar styles into reusable classes
- Remove unused components

### Efficient CSS
- Use CSS classes instead of inline styles
- Minimize use of complex selectors
- Use CSS Grid and Flexbox efficiently
- Avoid excessive box-shadows and transitions

### JavaScript Optimization
- Load scripts in the footer when possible
- Use async or defer attributes
- Minimize use of heavy libraries
- Only include necessary functionality

### Framework Efficiency
- Only load framework components you actually use
- Remove unused CSS/JS from frameworks
- Consider custom builds of frameworks
- Use CDN versions for better caching

## Accessibility in the Design Editor

Creating accessible designs:

### Semantic HTML
- Use heading levels properly (H1, H2, H3, etc.)
- Use `<nav>` for navigation
- Use `<article>` for content blocks
- Use `<footer>` for footer content

### ARIA Labels
- Add `aria-label` to interactive elements
- Use `aria-describedby` for form fields
- Add `role` attributes where appropriate
- Use `aria-hidden` for decorative elements

### Color Contrast
- Ensure text has sufficient contrast with background
- Test color combinations using accessibility tools
- Don't rely on color alone to convey information
- Consider colorblind users

### Keyboard Navigation
- Ensure all interactive elements are keyboard accessible
- Test tab order makes sense
- Provide focus indicators
- Add skip links for navigation

### Alt Text
- Add descriptive alt text to all images
- Use empty alt (`alt=""`) for decorative images
- Keep alt text concise but descriptive
- Don't include "image of" in alt text

### Form Accessibility
- Add labels to all form fields
- Group related fields with fieldsets
- Provide helpful error messages
- Indicate required fields clearly

## Integration with Other SkyCMS Features

### File Manager Integration
- The Asset Manager connects to the SkyCMS File Manager
- Uploaded files are stored in the configured storage provider (Azure/S3/Cloudflare R2)
- Files are automatically organized in `/pub` directories
- Use the same files across multiple articles and templates

### Version Control
- For layouts: Each time you publish, a new version is created
- For articles: Versions are tracked automatically
- You can view and restore previous versions
- Version history is maintained in the database

### Template System
- Templates designed in the Design Editor become available for new articles
- Templates inherit the active layout
- Changes to templates don't affect existing articles using that template
- You can create multiple templates for different content types

### Layout System
- Only one layout can be published (active) at a time
- The active layout applies to all pages site-wide
- You can create multiple layout versions
- Switching layouts affects the entire site

### Publishing Workflow
- Layouts must be published to become active
- Articles can be saved as drafts or published
- Templates are always available for use
- Publishing triggers static site regeneration (if using static mode)

## Video Tutorials

For visual learners, SkyCMS provides video tutorials:

- **GrapesJS Overview** - [Watch on YouTube](https://www.youtube.com/watch?v=mVGPlbnbC5c)
- **Creating Layouts** - Video walkthroughs (coming soon)
- **Building Templates** - Video walkthroughs (coming soon)
- **Responsive Design** - Video walkthroughs (coming soon)

## Additional Resources

### Official Documentation
- **GrapesJS Documentation** - [https://grapesjs.com/docs/](https://grapesjs.com/docs/)
- **GrapesJS API Reference** - [https://grapesjs.com/docs/api/](https://grapesjs.com/docs/api/)
- **GrapesJS Plugins** - [https://grapesjs.com/docs/guides/Replace-Rich-Text-Editor.html](https://grapesjs.com/docs/guides/Replace-Rich-Text-Editor.html)

### SkyCMS Resources
- **Code Editor Documentation** - [../CodeEditor/README.md](../CodeEditor/README.md)
- **Live Editor Documentation** - [../LiveEditor/README.md](../LiveEditor/README.md)
- **SkyCMS GitHub** - [https://github.com/CWALabs/SkyCMS](https://github.com/CWALabs/SkyCMS)

### Community
- **SkyCMS Slack** - [Join for community support](https://Sky-cms.slack.com/)
- **GitHub Issues** - [Report bugs or request features](https://github.com/CWALabs/SkyCMS/issues)
- **YouTube Channel** - Video playlist (coming soon)

## Conclusion

The Design Editor (GrapesJS) in SkyCMS provides a professional-grade visual web design experience accessible to users of all skill levels. Whether you're creating layouts, templates, or articles, the drag-and-drop interface combined with powerful styling tools makes it easy to create beautiful, responsive web content without writing code.

For more advanced control, you can always switch to the [Code Editor](../CodeEditor/README.md) to work directly with HTML, CSS, and JavaScript.

Happy designing!

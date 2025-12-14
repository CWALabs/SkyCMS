{% include nav.html %}

# Page Templates Guide

## Overview

Page Templates in SkyCMS allow you to create reusable page structures that can be applied to multiple pages across your website. Templates help maintain consistency in design and layout while speeding up content creation.

## Table of Contents

- [What are Page Templates?](#what-are-page-templates)
- [Creating a Template](#creating-a-template)
- [Editing Templates](#editing-templates)
- [Using Templates](#using-templates)
- [Managing Template-Based Pages](#managing-template-based-pages)
- [Updating Pages with Templates](#updating-pages-with-templates)
- [Best Practices](#best-practices)

## What are Page Templates?

Page Templates are HTML structures that define the layout and design of your pages. They can include:

- **Static content**: Headers, navigation elements, sidebars, footers
- **Editable regions**: Areas where page-specific content can be added
- **Design elements**: CSS styling, layout structures, HTML components

Templates are particularly useful when you want to:

- Maintain consistent page layouts across your site
- Speed up page creation with pre-designed structures
- Make site-wide layout changes efficiently
- Create a library of reusable page designs

## Creating a Template

### Step 1: Access Templates

1. Navigate to the Templates section from the main menu
2. Click the **"New Template"** button

### Step 2: Choose an Editor

SkyCMS provides two editing modes for templates:

#### Design Editor

- Visual drag-and-drop interface powered by GrapeJS
- Ideal for users who prefer visual design
- Includes pre-built components and blocks
- Real-time preview of changes

#### Code Editor

- Monaco editor with HTML syntax highlighting
- Full control over HTML markup
- Ideal for developers and advanced users
- Supports copy/paste from external HTML sources

### Step 3: Define Editable Regions

Editable regions are areas where content editors can add page-specific content. To create an editable region, use the `contenteditable` attribute or add the `data-ccms-ceid` attribute:

```html
<div contenteditable="true">
  This content can be edited on individual pages
</div>
```

Or:

```html
<div data-ccms-ceid="unique-id">
  This is an editable region
</div>
```

**Important Notes:**

- Each editable region must have a unique ID (`data-ccms-ceid`)
- The system automatically generates IDs if not provided
- Nested editable regions are not allowed
- Editable regions enable the "Live Editor" for pages

### Step 4: Save and Test

1. Click **"Save"** to save your template
2. Use the **"Preview"** button to see how your template looks
3. Edit the title and description to help identify the template later

## Editing Templates

### Editing Title and Description

1. Go to the Templates list
2. Click the pencil icon next to the template name
3. Update the title and description
4. Click **"Save"**

The description field supports rich text formatting and is helpful for:

- Explaining when to use the template
- Documenting special features
- Providing usage instructions

### Editing Template Code

#### Using the Code Editor

1. Click **"Code"** button for the template
2. Edit the HTML in the Monaco code editor
3. Use keyboard shortcuts:
   - **Ctrl+S** (or **Cmd+S** on Mac): Save changes
   - Auto-save is available after 1.5 seconds of inactivity (if enabled)
4. Click **"Preview"** to see your changes

#### Using the Design Editor

1. Click **"Design"** button for the template
2. Use the drag-and-drop interface to modify the layout
3. Click **"Save"** when done
4. Changes are automatically applied to the template

### Template Indicators

Templates with the **"Live editor enabled"** indicator contain editable regions that allow pages to be edited using the WYSIWYG (What You See Is What You Get) editor.

## Using Templates

### Creating a Page from a Template

1. Navigate to **"Create a Page"**
2. Enter the desired page URL
3. Select a template from the "Quick Start Templates" list
4. Click **"Create"**

The new page will be created with the template's structure and will be ready for content editing.

### Templates vs. Blank Pages

- **With a Template**: Start with a pre-designed structure; add content to editable regions
- **Without a Template**: Start with a blank page; build from scratch

## Managing Template-Based Pages

### Viewing Pages Using a Template

1. Go to the Templates list
2. Click **"Pages"** button for any template
3. View all pages that use this template

The page list shows:

- Page number and title
- Last published date
- Current status (Published/Draft)
- Last updated date
- Access permissions (if authentication is enabled)

### Page Actions

From the template's page list, you can:

- **Select a page**: Click on the article number or title to open version history
- **Clone a page**: Create a duplicate of an existing page
- **Unpublish**: Remove a page from public view (admins/editors only)
- **Update single page**: Apply the latest template changes to one page

## Updating Pages with Templates

### Why Update Pages?

When you modify a template, existing pages don't automatically change. You must explicitly update pages to apply template changes.

### Update Methods

#### Update Single Page

1. Go to the template's page list
2. Click the **update icon** next to a specific page
3. The page will be updated and opened in the live editor

**What happens:**

- Template structure is applied to the page
- Content in editable regions is preserved
- Non-editable content is replaced with template content

#### Update All Pages

⚠️ **WARNING**: This action affects all pages using the template and may result in content loss if editable regions have been removed or changed.

1. Go to the template's page list
2. Click **"Update all pages"** button
3. Read and confirm the warning message
4. Wait for the process to complete (may take several minutes)

**Best Practice**: Update a single page first to ensure the changes work as expected before updating all pages.

### How Updates Work

The update process:

1. Loads the latest template HTML
2. Identifies editable regions by their `data-ccms-ceid` attribute
3. Preserves content from matching editable regions
4. Replaces the page structure with the new template
5. Creates a new version ready for editing

**Important**: Only content in editable regions with matching IDs is preserved. If you:

- Remove an editable region from the template → content in that region is lost
- Change an editable region's ID → content is not transferred
- Add new editable regions → they appear empty on updated pages

## Best Practices

### Template Design

1. **Plan Your Editable Regions**: Carefully consider which areas should be editable
2. **Use Descriptive IDs**: While system-generated IDs work, meaningful IDs make templates easier to maintain
3. **Test Thoroughly**: Preview templates and create test pages before rolling out
4. **Document Your Templates**: Use the description field to explain usage and features
5. **Keep It Simple**: Start with basic templates and add complexity as needed

### Managing Templates

1. **Organize by Purpose**: Create different templates for different page types (e.g., blog posts, landing pages, product pages)
2. **Version Control**: Before major template changes, consider the impact on existing pages
3. **Incremental Updates**: Make small, incremental changes rather than complete redesigns
4. **Communication**: If working in a team, communicate template changes to content editors

### Editable Regions

1. **Unique IDs**: Ensure each editable region has a unique `data-ccms-ceid` value
2. **No Nesting**: Never nest editable regions inside each other
3. **Consistent Structure**: Maintain editable region IDs when updating templates to preserve content
4. **Adequate Regions**: Provide enough editable regions for flexible content creation

### Updating Pages

1. **Test First**: Always update a single page first before updating all pages
2. **Backup Strategy**: Consider your backup/versioning strategy before major updates
3. **Off-Peak Updates**: Schedule mass updates during low-traffic periods
4. **Monitor Results**: Check updated pages to ensure content displays correctly
5. **Communicate Changes**: Inform content editors when templates have been updated

## Troubleshooting

### Template Not Showing in Create Page List

- Ensure the template is saved and associated with the default layout
- Check that the template has valid HTML content

### Live Editor Not Available

- Verify that the template contains editable regions with `contenteditable` or `data-ccms-ceid` attributes
- Check for nested editable regions (not allowed)
- Ensure editable regions use supported elements (div, h1-h5)

### Content Lost After Template Update

- Editable region IDs may have changed
- Editable regions may have been removed from the template
- Create a new version from the page history if needed

### Template Won't Save

- Check for nested editable regions (validation error)
- Ensure HTML is well-formed
- Check browser console for JavaScript errors

## Technical Details

### Required Permissions

Template management requires one of the following roles:

- **Administrators**: Full access to all template functions
- **Editors**: Full access to all template functions

### Template Properties

- **ID**: Unique identifier (GUID)
- **Title**: Display name for the template
- **Description**: Rich text description and notes
- **Content**: HTML markup of the template
- **Layout**: Associated site design/layout
- **Community Layout ID**: Reference to layout template

### Editable Region Attributes

- `contenteditable="true"`: Marks an element as editable
- `data-ccms-ceid="[unique-id]"`: SkyCMS unique identifier for editable regions
- Both attributes can be present; `data-ccms-ceid` is required for updates

### Supported Editable Elements

- `<div>`
- `<h1>`, `<h2>`, `<h3>`, `<h4>`, `<h5>`

Other elements with `contenteditable` will work in the editor but may not be properly tracked for template updates.

---

## Additional Resources

- [Creating Content](../README.md)
- [Live Editor Guide](./LiveEditor.md) *(if available)*
- [Layouts Documentation](./Layouts.md) *(if available)*
- [SkyCMS Documentation](https://www.moonrise.net/cosmos/documentation/)

## Support

For additional help or to report issues:

- Visit the [SkyCMS GitHub Repository](https://github.com/CWALabs/SkyCMS)
- Check the official documentation at [moonrise.net](https://www.moonrise.net/cosmos/documentation/)

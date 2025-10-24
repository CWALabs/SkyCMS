# Code Editor

The Code Editor in SkyCMS is a powerful, Monaco-based code editing environment that provides a professional development experience for editing HTML, CSS, and JavaScript directly within the CMS. This editor is used for working with **Layouts**, **Templates**, and **Articles** when you need precise control over the source code.

## What is the Monaco Editor?

The Code Editor is built on [Monaco Editor](https://microsoft.github.io/monaco-editor/), the same editor that powers Visual Studio Code. This means you get the same powerful editing features in your browser that professional developers use every day.

## Where to Find the Code Editor

The Code Editor is available in three main areas of SkyCMS:

1. **Layouts** - Access via the Layouts section for editing layout HTML, CSS, and JavaScript
2. **Templates** - Access via the Templates section for editing template content
3. **Articles** - Access via the Editor section for editing article content and scripts

Each section has an "Edit Code" option that opens the Code Editor interface.

## Key Features

### 1. Syntax Highlighting

The editor automatically highlights code based on the language you're working with:

- **HTML** - For markup and structure
- **CSS** - For styling
- **JavaScript** - For scripts and interactivity

The syntax highlighting makes it easy to read and understand your code at a glance.

### 2. Dark Theme

The editor uses a dark theme (`vs-dark`) that reduces eye strain during extended editing sessions and provides excellent contrast for code syntax highlighting.

### 3. IntelliSense & Auto-completion

As you type, the editor provides intelligent code completion suggestions:

- HTML tag names and attributes
- CSS properties and values
- JavaScript functions and variables
- Context-aware suggestions based on your current code

Simply start typing and press `Tab` or `Enter` to accept a suggestion.

### 4. Emmet Support

One of the most powerful features of the Code Editor is **Emmet notation support**. Emmet allows you to write HTML and CSS much faster using abbreviations.

#### Emmet Examples

**HTML Abbreviations:**

- Type `ul>li*5` and press `Tab` → Creates an unordered list with 5 list items
- Type `div.container>h1+p` → Creates a div with class "container" containing an h1 and paragraph
- Type `a[href="#"]` → Creates an anchor tag with href attribute
- Type `img[src alt]` → Creates an image tag with src and alt attributes

**CSS Abbreviations:**

- Type `m10` → Expands to `margin: 10px;`
- Type `p20` → Expands to `padding: 20px;`
- Type `fz14` → Expands to `font-size: 14px;`
- Type `w100p` → Expands to `width: 100%;`

For a complete guide to Emmet abbreviations, visit the [Emmet documentation](https://docs.emmet.io/abbreviations/).

### 5. Multiple Field Editing

When editing Layouts or Articles, you'll see tabs at the top of the editor for different code sections:

- **Header JavaScript** - Scripts that run in the `<head>` section
- **Html Content** - The main HTML content
- **Footer JavaScript** - Scripts that run at the end of the page

Click on any tab to switch between these sections. The editor automatically saves your current work when switching tabs.

### 6. Auto-Save

The Code Editor includes an intelligent auto-save feature:

- After you stop typing for 1.5-3.5 seconds (depending on the context), your changes are automatically saved
- You'll see a saving indicator when auto-save is in progress
- This ensures you never lose your work, even if you forget to manually save

### 7. Keyboard Shortcuts

The editor supports powerful keyboard shortcuts to speed up your workflow:

#### Saving

- **Ctrl + S** (Windows/Linux) or **Cmd + S** (Mac) - Manually save your work

#### Navigation

- **Ctrl + F** - Find text in your code
- **Ctrl + H** - Find and replace
- **Ctrl + G** - Go to line number

#### Editing

- **Ctrl + /** - Toggle line comment
- **Ctrl + Z** - Undo
- **Ctrl + Y** or **Ctrl + Shift + Z** - Redo
- **Alt + Up/Down** - Move line up or down
- **Shift + Alt + Up/Down** - Copy line up or down
- **Ctrl + D** - Select next occurrence of current word
- **Ctrl + Shift + K** - Delete line

#### Multi-Cursor Editing

- **Alt + Click** - Add cursor at click position
- **Ctrl + Alt + Up/Down** - Add cursor above/below

### 8. Content Insertion Tools

The toolbar provides quick access to insert common elements:

#### Insert Page Link

Click the "Insert Link" button to:

1. Search for a page in your site
2. Enter the link text
3. The editor will insert a properly formatted `<a>` tag

#### Insert File Link

Click the "Insert File Link" button to:

1. Browse your file manager
2. Select a file (PDF, document, etc.)
3. The editor will insert a link to that file

#### Insert Image

Click the "Insert Image" button to:

1. Browse your file manager for images
2. Select an image
3. The editor will insert an `<img>` tag with the correct path

## Working with Different Content Types

### Editing Layouts

When editing layouts, you have three main sections:

1. **Head** - JavaScript and CSS that loads in the page header
2. **HtmlHeader** - HTML content for the header section of your layout
3. **FooterHtmlContent** - HTML content for the footer section

Use the tabs at the top to switch between these sections.

### Editing Templates

Templates typically have a single **Content** section where you can define reusable HTML structures that can be applied to multiple pages.

### Editing Articles

Articles provide three editing sections:

1. **HeadJavaScript** - Scripts that run in the page header
2. **Content** - The main HTML content of your article
3. **FooterJavaScript** - Scripts that run at the end of the page

## Best Practices

### 1. Use Emmet for Faster Coding

Learn common Emmet abbreviations to dramatically speed up your HTML and CSS writing. Start with simple ones and gradually expand your knowledge.

### 2. Test Your Code

After making changes, preview your page to ensure everything works as expected. The Code Editor gives you full control, but also full responsibility for valid code.

### 3. Keep Your Code Organized

- Use consistent indentation (the editor helps with this automatically)
- Add comments to explain complex sections
- Group related code together

### 4. Watch for Errors

Pay attention to any error messages or warnings the editor might display. These can help you catch issues before they affect your published pages.

### 5. Use the Find Feature

For large code files, use **Ctrl + F** to quickly locate specific elements, classes, or IDs.

### 6. Save Before Switching Editors

While auto-save helps protect your work, it's good practice to manually save (**Ctrl + S**) before switching to a different editor or closing the browser.

## Common Use Cases

### Adding Custom CSS

1. Open your Layout in the Code Editor
2. Switch to the **Head** or **HtmlHeader** tab
3. Add your CSS inside `<style>` tags:

   ```html
   <style>
   .my-custom-class {
       color: #333;
       font-size: 16px;
   }
   </style>
   ```

### Adding Custom JavaScript

1. Open your Layout or Article in the Code Editor
2. Switch to the **Header JavaScript** or **Footer JavaScript** tab
3. Add your JavaScript inside `<script>` tags:

   ```html
   <script>
   document.addEventListener('DOMContentLoaded', function() {
       // Your code here
       console.log('Page loaded!');
   });
   </script>
   ```

### Creating a Complex HTML Structure with Emmet

1. Open the Code Editor
2. Type: `section.hero>div.container>h1.title+p.subtitle+button.cta`
3. Press `Tab`
4. Result:

   ```html
   <section class="hero">
       <div class="container">
           <h1 class="title"></h1>
           <p class="subtitle"></p>
           <button class="cta"></button>
       </div>
   </section>
   ```

## Troubleshooting

### Editor Not Loading

If the Code Editor doesn't load:

- Check your browser console for JavaScript errors
- Ensure you have a stable internet connection
- Try refreshing the page
- Clear your browser cache

### Auto-Save Not Working

If auto-save isn't working:

- Ensure you're not in read-only mode
- Check that you have permission to edit the content
- Look for error messages in the save status indicator

### Emmet Abbreviations Not Expanding

If Emmet isn't working:

- Make sure you're in the correct field (HTML mode for HTML abbreviations)
- Press `Tab` after typing the abbreviation
- Check that your abbreviation syntax is correct

### Lost Changes

The editor auto-saves frequently, but if you experience issues:

- Use **Ctrl + S** to manually save regularly
- Don't close your browser immediately after making changes
- Wait for the "Saved" confirmation before navigating away

## Tips for Power Users

1. **Multi-Cursor Editing** - Use `Alt + Click` to place multiple cursors and edit several locations simultaneously

2. **Column Selection** - Hold `Shift + Alt` and drag to select columns of text

3. **Quick Documentation** - Hover over HTML elements or CSS properties to see quick documentation

4. **Code Folding** - Click the arrows in the left margin to collapse/expand code sections

5. **Bracket Matching** - Place your cursor next to a bracket, and the editor will highlight its matching pair

6. **Format Document** - Right-click in the editor and select "Format Document" to auto-format your code

## Additional Resources

- [Monaco Editor Documentation](https://microsoft.github.io/monaco-editor/)
- [Emmet Cheat Sheet](https://docs.emmet.io/cheat-sheet/)
- [HTML Reference](https://developer.mozilla.org/en-US/docs/Web/HTML)
- [CSS Reference](https://developer.mozilla.org/en-US/docs/Web/CSS)
- [JavaScript Guide](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide)

## Summary

The Code Editor in SkyCMS provides professional-grade code editing capabilities with features like syntax highlighting, IntelliSense, Emmet support, and auto-save. Whether you're making small tweaks or building complex layouts from scratch, the Code Editor gives you the power and flexibility to create exactly what you need.

Take time to explore the features, learn some Emmet shortcuts, and practice with the keyboard shortcuts to become more efficient in your content creation workflow.

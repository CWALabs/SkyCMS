# Sky CMS Git API - VS Code Integration

This document explains how to set up VS Code to work with Sky CMS articles through the Git API.

## Quick Start

1. **Start the Git API server**:
   ```bash
   cd GitAPI
   dotnet run
   ```

2. **Clone the repository in VS Code**:
   - Open VS Code
   - Press `Ctrl+Shift+P` (or `Cmd+Shift+P` on Mac)
   - Type "Git: Clone" and select it
   - Enter: `http://admin:your-password@localhost:5000/api/git`
   - Choose a local folder

3. **Start editing articles**:
   - Articles appear as `.html` files in the `articles/` folder
   - Edit them like any other file
   - Use Git operations (commit, push) to save changes back to Sky CMS

## File Structure

```
your-local-folder/
??? articles/
?   ??? home-page-1.html
?   ??? about-us-2.html
?   ??? contact-3.html
??? .git/
```

## Article File Format

Each article is saved as an HTML file with YAML front matter:

```html
---
title: "Home Page"
articleNumber: 1
version: 1
urlPath: "root"
updated: "2024-01-01T12:00:00Z"
author: "admin"
published: null
status: 1
---

<!-- HEAD_JAVASCRIPT_START -->
<script>
  console.log('Head script');
</script>
<!-- HEAD_JAVASCRIPT_END -->

<!-- CONTENT_START -->
<h1>Welcome to Sky CMS</h1>
<p>This is the home page content.</p>
<!-- CONTENT_END -->

<!-- FOOTER_JAVASCRIPT_START -->
<script>
  console.log('Footer script');
</script>
<!-- FOOTER_JAVASCRIPT_END -->
```

## VS Code Extensions (Recommended)

1. **Git Graph** - Visual Git history
2. **HTML CSS Support** - Better HTML editing
3. **Auto Rename Tag** - HTML tag editing
4. **Prettier** - Code formatting
5. **Live Server** - Preview HTML files

## Git Operations

### Making Changes
1. Edit any article file
2. Save the file (`Ctrl+S`)
3. Commit changes (`Ctrl+Shift+G`, then type commit message)
4. Push to server (this saves to Sky CMS database)

### Creating New Articles
1. Create a new `.html` file in the `articles/` folder
2. Use the format: `article-title-{number}.html`
3. Add the YAML front matter and content
4. Commit and push

### Viewing Article History
1. Right-click on an article file
2. Select "Git: View File History"
3. See all versions of the article

## Authentication

The Git API uses HTTP Basic Authentication. Configure credentials in the Git API's `appsettings.json`:

```json
{
  "GitApiSettings": {
    "Username": "your-username",
    "Password": "your-secure-password"
  }
}
```

## API Endpoints for Advanced Users

### Git Protocol Endpoints
- `GET /api/git/refs` - Repository references
- `GET /api/git/trees/{sha}` - File tree
- `GET /api/git/blobs/{sha}` - File content
- `GET /api/git/commits/{sha}` - Commit info

### Article Endpoints
- `GET /api/articles` - List all articles
- `GET /api/articles/{id}` - Get specific article
- `PUT /api/articles/{id}` - Update article
- `POST /api/articles` - Create new article

## Troubleshooting

### Authentication Issues
- Ensure username/password are correct in the Git remote URL
- Check that the Git API server is running
- Verify credentials in `appsettings.json`

### File Not Found
- Make sure the Git API server has access to the Sky CMS database
- Check the connection string in `appsettings.json`
- Verify articles exist in the database

### Push/Pull Issues
- Ensure you have the latest changes: `git pull`
- Check for merge conflicts
- Verify network connectivity to the Git API server

## Security Notes

- Always use HTTPS in production
- Use strong passwords
- Consider implementing IP restrictions
- Monitor access logs
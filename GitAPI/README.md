# Sky CMS Git API

This project provides a Git-compatible API for editing Sky CMS articles through Git clients like VS Code, GitHub Desktop, or command-line Git tools.

## Features

- Git-compatible HTTP API
- Basic authentication
- List, view, and edit articles as files
- Version control for articles
- VS Code integration support

## Setup

1. **Configure Connection String**: Update the database connection string in `appsettings.json`
2. **Set Authentication**: Update the `GitApiSettings` section with secure credentials
3. **Run the Application**: `dotnet run`

## API Endpoints

### Git Protocol Endpoints
- `GET /api/git/refs` - Get repository references
- `GET /api/git/refs/{refPath}` - Get specific reference
- `GET /api/git/trees/{sha}` - Get repository tree (file listing)
- `GET /api/git/blobs/{sha}` - Get file content
- `POST /api/git/blobs` - Create new blob
- `GET /api/git/commits/{sha}` - Get commit information
- `PATCH /api/git/refs/{refPath}` - Update reference

### Article-Specific Endpoints
- `GET /api/articles` - List all articles
- `GET /api/articles/{id}` - Get specific article
- `GET /api/articles/{id}/versions` - Get article versions
- `PUT /api/articles/{id}` - Update article
- `POST /api/articles` - Create new article

## VS Code Setup

1. Install the "Git" extension (usually included)
2. Clone the repository:
   ```bash
   git clone http://admin:password@localhost:5000/api/git your-local-folder
   ```
3. Open the folder in VS Code
4. Edit articles as HTML files
5. Commit and push changes back to Sky CMS

## Authentication

The API uses HTTP Basic Authentication. Configure credentials in `appsettings.json`:

```json
{
  "GitApiSettings": {
    "Username": "your-username",
    "Password": "your-secure-password"
  }
}
```

## File Format

Articles are represented as HTML files with YAML front matter:

```html
---
title: "My Article"
articleNumber: 1
version: 1
urlPath: "my-article"
updated: "2024-01-01T00:00:00Z"
author: "system"
published: null
status: 1
---

<!-- CONTENT_START -->
<h1>My Article Content</h1>
<p>This is the main content of the article.</p>
<!-- CONTENT_END -->
```

## Security Notes

- Always use HTTPS in production
- Use strong passwords for authentication
- Consider implementing IP restrictions
- Monitor access logs for unauthorized attempts
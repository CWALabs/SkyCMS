# SkyCMS Editor - Content Management Interface

The SkyCMS Editor is a sophisticated content management application that provides a comprehensive interface for creating, editing, and managing website content. Built on .NET 9.0 with ASP.NET Core, it offers multiple editing modes, multi-tenancy support, and integrates with modern web content editing tools.

Quick links:

- Storage setup: [Docs/StorageConfig.md](../Docs/StorageConfig.md) (see also: [AWS S3](../Docs/AWS-S3-AccessKeys.md), [Cloudflare R2](../Docs/Cloudflare-R2-AccessKeys.md))
- Database setup: [Docs/DatabaseConfig.md](../Docs/DatabaseConfig.md)
- Deploy to Azure: [ArmTemplates/azuredeploy.json](../ArmTemplates/azuredeploy.json)

## 🎯 Overview

The Editor serves as the administrative and content creation hub of the SkyCMS platform, providing content creators, editors, and administrators with powerful tools to manage website content without requiring technical expertise.

### Key Features

- **Multi-Editor Support**: Visual editors (GrapesJS), WYSIWYG editors (CKEditor 5), and code editors (Monaco)
- **Multi-Tenancy**: Support for both single-tenant and multi-tenant configurations
- **Real-time Collaboration**: SignalR integration for live editing sessions
- **File Management**: Comprehensive file and media management system
- **Template System**: Layout and template management for consistent design
- **User Management**: Role-based access control with granular permissions
- **Version Control**: Article versioning and comparison tools
- **Publishing Workflow**: Content approval and publishing management

## 🏗️ Architecture

### Application Structure

```text
Editor/
├── Boot/                      # Application startup configurations
│   ├── MultiTenant.cs        # Multi-tenant mode configuration
│   └── SingleTenant.cs       # Single-tenant mode configuration
├── Controllers/              # MVC Controllers
│   ├── EditorController.cs   # Main content editing controller
│   ├── FileManagerController.cs  # File management operations
│   ├── TemplatesController.cs    # Template management
│   └── ...
├── Data/                     # Data layer and business logic
│   ├── Logic/               # Business logic services
│   └── ApplicationDbContext.cs
├── Models/                   # View models and data models
├── Services/                 # Application services
├── Views/                    # Razor views and templates
└── wwwroot/                  # Static assets and libraries
```

### Core Components

#### EditorController

The main controller handling content editing operations:

- **Article Management**: Create, edit, delete, and version articles
- **Content Editing**: Support for HTML, code, and visual editing modes
- **Publishing Workflow**: Draft management and content publishing
- **Collaboration Tools**: Real-time editing and commenting features

#### Multi-Tenancy Support

- **Single Tenant Mode**: Traditional CMS setup for individual websites
- **Multi-Tenant Mode**: Shared editor serving multiple client websites
- **Configuration Management**: Dynamic settings per tenant

#### Content Editing Tools Integration

##### CKEditor 5

- Rich text WYSIWYG editing
- Custom plugins for SkyCMS functionality
- Auto-save capabilities
- Media embedding and link management

##### GrapesJS

- Visual web page builder
- Drag-and-drop interface
- Component-based design
- CSS editing and styling tools

##### Monaco Editor (VS Code)

- Advanced code editing with syntax highlighting
- IntelliSense and auto-completion
- Diff tools for version comparison
- Emmet support for faster HTML/CSS editing

##### Filerobot Image Editor

- Integrated image editing capabilities
- Crop, resize, and filter operations
- Direct integration with file management

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK
- Storage: Azure Blob or Amazon S3 (see StorageConfig)
- Database: SQLite (quick start), or Azure SQL / SQL Server / Cosmos DB / MySQL
- Node.js (for frontend dependencies)

### Installation

1. **Clone and Navigate**

   ```bash
   git clone https://github.com/MoonriseSoftwareCalifornia/SkyCMS.git
   cd SkyCMS/Editor
   ```

2. **Install Dependencies**

   ```bash
   # .NET packages
   dotnet restore
   
   # Frontend dependencies
   npm install
   ```

3. **Configure Settings**

    Update `appsettings.json` (local quick start shown):

    ```json
    {
       "ConnectionStrings": {
          "ApplicationDbContextConnection": "Data Source=skycms.db", 
          "StorageConnectionString": "DefaultEndpointsProtocol=https;AccountName=youraccount;AccountKey=yourkey;EndpointSuffix=core.windows.net",
          "BackupStorageConnectionString": "DefaultEndpointsProtocol=https;AccountName=backupacct;AccountKey=...;EndpointSuffix=core.windows.net"
       },
       "CosmosPublisherUrl": "https://your-publisher-url.com",
       "MultiTenantEditor": false
    }
    ```

    Notes:
    - For S3, use: `Bucket={bucket};Region={region};KeyId={access-key-id};Key={secret};`
    - For Azure managed identity, use: `AccountKey=AccessToken` and assign Blob Data roles to the app identity.

4. **Initialize Database**

   For relational providers (SQL Server/MySQL), run EF migrations:

   ```bash
   dotnet ef database update
   ```

   For SQLite quick start, the database file will be created automatically on first run.

5. **Run the Application**

   ```bash
   dotnet run
   ```

   The Editor will be available at `https://localhost:5001`

## 🔧 Configuration

### Single Tenant vs Multi-Tenant

#### Single Tenant Mode

- Configuration via `appsettings.json`
- Direct database connection
- Simpler setup for individual websites

#### Multi-Tenant Mode

- Dynamic configuration per tenant
- Shared application with tenant isolation
- Requires DynamicConfig (Cosmos.ConnectionStrings)

Register in Program.cs:

```csharp
using Cosmos.DynamicConfig;

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IDynamicConfigurationProvider, DynamicConfigurationProvider>();

var app = builder.Build();
app.UseMiddleware<DomainMiddleware>();
```

And add the config database connection:

```json
{
   "ConnectionStrings": {
      "ConfigDbConnectionString": "AccountEndpoint=...;AccountKey=...;Database=SkyCmsConfig" 
   }
}
```

### Key Configuration Options

| Setting | Description | Default |
|---------|-------------|---------|
| `MultiTenantEditor` | Enable multi-tenant mode | false |
| `CosmosPublisherUrl` | Publisher application URL | Required |
| `StorageConnectionString` | Storage provider connection (Azure/S3) | Required |
| `BackupStorageConnectionString` | Optional backup storage for periodic snapshots | Optional |
| `AzureBlobStorageEndPoint` | Static file storage URL (legacy) | "/" |
| `CosmosRequiresAuthentication` | Publisher authentication | false |
| `CosmosStaticWebPages` | Static site generation | true |
| `AllowSetup` | Enable setup wizard | false |

## 🛠️ Development

### Architecture Patterns

- **MVC Pattern**: Clean separation of concerns
- **Dependency Injection**: Service-based architecture
- **Repository Pattern**: Data access abstraction
- **Settings Pattern**: Configuration management

### Key Services

#### ArticleEditLogic

Core business logic for content management:

- Article CRUD operations
- Content validation and processing
- Publishing workflow management
- Version control

#### FileManagerService

File and media management:

- Upload and storage operations
- Image processing and thumbnails
- CDN integration
- File security and validation

#### EditorSettings

Configuration and settings management:

- Multi-tenant configuration
- Dynamic settings loading
- Environment-specific configurations

### Adding Custom Editors

1. **Create Editor Model**

   ```csharp
   public class CustomEditorViewModel : ICodeEditorViewModel
   {
       // Implement required properties
   }
   ```

2. **Add Controller Action**

   ```csharp
   [HttpGet]
   public async Task<IActionResult> CustomEditor(int id)
   {
       // Editor logic
       return View(model);
   }
   ```

3. **Create Editor View**

   ```html
   @model CustomEditorViewModel
   <!-- Editor UI -->
   ```

## 🔐 Security

### Authentication & Authorization

- **ASP.NET Core Identity**: User management
- **Role-Based Access**: Granular permissions
  - **Administrators**: Full system access
  - **Editors**: Content and template management
  - **Authors**: Content creation only
  - **Reviewers**: Content review and approval

### Content Security

- **Input Validation**: XSS protection
- **File Upload Security**: Type and size restrictions
- **Content Sanitization**: HTML cleaning and validation
- **CSRF Protection**: Anti-forgery tokens

## 📊 Performance

### Optimization Features

- **Memory Caching**: Configuration and data caching
- **CDN Integration**: Static asset delivery
- **Lazy Loading**: On-demand content loading
- **Image Processing**: Automatic thumbnails and optimization

### Monitoring

- **Application Insights**: Performance monitoring
- **Custom Metrics**: Editor usage analytics
- **Error Tracking**: Centralized logging
- **Health Checks**: System status monitoring

## 🔗 Integration

### Publisher Integration

The Editor works seamlessly with the SkyCMS Publisher:

- **Content Synchronization**: Real-time content updates
- **Static Site Generation**: Pre-rendered page creation
- **CDN Management**: Automatic cache invalidation
- **SEO Optimization**: Meta data and structure management

### External Services

- **Azure Blob Storage**: File and media storage
- **Azure CDN**: Global content delivery
- **Application Insights**: Monitoring and analytics
- **Azure SQL Database**: Content and configuration storage

## 🚀 Deployment

### Docker Deployment

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY . .
EXPOSE 80
ENTRYPOINT ["dotnet", "Sky.Editor.dll"]
```

### Azure App Service

1. Configure application settings
2. Set up database connection
3. Configure blob storage
4. Deploy using Azure DevOps or GitHub Actions

### Environment Variables

```bash
ConnectionStrings__ApplicationDbContextConnection="Data Source=skycms.db"
ConnectionStrings__StorageConnectionString="DefaultEndpointsProtocol=...;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net"
ConnectionStrings__BackupStorageConnectionString="DefaultEndpointsProtocol=...;AccountName=backupacct;AccountKey=...;EndpointSuffix=core.windows.net"
CosmosPublisherUrl="https://your-site.com"
MultiTenantEditor="false"
# Optional (multi-tenant config DB)
ConnectionStrings__ConfigDbConnectionString="AccountEndpoint=...;AccountKey=...;Database=SkyCmsConfig"
```

## 📖 API Reference

### Content API Endpoints

- `GET /Editor/Index` - Article listing
- `GET /Editor/Edit/{id}` - Visual editor
- `GET /Editor/EditCode/{id}` - Code editor
- `GET /Editor/Designer/{id}` - GrapesJS designer
- `POST /Editor/SaveContent` - Save article content
- `GET /Editor/Versions/{id}` - Version history

### File Management API

- `GET /FileManager/Index` - File browser
- `POST /FileManager/Upload` - File upload
- `GET /FileManager/Download/{id}` - File download
- `DELETE /FileManager/Delete/{id}` - File deletion

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make changes with tests
4. Submit a pull request

### Development Guidelines

- Follow C# coding standards
- Include unit tests for new features
- Update documentation for API changes
- Ensure responsive design for new UI components

## 📄 License

Licensed under the GNU General Public License v3.0. See [LICENSE](../License.md) for details.

## 🔗 Related Projects

- **[Publisher](../Publisher/README.md)**: Public-facing website application
- **[Common](../Common/README.md)**: Shared libraries and utilities
- **[AspNetCore.Identity.FlexDb](../AspNetCore.Identity.FlexDb/README.md)**: Flexible identity provider

## 📞 Support

- **Documentation**: [SkyCMS Wiki](https://github.com/MoonriseSoftwareCalifornia/SkyCMS/wiki)
- **Issues**: [GitHub Issues](https://github.com/MoonriseSoftwareCalifornia/SkyCMS/issues)
- **Discussions**: [GitHub Discussions](https://github.com/MoonriseSoftwareCalifornia/SkyCMS/discussions)

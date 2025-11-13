# A light-weight, high-Performance, full-featured, multi-cloud content management system

![SkyCMS Logo](/Editor/wwwroot/images/skycms/SkyCMSLogoNoWiTextDarkTransparent30h.png)

[![CodeQL](https://github.com/MoonriseSoftwareCalifornia/SkyCMS/actions/workflows/codeql.yml/badge.svg)](https://github.com/MoonriseSoftwareCalifornia/SkyCMS/actions/workflows/codeql.yml)
[![Publish Docker Images CI](https://github.com/MoonriseSoftwareCalifornia/SkyCMS/actions/workflows/docker-image.yml/badge.svg)](https://github.com/MoonriseSoftwareCalifornia/SkyCMS/actions/workflows/docker-image.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

<!-- [Project Website](https://Sky.moonrise.net) | [Documentation](https://Sky.moonrise.net/Docs) | [Get Free Help](https://Sky.moonrise.net/Support) | [YouTube Channel](https://www.youtube.com/@Sky-cms) | [Slack Channel](https://Sky-cms.slack.com/) -->

Deploy anywhere: AWS, Azure, Cloudflare, Google — or any cloud that supports Docker containers, one of our supported databases (SQLite, MySQL, MS SQL or Cosmos DB), and either S3‑compatible object storage or Azure Storage.

Content tools are intuitive and rich, making them developer-friendly and non-technical user-friendly, perfect for web professionals working together with content creators.

Choose your hosting model: [Static](./Docs/StorageConfig.md#static-website-hosting-azure) | [Edge (origin-less)](./Docs/CloudflareEdgeHosting.md) | [Dynamic](./Publisher/README.md) | [Decoupled](./Publisher/README.md)

## Overview

[SkyCMS](https://Sky-cms.com/) is an open-source, cloud-native Content Management System designed for high-performance, scalability, and ease of use. Built with modern web technologies, **SkyCMS is primarily a traditional CMS that renders complete web pages**, either dynamically or as static files. It runs in multiple modes to meet different deployment needs and can be hosted traditionally or at the edge (origin-less) using Cloudflare Workers + R2:

- **Static Mode** (Primary): Content rendered as static HTML files and hosted on cloud storage (Azure Blob, S3, Cloudflare R2) - highest performance, stability, and operational simplicity
  - Can be deployed as an **origin-less, edge-hosted** site via Cloudflare R2 + Worker
  - Automatic static site generation from CMS content
- **Dynamic Mode**: Publisher application dynamically renders pages on-demand - full CMS functionality with server-side rendering
- **Decoupled Mode**: Separate editor and publisher applications - near-static performance with backend functionality
- **API Mode** (Optional): RESTful API available for headless scenarios - content delivered as JSON for multi-channel distribution

## 🎯 Design Objectives

SkyCMS was built with the following core objectives:

- **Performance**: Outperform traditional CMSs through static site generation and optimized dynamic rendering
- **User-Friendly**: Easy to use by both web developers and non-technical content editors
- **Low Maintenance**: Easy to administer with low operational costs
- **Flexible Deployment**: Support for static, dynamic, decoupled, and optional API modes
- **Cloud-Native**: Built for modern cloud infrastructure with global scalability
- **Complete Page Rendering**: Primary focus on delivering complete HTML pages rather than API-first architecture
- **Integrated Publishing**: Built-in version control, automatic triggers, and direct cloud deployment—eliminating the complexity of traditional Git-based static site workflows

## ✨ Key Features

### Advantages Over Traditional Git-Based Static Site Deployment

SkyCMS represents a **next-generation approach** to static site publishing that eliminates the complexity and friction of traditional Git-based CI/CD workflows:

| **Traditional Approach** | **SkyCMS Approach** |
|--------------------------|---------------------|
| External Git repository required | Built-in version control integrated into CMS |
| Separate CI/CD pipeline (GitHub Actions, Netlify, etc.) | Automatic triggers built into the system |
| Static site generator needed (Jekyll, Hugo, Gatsby, Next.js) | Direct rendering without external build tools |
| Multiple tools to learn and configure | Single integrated platform |
| Build time delays (minutes per deployment) | Instant publishing with Publisher component |
| Complex pipeline debugging | Streamlined troubleshooting |
| Content creators need Git knowledge | User-friendly content management interface |
| Static OR dynamic content | **Hybrid: simultaneous static AND dynamic content** |

**Key Technical Advantages:**

- **No Build Pipeline Required**: Content is rendered directly by the Publisher component, eliminating wait times and pipeline configuration
- **Integrated Version Control**: Full versioning system built into the CMS—no external Git workflow needed
- **Automatic Deployment**: Direct deployment to Azure Blob Storage, AWS S3, or Cloudflare R2 without intermediary services
- **Faster Publishing**: Changes go live immediately without waiting for CI/CD builds
- **Hybrid Architecture**: Serve static files for performance while maintaining dynamic capabilities when needed
- **Simplified Operations**: Fewer moving parts mean less infrastructure to maintain and fewer points of failure
- **Multi-Cloud Native**: Deploy to any cloud platform that supports Docker containers and object storage

### Performance Benchmarks

Real-world comparison of publishing workflows:

| Scenario | Git-Based (Netlify) | SkyCMS |
|----------|-------------------|---------|
| Single page update | 2-5 minutes | < 5 seconds |
| Bulk content update (50 pages) | 5-15 minutes | < 30 seconds |
| Image optimization | Build-time penalty | On-upload processing |
| Preview environment | Separate branch + build | Instant preview mode |
| Rollback time | Redeploy previous build (2-5 min) | Instant version restore |

This **CMS-native approach** achieves the same benefits as JAMstack (speed, scalability, global distribution) but with dramatically reduced complexity and operational overhead.

### Content Management

- **Multiple Content Types**: Standard pages, blog posts, and custom article types
- **Rich Editing Tools**: CKEditor 5, GrapesJS, Monaco Editor, and Filerobot image editor
- **Version Control**: Full versioning system with restore capabilities
- **Template System**: Reusable page templates with editable regions
- **Multi-Mode Publishing**: Static file generation, dynamic rendering, or optional API delivery

### Blogging Platform

SkyCMS includes robust blogging capabilities built on its flexible article system:

- **Blog Post Management**: Create and manage blog posts with categories and introductions
- **Article Versioning**: Full version history for all blog posts
- **Publishing Workflow**: Draft, schedule, and publish blog content
- **Catalog System**: Organized listing of blog posts with metadata
- **Future Enhancements**: RSS feeds, category archives, tagging system, and more

[Learn more about Blog Features](./Docs/blog/BlogPostLifecycle.md) | [Planned Blog Enhancements](./Docs/blog/BlogFutureEnhancements.md)

### Performance & Scalability

- **Static Site Generation**: Automatic static file creation for maximum performance—without external build tools or CI/CD pipelines
- **Integrated Publishing**: Built-in Publisher component handles rendering and deployment, eliminating the complexity of traditional Git-based static site workflows
- **CDN Integration**: Built-in support for content delivery networks
- **Edge Hosting**: Origin-less deployment via Cloudflare Workers
- **Multi-Cloud Storage**: Azure Blob, AWS S3, Cloudflare R2 support
- **Caching Strategies**: Intelligent caching for optimal performance
- **Instant Deployment**: Changes published immediately without waiting for build pipelines

### Security & Access Control

- **Role-Based Access**: Administrator and Editor roles
- **Identity Integration**: ASP.NET Core Identity with Azure B2C support
- **External Providers**: Google and Microsoft authentication
- **Permission System**: Article-level access control
- **Secure File Storage**: Encrypted and authenticated storage access

## 🚀 Use Cases

SkyCMS excels in demanding scenarios:

- **High-Capacity Websites**: Government sites during emergencies, news portals
- **Content-Heavy Platforms**: Media sites like New York Times, National Geographic, streaming platforms
- **Performance-Critical Applications**: Sites requiring minimal latency and efficient content delivery
- **Global Distribution**: Multi-regional redundancy with minimal administration overhead
- **Non-Technical Teams**: User-friendly interface requiring minimal training

## 🛠️ Content Editing Tools

SkyCMS integrates the best web content creation tools to provide a comprehensive editing experience:

### CKEditor 5

![CKEditor](./Docs/ckeditor.webp)

Industry-standard WYSIWYG editor with rich text formatting, extensive plugin support, and intuitive interface. Perfect for non-technical users who want Word-like editing capabilities.

[Learn more about the Live Editor](./Docs/Editors/LiveEditor/README.md)

### GrapesJS

![GrapesJS](./Docs/grapesjs.png)

Visual web builder with drag-and-drop interface for creating complex layouts without coding. Ideal for designing landing pages, newsletters, and custom templates.

[Watch our GrapesJS demo video](https://www.youtube.com/watch?v=mVGPlbnbC5c) | [Designer Documentation](./Docs/Editors/Designer/README.md)

### Monaco Editor (Visual Studio Code)

![Monaco Editor](./Docs/CodeEditor.png)

Powerful code editor for developers, featuring syntax highlighting, IntelliSense, and advanced editing capabilities. Includes diff tools and Emmet notation support.

[Code Editor Documentation](./Docs/Editors/CodeEditor/README.md)

### Filerobot Image Editor

![Filerobot](./Docs/Filerobot.png)

Integrated image editing with resizing, cropping, filtering, and annotation capabilities. Edit images directly within the CMS without external tools.

[Image Editing Documentation](./Docs/Editors/ImageEditing/README.md)

### FilePond File Uploader

Modern file upload interface with drag-and-drop, image previews, and file validation. Supports multiple file types with progress tracking.

## 🏗️ Architecture & Technology Stack

### Core Applications

- **Editor Application** (`Editor/`): Content creation and management interface
- **Publisher Application** (`Publisher/`): Public-facing website renderer
- **Common Library** (`Common/`): Shared functionality and utilities
- **Blob Service** (`Cosmos.BlobService/`): File storage management
- **Dynamic Configuration** (`Cosmos.ConnectionStrings/`): Runtime configuration
- **Identity Framework** (`AspNetCore.Identity.FlexDb/`): User authentication and authorization

### Technology Stack

- **Backend**: ASP.NET Core 9.0+ (C#)
- **Frontend**: JavaScript (70% of codebase), HTML5, CSS3, SCSS
- **Database**: Azure Cosmos DB (NoSQL), MS SQL, MySQL or SQLite
- **Storage**: Azure Blob Storage, Amazon S3, Cloudflare R2 (S3-compatible)
- **Hosting**: Linux Docker containers; Cloudflare Workers (edge/origin-less)
- **Authentication**: ASP.NET Core Identity, Google and Microsoft

### Infrastructure Components

- **Database Options**
  - Azure Cosmos DB: Multi-user, globally distributed NoSQL database
  - MS SQL, MySQL: Relational databases
  - SQLite: Built in database for single editor applications
- **Cloud Storage Options**
  - Azure Storage: File share and BLOB storage
  - Amazon S3 (and compatible): BLOB storage
  - Cloudflare R2 for BLOB storage
  - Any SMB or NFS persistent file share storage
- **Edge Hosting Options**
  - Cloudflare Workers + R2: Origin-less static hosting at the edge

## 📁 Project Structure

```text
SkyCMS/
├── ArmTemplates/           # Azure Resource Manager deployment templates
├── Common/                 # Shared libraries and utilities
├── Cosmos.BlobService/     # File storage service layer
├── Cosmos.ConnectionStrings/ # Dynamic configuration management
├── AspNetCore.Identity.FlexDb/ # Flexible identity framework
├── Editor/                 # Content management application
├── Publisher/              # Public website application
├── docker-compose.yml      # Local development orchestration
└── SkyCMS.sln             # Visual Studio solution file
```

## 📚 Component Documentation

Each component has detailed documentation explaining its purpose, configuration, and usage:

### Infrastructure & Deployment

- **[ARM Templates](./ArmTemplates/README.md)** - Azure deployment templates and infrastructure setup
  - Complete Azure Resource Manager templates
  - One-click deployment configuration
  - Email service integration (Azure Communication Services, SendGrid, SMTP)
  - Storage and database setup

### Applications

- **[Editor Application](./Editor/README.md)** - Content management interface
  - Article creation and editing with CKEditor 5, GrapesJS, and Monaco Editor
  - Media management with Filerobot image editor
  - User management and role-based access control
  - Real-time collaboration features

- **[Publisher Application](./Publisher/README.md)** - Public-facing website
  - High-performance content delivery
  - SEO optimization and sitemap generation
  - Multi-tenant support for hosting multiple websites
  - Static and dynamic content rendering

### Shared Libraries

- **[Common Library](./Common/README.md)** - Core shared functionality
  - Multi-database support (Cosmos DB, SQL Server, MySQL, SQLite)
  - Base controllers and data models
  - Authentication utilities and services
  - Article management and content processing

- **[Blob Service](./Cosmos.BlobService/README.md)** - Multi-cloud file storage
  - Azure Blob Storage and AWS S3 support
  - File management and media handling
  - CDN integration and performance optimization
  - Secure file access and permissions

- **[Dynamic Configuration](./Cosmos.ConnectionStrings/README.md)** - Runtime configuration management
  - Multi-tenant configuration support
  - Dynamic connection string management
  - Environment-specific settings
  - Configuration caching and performance

- **[Identity Framework](./AspNetCore.Identity.FlexDb/README.md)** - Flexible authentication
  - Multi-database identity provider support
  - ASP.NET Core Identity integration
  - Azure B2C and external provider support
  - Role-based security and permissions

## 🐳 Docker Containers

SkyCMS applications are distributed as Docker containers for consistent deployment:

- **Editor**: [`toiyabe/sky-editor:latest`](https://hub.docker.com/r/toiyabe/sky-editor)
- **Publisher**: [`toiyabe/sky-publisher:latest`](https://hub.docker.com/r/toiyabe/sky-publisher)
- **API**: [`toiyabe/sky-api:latest`](https://hub.docker.com/r/toiyabe/sky-api)

Alternative NodeJS Publisher: [Sky.Publisher.NodeJs](https://github.com/MoonriseSoftwareCalifornia/Sky.Publisher.NodeJs)

## 🚀 Quick Start

### Azure Deployment (Recommended)

1. Click the "Deploy to Azure" button above
2. Fill in required parameters (email configuration, storage options)
3. Deploy and access your SkyCMS instance

### Local Development

```bash
# Clone the repository
git clone https://github.com/MoonriseSoftwareCalifornia/SkyCMS.git
cd SkyCMS

# Run with Docker Compose
docker-compose up

# Or build and run locally
dotnet build SkyCMS.sln
dotnet run --project Editor
```

### System Requirements

- **.NET 9.0+** for local development
- **Docker** for containerized deployment
- **Azure/AWS/Google Cloud, etc...** for cloud deployment
- **Visual Studio 2022** or **VS Code** (recommended for development)

## 📖 Documentation

### Getting Started

- **Installation Guide**: [sky.moonrise.net/install](/ArmTemplates/README.md)
- **Azure Deployment**: [Azure Installation Guide](./Docs/AzureInstall.md)
- **AWS Deployment (CloudFormation + ECS Fargate)**: [AWS/README.md](./AWS/README.md)

### Configuration

- **Storage Configuration**: [Docs/StorageConfig.md](./Docs/StorageConfig.md) — Supported providers (Azure Blob, AWS S3, Cloudflare R2), container/bucket naming, CDN integration, and recommended settings
  - AWS: [AWS S3 access keys (quick guide)](./Docs/AWS-S3-AccessKeys.md)
  - Cloudflare: [Cloudflare R2 access keys (quick guide)](./Docs/Cloudflare-R2-AccessKeys.md)
  - Cloudflare: [Edge/origin-less hosting guide (R2 + Worker)](./Docs/CloudflareEdgeHosting.md)
- **Database Configuration**: [Docs/DatabaseConfig.md](./Docs/DatabaseConfig.md) — Provider options (Cosmos DB, SQL Server, MySQL, SQLite), connection strings, EF configuration, and migration guidance

### Content Creation

- **Page Templates**: [Templates Guide](./Docs/Templates/Readme.md) — Creating and managing reusable page templates
- **Live Editor**: [Live Editor Documentation](./Docs/Editors/LiveEditor/README.md) — WYSIWYG content editing with CKEditor 5
- **Designer**: [Designer Documentation](./Docs/Editors/Designer/README.md) — Visual page building with GrapesJS
- **Code Editor**: [Code Editor Documentation](./Docs/Editors/CodeEditor/README.md) — HTML editing with Monaco Editor
- **Image Editing**: [Image Editing Documentation](./Docs/Editors/ImageEditing/README.md) — Filerobot image editor guide
- **File Management**: [File Management Guide](./Docs/FileManagement/README.md) — Managing media and files

### Blogging

- **Blog Post Lifecycle**: [Blog Features](./Docs/blog/BlogPostLifecycle.md) — Understanding blog post creation, editing, and publishing
- **Future Blog Enhancements**: [Planned Features](./Docs/blog/BlogFutureEnhancements.md) — Upcoming blog functionality

### Additional Resources

- **Cosmos vs Headless CMS**: [Comparison Guide](./Docs/CosmosVsHeadless.md)
- **API Reference**: Available in the running application
- **Video Tutorials**: [YouTube Channel](https://www.youtube.com/@Sky-cms)
- **Developer Documentation**: sky.moonrise.net/docs (coming soon)

## 🤝 Contributing

We welcome contributions! Please see our contributing guidelines and:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📧 Email Configuration

SkyCMS supports multiple email providers:

- **Azure Communication Services**
- **SendGrid**
- **Any SMTP**

See the [deployment documentation](./ArmTemplates/README.md) for configuration details.

## 🌟 Key Features

- **Multi-Mode Operation**: Static, headless, and decoupled deployment options
- **High Performance**: Optimized for speed with blob storage and CDN integration
- **User-Friendly**: Intuitive interface for both developers and content creators
- **Scalable**: Built for high-traffic websites with global distribution
- **Secure**: Modern authentication with Azure B2C integration
- **Open Source**: GPL v3 licensed with active community support

<!-- ## 📞 Support

- **Free Community Support**: [sky.moonrise.net/support](https://sky.moonrise.net/support)
- **Slack Community**: [sky-cms.slack.com](https://sky-cms.slack.com/)
- **GitHub Issues**: Report bugs and request features
- **Professional Support**: Available through Moonrise Software -->

## 📄 License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE.md](./Docs/License.md) file for details.

## 🙏 Acknowledgments

SkyCMS integrates several excellent open-source projects:

- CKEditor for rich text editing
- GrapesJS for visual page building
- Monaco Editor for code editing
- Filerobot for image editing
- FilePond for file uploads

---

**Copyright (c) 2025 Moonrise Software, LLC. All rights reserved.**

Built with ❤️ by the [Moonrise Software](https://moonrise.net) team.


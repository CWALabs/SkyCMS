
# Cosmos.BlobService - Sky CMS Azure Storage Provider

**Version:** 9.2.0.2  
**License:** MIT  
**Repository:** [https://github.com/CWALabs/Cosmos.BlobService](https://github.com/CWALabs/Cosmos.BlobService)

## Overview

Cosmos.BlobService is a comprehensive multi-cloud blob storage abstraction layer that provides a unified interface for managing files across different cloud storage providers. It supports Azure Blob Storage, Amazon S3, and Cloudflare R2 (S3-compatible), allowing applications to seamlessly switch between providers or use multiple providers simultaneously.

This library is part of the [Sky CMS ecosystem](https://sky-cms.com) and is built on .NET 9.0.

## Features

### Multi-Cloud Support

- **Azure Blob Storage**: Full support with managed identity authentication and Azure Files Share
- **Amazon S3**: Complete AWS S3 integration with chunked upload support
- **Cloudflare R2**: Full S3-compatible support with custom endpoint configuration
- **Unified Interface**: Single API for all storage operations regardless of provider
- **Runtime Provider Selection**: Dynamic configuration of storage providers based on connection strings

### File Management Operations

- **Upload/Download**: Single and chunked file uploads with metadata tracking
- **Copy/Move/Rename**: File and folder operations across cloud providers
- **Delete**: File and folder deletion with recursive support
- **Metadata Management**: Comprehensive file metadata handling including image dimensions
- **Directory Operations**: Create, list, and manage virtual directory structures
- **Stream Operations**: Efficient streaming for large file operations

### Advanced Features

- **Chunked Uploads**: Support for large file uploads with progress tracking
- **Static Website Hosting**: Enable/disable Azure static website hosting programmatically
- **Data Protection**: Integration with ASP.NET Core data protection
- **Multi-Tenant Support**: Support for single and multi-tenant configurations
- **Memory Caching**: Efficient caching for improved performance
- **Metadata Operations**: Upsert, delete, and retrieve custom metadata for blobs

## Installation

### From Source

This package can be obtained by cloning the [Sky CMS GitHub repository](https://github.com/CWALabs/SkyCMS):

### NuGet Package

The package can be built and referenced directly from your project or published to your private NuGet feed.

## Dependencies

- **.NET 9.0**: Target framework
- **Azure.Storage.Blobs**: Azure Blob Storage SDK
- **Azure.Storage.Files.Shares**: Azure Files Share SDK
- **Azure.Extensions.AspNetCore.DataProtection.Blobs**: Data protection integration
- **Azure.Identity**: Azure authentication
- **AWSSDK.S3**: Amazon S3 SDK
- **Microsoft.Extensions.Caching.Memory**: Memory caching
- **StyleCop.Analyzers**: Code style enforcement

## Configuration

### Azure Blob Storage

**Standard Connection:**

```json
{
  "ConnectionStrings": {
    "StorageConnectionString": "DefaultEndpointsProtocol=https;AccountName=youraccount;AccountKey=yourkey;EndpointSuffix=core.windows.net"
  }
}
```

### Amazon S3

```json
{
  "ConnectionStrings": {
    "StorageConnectionString": "Bucket=your-bucket-name;Region=us-west-2;AccessKey=your-access-key;SecretKey=your-secret-key"
  }
}
```

### Cloudflare R2

```json
{
  "ConnectionStrings": {
    "StorageConnectionString": "Bucket=your-bucket-name;AccountId=your-account-id;AccessKey=your-access-key;SecretKey=your-secret;Endpoint=https://your-account-id.r2.cloudflarestorage.com"
  }
}
```

## Service Registration

In your `Program.cs`:

```csharp
// Register storage context
builder.Services.AddCosmosStorageContext(builder.Configuration);

// Optional: Add data protection with blob storage
builder.Services.AddCosmosCmsDataProtection(builder.Configuration, new DefaultAzureCredential());
```

## Key Methods

| Method | Description | Returns |
|--------|-------------|---------|
| `BlobExistsAsync(path)` | Check if a blob exists | `Task<bool>` |
| `GetFileAsync(path)` | Get file metadata | `Task<FileManagerEntry>` |
| `GetFilesAndDirectories(path)` | List files and folders | `Task<List<FileManagerEntry>>` |
| `GetStreamAsync(path)` | Get file stream | `Task<Stream>` |
| `AppendBlob(stream, metadata, mode)` | Upload file data | `Task` |
| `CopyAsync(source, destination)` | Copy file/folder | `Task` |
| `MoveFileAsync(source, destination)` | Move/rename file | `Task` |
| `DeleteFile(path)` | Delete file | `void` |
| `DeleteFolderAsync(path)` | Delete folder | `Task` |
| `CreateFolder(path)` | Create folder | `Task<FileManagerEntry>` |

## Related Documentation

- [Storage Configuration Guide](../Configuration/Storage-Configuration-Reference.md)
- [Azure Installation Guide](../Installation/AzureInstall.md)
- [Editor Documentation](../../Editor/README.md)
- [Publisher Documentation](../../Publisher/README.md)
- [Common Library](../Components/Cosmos.Common.md)

## License

Licensed under the MIT License.

## Contributing

This project is part of the Sky CMS ecosystem. Visit the [GitHub repository](https://github.com/CWALabs/SkyCMS) for more information.

---

For comprehensive documentation, see the full [Cosmos.BlobService/README.md](../../Cosmos.BlobService/README.md)

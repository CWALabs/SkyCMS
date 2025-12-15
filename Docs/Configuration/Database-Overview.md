# Database Configuration Overview

SkyCMS stores CMS data (users, content, settings) in a relational or document database. Supported providers:

- Azure Cosmos DB (NoSQL/document)
- MS SQL Server / Azure SQL (relational)
- MySQL (relational)
- SQLite (file-based; development/testing only)

## How database configuration works

- You provide a **connection string** that identifies the database provider and credentials.
- SkyCMS automatically selects the correct Entity Framework Core provider based on the connection string format.
- Connection strings are configured via:
  - **SkyCMS Setup Wizard** (easiest for first-time setup)
  - **appsettings.json** (manual configuration)
  - **Environment variables** (recommended for secrets)

The configuration key is: `ConnectionStrings:ApplicationDbContextConnection`

## Quick prerequisites by provider

| Provider | Connection String Format | Where to get values |
| --- | --- | --- |
| **Azure Cosmos DB** | `AccountEndpoint=...;AccountKey=...;Database=...` | Azure Portal → Cosmos DB → Keys & Connection Strings |
| **MS SQL / Azure SQL** | `Server=...;Database=...;User ID=...;Password=...` | SQL Server Management Studio or Azure Portal → SQL Database → Connection strings |
| **MySQL** | `Server=...;Uid=...;Pwd=...;Database=...` | MySQL admin console or cloud provider dashboard |
| **SQLite** | `Data Source=skycms.db` | File-based; generated automatically |

## Configure in SkyCMS (common steps)

### Option 1: Setup Wizard (recommended for first-time setup)

1. Deploy SkyCMS with `CosmosAllowSetup=true`.
2. Open the Editor setup wizard.
3. When prompted for **Database**, paste your connection string.
4. The wizard validates connectivity and creates tables.

### Option 2: Manual Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "ApplicationDbContextConnection": "your-connection-string-here"
  }
}
```

### Option 3: Environment Variables

Set before starting SkyCMS:
```powershell
$env:ConnectionStrings__ApplicationDbContextConnection = "your-connection-string"
```

## Per-provider guides

- [Azure Cosmos DB](./Database-CosmosDB.md)
- [MS SQL Server / Azure SQL](./Database-SQLServer.md)
- [MySQL](./Database-MySQL.md)
- [SQLite](./Database-SQLite.md)

## Detailed Configuration

For complete connection string formats, configuration methods, security best practices, and advanced troubleshooting:

- **[Database Configuration Reference](./Database-Configuration-Reference.md)** - Detailed connection strings, environment variables, and configuration options

---

## See Also

- **[LEARNING_PATHS: DevOps](../LEARNING_PATHS.md#️-devops--system-administrator)** - Database setup as part of DevOps journey
- **[Storage Configuration](./Storage-Overview.md)** - Companion guide for file storage setup
- **[CDN Configuration](./CDN-Overview.md)** - Performance optimization
- **[Authentication & Authorization](../Authentication-Overview.md)** - User and role management
- **[Troubleshooting Guide](../Troubleshooting.md)** - General troubleshooting

---
title: SQL Server / Azure SQL Configuration
description: Configure MS SQL Server or Azure SQL database with connection strings
keywords: SQL-Server, Azure-SQL, database, configuration, relational, ACID
audience: [developers, administrators]
version: 2.0
last_updated: "2026-01-03"
stage: stable
read_time: 6
---

# MS SQL Server / Azure SQL with SkyCMS

MS SQL Server and Azure SQL are relational databases with strong ACID guarantees, rich query support, and enterprise tooling. Both work seamlessly with SkyCMS.

## When to use this
- You prefer relational SQL with strong tooling (SSMS/Azure SQL) and enterprise features.
- You run on Azure or self-host SQL Server and need connection formats quickly.

## Why this matters
- Correct connection strings (with TLS) are required for startup and migrations.
- Azure AD auth and firewall rules impact connectivity and security.

## Key takeaways
- Use `ApplicationDbContextConnection`; include `Encrypt=True` for Azure SQL.
- Azure AD auth is recommended; store secrets in Key Vault/secret stores.
- For local dev, `TrustServerCertificate=True` may be needed; avoid in prod.

## Prerequisites
- SQL/Azure SQL instance, database, and login (or AAD principal) with create/read/write.
- Firewall/NSG rules allowing the app to connect.

## Quick path
1. Create/identify database; copy ADO.NET connection string.
2. Set via wizard or env/appsettings; ensure TLS settings match environment.
3. Start app and verify connectivity; run migrations if applicable.

## Values you need

- **Server**: Hostname or IP of your SQL Server (e.g., `myserver.database.windows.net` for Azure SQL)
- **Database**: Database name
- **User ID**: Login username
- **Password**: Login password

## Create an Azure SQL Database

1. **Azure Portal** → **Create a resource** → search **"SQL Database"** → **Create**.
2. Fill in:
   - **Database name**: Your database name
   - **Server**: Create new or select existing (or create separately)
   - **Server name**: Globally unique hostname (becomes `servername.database.windows.net`)
   - **Authentication**: SQL authentication (enter admin username and password)
   - **Location**: Choose a region
3. Configure **Compute + storage** (Standard/Premium depending on workload).
4. Click **Create** and wait for deployment.

## Get your connection string

### From Azure Portal

1. Portal → **SQL Databases** → select your database.
2. Click **Show database connection strings**.
3. Copy the **ADO.NET (SQL authentication)** connection string:
   ```
   Server=tcp:myserver.database.windows.net,1433;Initial Catalog=mydb;Persist Security Info=False;User ID=sqladmin;Password=yourpassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
   ```

### From SQL Server Management Studio (local SQL Server)

1. Open **SQL Server Management Studio** → **Connect** → enter server name and credentials.
2. Right-click the database → **Properties** → copy connection string from the **General** tab.

## Configure in SkyCMS

### Using the Setup Wizard (recommended)

1. Deploy SkyCMS with `CosmosAllowSetup=true`.
2. Open the Editor setup wizard.
3. When prompted for **Database**, paste the connection string from Azure:
   ```
   Server=tcp:myserver.database.windows.net,1433;Initial Catalog=mydb;Persist Security Info=False;User ID=sqladmin;Password=yourpassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
   ```
4. Click **Validate** and proceed.

### Manual Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "ApplicationDbContextConnection": "Server=tcp:myserver.database.windows.net,1433;Initial Catalog=mydb;Persist Security Info=False;User ID=sqladmin;Password=yourpassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### Local Development (SQL Server)

```json
{
  "ConnectionStrings": {
    "ApplicationDbContextConnection": "Server=localhost;Database=SkyCMS;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Environment Variables

```powershell
$env:ConnectionStrings__ApplicationDbContextConnection = "Server=tcp:myserver.database.windows.net,1433;Initial Catalog=mydb;Persist Security Info=False;User ID=sqladmin;Password=yourpassword;..."
```

## Best practices

- **Use Azure AD authentication** for Azure SQL (instead of SQL authentication) when possible. Simplifies credential management and improves audit trails.
- **Secure passwords** in Azure Key Vault or your secrets manager, not in code.
- **Enable firewall rules** in Azure SQL to restrict access to known IPs/subnets.
- **Backup strategy**: Azure SQL handles automated backups; configure retention and geo-replication as needed.
- **Rotate credentials** periodically.

## Tips and troubleshooting

- For Azure SQL, ensure the connection string includes `Encrypt=True` for TLS.
- For local SQL Server development, set `TrustServerCertificate=True`.
- If connection fails with "Login failed", verify the username/password and firewall rules (Azure SQL).
- Azure SQL connection strings are lengthy; store them in Key Vault or environment variables.

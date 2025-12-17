---
title: MySQL Database Configuration
description: Configure MySQL database for SkyCMS with connection strings and setup instructions
keywords: MySQL, database, configuration, connection-string, relational
audience: [developers, administrators]
---

# MySQL with SkyCMS

MySQL is a popular open-source relational database, widely available on shared hosting and cloud platforms. SkyCMS works seamlessly with MySQL.

## Values you need

- **Server**: Hostname or IP of your MySQL server (e.g., `mydb.mysql.database.azure.com` for Azure Database for MySQL)
- **Database**: Database name
- **User**: MySQL username
- **Password**: MySQL password
- **Port**: Usually 3306 (default)

## Create a MySQL Database

### Azure Database for MySQL (managed)

1. **Azure Portal** → **Create a resource** → search **"Azure Database for MySQL"** → **Create**.
2. Choose **Single Server** or **Flexible Server** (Flexible is newer).
3. Fill in:
   - **Server name**: Globally unique name (becomes `servername.mysql.database.azure.com`)
   - **Admin username**: Root/admin user
   - **Password**: Strong password
   - **Location**: Choose a region
4. Configure **Compute + storage** as needed.
5. Click **Create** and wait for deployment.

### Self-hosted MySQL

- Use MySQL Community Server or a container image.
- Ensure the server is reachable from your SkyCMS application.
- Create a database and user with appropriate privileges.

## Get your connection string

### From Azure Portal (Azure Database for MySQL)

1. Portal → **Azure Database for MySQL** → select your server.
2. Click **Connection strings** → copy **JDBC** or construct manually:
   ```
   Server=myserver.mysql.database.azure.com;Uid=admin@myserver;Pwd=yourpassword;Database=mydb;
   ```

### From MySQL Command Line (self-hosted)

```bash
mysql -h localhost -u root -p
# Then create database and user:
CREATE DATABASE skycms;
CREATE USER 'skycms'@'localhost' IDENTIFIED BY 'password';
GRANT ALL PRIVILEGES ON skycms.* TO 'skycms'@'localhost';
```

## Configure in SkyCMS

### Using the Setup Wizard (recommended)

1. Deploy SkyCMS with `CosmosAllowSetup=true`.
2. Open the Editor setup wizard.
3. When prompted for **Database**, paste the connection string:
   ```
   Server=myserver.mysql.database.azure.com;Uid=admin@myserver;Pwd=yourpassword;Database=mydb;
   ```
4. Click **Validate** and proceed.

### Manual Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "ApplicationDbContextConnection": "Server=myserver.mysql.database.azure.com;Uid=admin@myserver;Pwd=yourpassword;Database=mydb;"
  }
}
```

### Local Development (self-hosted MySQL)

```json
{
  "ConnectionStrings": {
    "ApplicationDbContextConnection": "Server=localhost;Port=3306;Uid=root;Pwd=password;Database=skycms;"
  }
}
```

### Environment Variables

```powershell
$env:ConnectionStrings__ApplicationDbContextConnection = "Server=myserver.mysql.database.azure.com;Uid=admin@myserver;Pwd=yourpassword;Database=mydb;"
```

## Best practices

- **Separate admin and app users**: Create a dedicated MySQL user for SkyCMS with limited privileges (not root).
- **Secure passwords** in Azure Key Vault, environment variables, or secrets manager.
- **Enable SSL/TLS** for connections (especially on Azure Database for MySQL).
- **Backup strategy**: Set up automated backups and test restore procedures.
- **Rotate credentials** periodically.
- **Monitor performance**: Azure Database for MySQL provides metrics and slow query logs.

## Tips and troubleshooting

- Connection string format uses lowercase `server=` and `uid=` for MySQL (unlike SQL Server).
- For Azure Database for MySQL, the user often includes the server name: `admin@servername`.
- If connection fails, verify the hostname, user, password, and firewall rules.
- MySQL has different feature support than SQL Server; ensure your SkyCMS customizations use compatible SQL.

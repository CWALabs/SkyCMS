---
title: Setup Wizard - Welcome
description: Welcome screen for SkyCMS interactive setup wizard
keywords: setup-wizard, welcome, installation, configuration
audience: [developers, administrators]
---

# Setup Wizard: Welcome Screen

**Step 0 of 7** | [Next: Storage Configuration →](./SetupWizard-Step1-Storage.md)

---

## Welcome to SkyCMS Setup

This is the welcome screen that appears when you first access the setup wizard.

![Setup Welcome Screen](../assets/setup-welcome.png) *(Screenshot placeholder)*

---

## What You'll See

### Setup Wizard Will Configure

The welcome screen explains what the wizard will help you configure:

✅ **Storage** - Where your files and media are stored  
✅ **Admin Account** - Your administrator login  
✅ **Publisher Settings** - Website URL and options  
✅ **Email** (Optional) - Transactional email provider  
✅ **CDN** (Optional) - Content delivery and cache purging  

### Prerequisites Check

Before proceeding, the wizard verifies:

- ✅ **Database Connection** - `ConnectionStrings:ApplicationDbContextConnection` is configured
- ✅ **Setup Allowed** - `CosmosAllowSetup=true` is set
- ✅ **No Existing Setup** - Setup hasn't already been completed

---

## Prerequisites

### Required Configuration

**Minimum appsettings.json** or **environment variables**:

```json
{
  "CosmosAllowSetup": true,
  "ConnectionStrings": {
    "ApplicationDbContextConnection": "Data Source=D:\\data\\cosmoscms.db"
  }
}
```

### Database Options

Choose one:

**SQLite** (easiest for development):
```json
{
  "ConnectionStrings": {
    "ApplicationDbContextConnection": "Data Source=D:\\data\\cosmoscms.db"
  }
}
```

**SQL Server / Azure SQL**:
```json
{
  "ConnectionStrings": {
    "ApplicationDbContextConnection": "Server=myserver.database.windows.net;Database=cosmoscms;User Id=adminuser;Password=xxxxx;"
  }
}
```

**Azure Cosmos DB**:
```json
{
  "ConnectionStrings": {
    "ApplicationDbContextConnection": "AccountEndpoint=https://myaccount.documents.azure.com:443/;AccountKey=xxxxx;Database=cosmoscms;"
  }
}
```

**MySQL**:
```json
{
  "ConnectionStrings": {
    "ApplicationDbContextConnection": "Server=myserver;Database=cosmoscms;User=root;Password=xxxxx;"
  }
}
```

See [Database Configuration](../Configuration/Database-Overview.md) for complete details.

---

## Actions

### "Start Setup" Button

Click **Start Setup** to begin the wizard.

This will:
1. Create a new setup session in the database
2. Initialize default values
3. Redirect you to **Step 1: Storage Configuration**

---

## What Happens Next

After clicking **Start Setup**, you'll proceed to:

**[Step 1: Storage Configuration →](./SetupWizard-Step1-Storage.md)**

---

## Troubleshooting

### "Setup not allowed" Error

**Cause**: `CosmosAllowSetup` is not set to `true`.

**Solution**:

```powershell
# PowerShell
$env:CosmosAllowSetup = "true"
dotnet run

# Or in appsettings.json
{
  "CosmosAllowSetup": true
}
```

### "Database connection string not found" Error

**Cause**: `ConnectionStrings:ApplicationDbContextConnection` is missing.

**Solution**:

Add the database connection string to appsettings.json or environment variables:

```json
{
  "ConnectionStrings": {
    "ApplicationDbContextConnection": "Data Source=D:\\data\\cosmoscms.db"
  }
}
```

### Redirected to Home Page Instead of Setup

**Cause**: Setup has already been completed.

**Solutions**:

1. **Use existing setup**: Login with your admin account
2. **Re-run setup**: Delete the database and start fresh
3. **Manually configure**: See [Minimum Required Settings](./MinimumRequiredSettings.md)

### Page Loads But "Start Setup" Doesn't Work

**Check**:
1. Browser console for JavaScript errors (press F12)
2. Network tab for failed requests
3. Application logs for server-side errors

**Common causes**:
- Database connection failed
- Database permissions insufficient
- Application errors during initialization

---

## See Also

- **[Setup Wizard Overview](./SetupWizard.md)** - Complete wizard guide
- **[Next: Storage Configuration →](./SetupWizard-Step1-Storage.md)**
- **[Minimum Required Settings](./MinimumRequiredSettings.md)** - Manual configuration
- **[Database Configuration](../Configuration/Database-Overview.md)** - Database setup

# Azure Deployment for SkyCMS

Complete Azure infrastructure deployment for SkyCMS using Bicep Infrastructure as Code.

> **December 2025 Update**
> - Azure Container Apps provides serverless container hosting with built-in HTTPS endpoints
> - MySQL Flexible Server with TLS enforcement and automated backups
> - Azure Key Vault for secure secrets management with RBAC
> - Optional Blob Storage with static website hosting for publisher
> - Managed Identity for passwordless authentication between services
> - Similar architecture to AWS deployment but optimized for Azure

## 📁 Script Overview

### Deployment Files

| File | Purpose |
|------|---------|
| **deploy-skycms.ps1** | Interactive PowerShell deployment script |
| **destroy-skycms.ps1** | Teardown script to delete all resources |
| **validate-bicep.ps1** | Bicep template validation |
| **helpers.ps1** | Common management operations (logs, restart, scale, etc.) |
| **bicep/main.bicep** | Main orchestration template |
| **bicep/modules/\*.bicep** | Modular Bicep templates for each resource type |

### Bicep Modules

| Module | Resources | Purpose |
|--------|-----------|---------|
| **containerApp.bicep** | Container Apps + Environment | Host SkyCMS Editor |
| **mysql.bicep** | Azure Database for MySQL | Managed database with TLS |
| **keyVault.bicep** | Key Vault | Secure secrets storage |
| **storage.bicep** | Blob Storage | Static website hosting (optional) |

---

## 🚀 Quick Start

### Prerequisites

- Azure CLI installed ([Download](https://aka.ms/installazurecliwindows))
- Active Azure subscription
- PowerShell 5.1 or later

### Deploy SkyCMS

```powershell
# Navigate to Azure scripts directory
cd D:\source\SkyCMS\InstallScripts\Azure

# Run interactive deployment
.\deploy-skycms.ps1
```

**You'll be prompted for:**
- Resource group name
- Azure region
- Base name for resources
- Environment (dev/staging/prod)
- Docker image (default: `toiyabe/sky-editor:latest`)
- MySQL database name and password
- Container scaling settings (min/max replicas)
- Whether to deploy publisher (Blob Storage)

⏱️ **Deployment time:** 10-15 minutes (MySQL provisioning is the slowest)

---

## 📦 What Gets Deployed

### Core Infrastructure (Always)

| Resource | Type | Purpose | Cost (Est/Month) |
|----------|------|---------|------------------|
| **Container App** | Microsoft.App/containerApps | Editor application | ~$15-30 (0.5 vCPU, 1GB RAM) |
| **Container App Environment** | Microsoft.App/managedEnvironments | Shared platform | ~$5 |
| **MySQL Flexible Server** | Microsoft.DBforMySQL/flexibleServers | Database | ~$10-20 (Burstable B1ms) |
| **Key Vault** | Microsoft.KeyVault/vaults | Secrets storage | ~$0.50 |
| **Managed Identity** | Microsoft.ManagedIdentity/userAssignedIdentities | Service authentication | Free |

**Total Core:** ~$30-55/month (dev environment)

### Optional Publisher

| Resource | Type | Purpose | Cost (Est/Month) |
|----------|------|---------|------------------|
| **Storage Account** | Microsoft.Storage/storageAccounts | Static website hosting | ~$1-5 (depends on usage) |

---

## 🌐 Deployed Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      AZURE SUBSCRIPTION                      │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │           RESOURCE GROUP (rg-skycms-dev)            │   │
│  │                                                       │   │
│  │  ┌──────────────────────────────────────────┐      │   │
│  │  │     Container Apps Environment            │      │   │
│  │  │  ┌────────────────────────────────────┐  │      │   │
│  │  │  │   Container App (Editor)            │  │      │   │
│  │  │  │   • Docker: toiyabe/sky-editor      │  │      │   │
│  │  │  │   • HTTPS: *.azurecontainerapps.io  │  │      │   │
│  │  │  │   • Auto-scaling: 1-3 replicas      │  │      │   │
│  │  │  └────────────────────────────────────┘  │      │   │
│  │  └──────────────────────────────────────────┘      │   │
│  │          │                                           │   │
│  │          │ (Managed Identity)                       │   │
│  │          ↓                                           │   │
│  │  ┌──────────────────┐    ┌─────────────────────┐  │   │
│  │  │   Key Vault      │    │  MySQL Flexible     │  │   │
│  │  │   • Secrets      │    │  • TLS Required     │  │   │
│  │  │   • RBAC Auth    │    │  • Backup: 7 days   │  │   │
│  │  └──────────────────┘    └─────────────────────┘  │   │
│  │          │                         │                │   │
│  │          │                         │ Port 3306      │   │
│  │          │ Passwordless            │ (Firewall)     │   │
│  │          ↓                         ↓                │   │
│  │  ┌──────────────────────────────────────────┐      │   │
│  │  │          Blob Storage (Optional)          │      │   │
│  │  │  • Static Website Enabled                 │      │   │
│  │  │  • Public HTTPS endpoint                  │      │   │
│  │  └──────────────────────────────────────────┘      │   │
│  │                                                       │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔐 Security Features

- ✅ **TLS Everywhere** - MySQL requires TLS, Container Apps use HTTPS only
- ✅ **Managed Identity** - No passwords for service-to-service auth
- ✅ **Key Vault RBAC** - Role-based access control for secrets
- ✅ **Secure Secrets** - DB credentials stored in Key Vault, never in code
- ✅ **Network Isolation** - MySQL firewall rules (configurable)
- ✅ **Soft Delete** - Key Vault secrets recoverable for 7 days

---

## 📋 Post-Deployment Steps

### 1. Access the Editor

After deployment completes, you'll see output like:

```
✅ SkyCMS Editor URL: https://ca-skycms-editor-dev.kindocean-abc123.eastus.azurecontainerapps.io
```

Wait 1-2 minutes for the container to fully start, then visit the URL.

### 2. Complete Setup Wizard

The SkyCMS setup wizard will guide you through:
- ✅ Database connection (pre-configured)
- 🔑 Storage configuration (Azure Blob if deployed)
- 👤 Admin account creation
- 🌐 Publisher settings

### 3. Upload Publisher Files (Optional)

If you deployed Blob Storage:

```powershell
# Using helpers script
.\helpers.ps1 -Action UploadToStorage -ResourceGroupName "rg-skycms-dev" -StorageAccountName "st<name>" -SourcePath "./website"

# Or using Azure CLI directly
az storage blob upload-batch `
    --account-name $storageAccount `
    --source ./website `
    --destination '$web' `
    --auth-mode login
```

### 4. Manage Your Deployment

Use the helpers script for common operations:

```powershell
# Interactive menu
.\helpers.ps1

# Or specify action directly
.\helpers.ps1 -Action ViewLogs -ResourceGroupName "rg-skycms-dev"
.\helpers.ps1 -Action RestartContainerApp -ResourceGroupName "rg-skycms-dev" -ContainerAppName "ca-skycms-editor-dev"
.\helpers.ps1 -Action ScaleContainerApp -ResourceGroupName "rg-skycms-dev" -MinReplicas 0 -MaxReplicas 5
```

**Available Actions:**
- `ViewLogs` - Stream container app logs
- `RestartContainerApp` - Restart the editor application
- `ScaleContainerApp` - Adjust min/max replicas
- `GetConnectionString` - Display MySQL connection info
- `EnableStaticWebsite` - Configure blob storage for static hosting
- `UploadToStorage` - Upload files to blob storage
- `ListResources` - Show all resources in resource group

---

## 🗑️ Teardown

### Delete Everything

```powershell
# Interactive teardown (with confirmation)
.\destroy-skycms.ps1

# Or specify resource group
.\destroy-skycms.ps1 -ResourceGroupName "rg-skycms-dev"

# Force delete (no prompts)
.\destroy-skycms.ps1 -ResourceGroupName "rg-skycms-dev" -Force
```

**What gets deleted:**
- Container Apps and environments
- MySQL database (ALL data lost)
- Key Vault and all secrets
- Storage accounts and blobs
- Managed identities
- The entire resource group

---

## 🔧 Customization

### Environment Variables

Edit [bicep/modules/containerApp.bicep](bicep/modules/containerApp.bicep) to add environment variables:

```bicep
{
  name: 'CUSTOM_VARIABLE'
  value: 'custom_value'
}
```

### Scaling Configuration

Modify in [bicep/main.bicep](bicep/main.bicep):

```bicep
minReplicas: 0  // Scale to zero when idle (cost savings)
maxReplicas: 10 // Handle traffic spikes
```

### MySQL SKU

For production, upgrade in [bicep/modules/mysql.bicep](bicep/modules/mysql.bicep):

```bicep
skuName: 'Standard_D2ds_v4'  // 2 vCPU, 8GB RAM
skuTier: 'GeneralPurpose'
```

---

## 🆚 AWS vs Azure Comparison

| Feature | AWS | Azure |
|---------|-----|-------|
| **Compute** | ECS Fargate + ALB + CloudFront | Container Apps (all-in-one) |
| **Database** | RDS MySQL | Azure Database for MySQL |
| **Secrets** | Secrets Manager | Key Vault |
| **Storage** | S3 + CloudFront | Blob Storage (+ optional Front Door) |
| **Auth** | IAM Roles | Managed Identity + RBAC |
| **IaC** | AWS CDK (TypeScript) | Bicep |
| **HTTPS** | Manual CloudFront config | Built-in (Container Apps) |
| **Cost (Dev)** | ~$40-60/month | ~$30-55/month |

**Azure Advantages:**
- Simpler networking (no VPC/subnets required)
- Built-in HTTPS endpoints
- Lower idle costs (scale to zero)
- Unified RBAC model

---

## 📖 Additional Documentation

- [Azure Container Apps Docs](https://learn.microsoft.com/azure/container-apps/)
- [Azure Database for MySQL](https://learn.microsoft.com/azure/mysql/flexible-server/)
- [Bicep Documentation](https://learn.microsoft.com/azure/azure-resource-manager/bicep/)
- [Azure Key Vault Best Practices](https://learn.microsoft.com/azure/key-vault/general/best-practices)

---

## 🐛 Troubleshooting

| Problem | Solution |
|---------|----------|
| Deployment fails | Check Azure CLI is logged in: `az account show` |
| MySQL connection fails | Verify firewall rules in Azure Portal |
| Container App not starting | Check logs: `az containerapp logs show --name <app-name> --resource-group <rg-name>` |
| Static website not enabled | Run: `az storage blob service-properties update --account-name <name> --static-website` |
| Key Vault access denied | Check Managed Identity has correct RBAC role |

---

## 💡 Tips

- **Development:** Use `minReplicas: 0` to scale to zero and save costs
- **Production:** Enable MySQL geo-redundant backup and increase SKU
- **Monitoring:** Enable Application Insights for detailed telemetry
- **Custom Domains:** Add custom domains via Container Apps portal
- **Cost Control:** Set up Azure Cost Management alerts

---

## 📝 License

Same as SkyCMS project license.

# Azure Deployment for SkyCMS

Complete Azure infrastructure deployment for SkyCMS using Bicep Infrastructure as Code.

> **January 2026 Update**
> - Azure App Service with Web Apps for Containers provides managed container hosting
> - Built-in HTTPS endpoints with automatic certificate management
> - Azure SQL Database with TLS enforcement and automated backups
> - Azure Key Vault for secure secrets management with RBAC
> - Deployment slots for zero-downtime updates with auto-swap
> - Health check monitoring on `/___healthz` endpoint
> - Optional Blob Storage with static website hosting for publisher
> - Managed Identity for passwordless authentication between services

## � One-Click Deploy

**Click the button below to deploy directly to Azure Portal (no local tools needed):**

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FCWALabs%2FSkyCMS%2Fmain%2FInstallScripts%2FAzure%2Fbicep%2Fmain.bicep)

**First time?** See the [Post-Deployment Quick Start Guide](./QUICKSTART_DEPLOY_BUTTON.md) after deployment completes.

---

## �📁 Script Overview
### 🚀 Quick Deploy Button

**Prefer one-click deployment?** Click the Deploy to Azure button at the top of this README to deploy directly from the Azure Portal without installing any tools.
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
| **webApp.bicep** | App Service + App Service Plan | Host SkyCMS Editor with deployment slots |
| **sqlDatabase.bicep** | Azure SQL Database + SQL Server | Managed database with TLS |
| **keyVault.bicep** | Key Vault | Secure secrets storage with RBAC |
| **storage.bicep** | Blob Storage | Static website hosting (optional) |
| **acs.bicep** | Communication Services | Email sending (optional) |
| **applicationInsights.bicep** | App Insights + Log Analytics | Monitoring and telemetry (optional) |

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
- Base name for resources (3-10 characters, alphanumeric)
- Environment (dev/staging/prod)
- Docker image (default: `toiyabe/sky-editor:latest`)
- Minimum App Service instances (for scaling)
- Whether to deploy publisher (Blob Storage)
- Whether to deploy email (Azure Communication Services)
- Whether to deploy Application Insights monitoring
- Whether to create staging deployment slot

⏱️ **Deployment time:** 10-15 minutes (SQL provisioning is the slowest)

**About Deployment Slots:**
- **First deployment:** Goes directly to **production slot** - your editor URL works immediately for setup
- **Staging slot** is created alongside but remains empty initially
- **Future updates:** Deploy to staging → health check validates → auto-swaps to production
- **Zero-downtime:** Users never experience downtime during updates

---

## 📦 What Gets Deployed

### Core Infrastructure (Always)

| Resource | Type | Purpose | Cost (Est/Month) |
|----------|------|---------|------------------|
| **App Service** | Microsoft.Web/sites | Editor application | ~$55-75 (P1v3 plan) |
| **App Service Plan** | Microsoft.Web/serverfarms | Compute platform | Included in App Service |
| **Azure SQL Database** | Microsoft.Sql/servers/databases | Database | ~$5-15 (Basic tier) |
| **Key Vault** | Microsoft.KeyVault/vaults | Secrets storage | ~$0.50 |
| **Managed Identity** | Microsoft.ManagedIdentity/userAssignedIdentities | Service authentication | Free |
| **Deployment Slot (Staging)** | Microsoft.Web/sites/slots | Zero-downtime deployments | Included in App Service |

**Total Core:** ~$60-90/month (dev environment with P1v3)

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
│  │  │     App Service Plan (P1v3 Linux)        │      │   │
│  │  │  ┌────────────────────────────────────┐  │      │   │
│  │  │  │   App Service (Editor - Production) │  │      │   │
│  │  │  │   • Docker: toiyabe/sky-editor      │  │      │   │
│  │  │  │   • HTTPS: *.azurewebsites.net      │  │      │   │
│  │  │  │   • Health: /___healthz             │  │      │   │
│  │  │  │   • AlwaysOn: Enabled               │  │      │   │
│  │  │  └────────────────────────────────────┘  │      │   │
│  │  │  ┌────────────────────────────────────┐  │      │   │
│  │  │  │   Deployment Slot (Staging)         │  │      │   │
│  │  │  │   • Auto-swap after health check    │  │      │   │
│  │  │  │   • Zero-downtime deployments       │  │      │   │
│  │  │  └────────────────────────────────────┘  │      │   │
│  │  └──────────────────────────────────────────┘      │   │
│  │          │                                           │   │
│  │          │ (Managed Identity)                       │   │
│  │          ↓                                           │   │
│  │  ┌──────────────────┐    ┌─────────────────────┐  │   │
│  │  │   Key Vault      │    │  Azure SQL Database │  │   │
│  │  │   • Secrets      │    │  • TLS Required     │  │   │
│  │  │   • RBAC Auth    │    │  • Auto-backup      │  │   │
│  │  │   • Soft Delete  │    │  • Geo-redundant    │  │   │
│  │  └──────────────────┘    └─────────────────────┘  │   │
│  │          │                         │                │   │
│  │          │ Key Vault References    │ Port 1433      │   │
│  │          │ (ConnectionStrings)     │ (Firewall)     │   │
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

- ✅ **TLS Everywhere** - Azure SQL requires TLS, App Service uses HTTPS only
- ✅ **Managed Identity** - No passwords for service-to-service auth
- ✅ **Key Vault RBAC** - Role-based access control for secrets
- ✅ **Key Vault References** - Secrets loaded dynamically via `@Microsoft.KeyVault()` syntax
- ✅ **Secure Secrets** - All credentials stored in Key Vault, never in code or env vars
- ✅ **Network Isolation** - SQL firewall rules (configurable)
- ✅ **Soft Delete** - Key Vault secrets recoverable for 7-90 days
- ✅ **Health Monitoring** - Continuous health checks on `/___healthz` endpoint
- ✅ **Zero-Downtime Deployments** - Staging slot with health validation before swap

---

## 📋 Post-Deployment Steps

### 1. Access the Editor

After deployment completes, you'll see output like:

```
✅ SkyCMS Editor URL: https://editor-skycms-dev-abc12345.azurewebsites.net
```

The health check ensures the app is ready - typically available within 30-60 seconds.

**Note:** The production slot is what you're accessing. The staging slot is created but empty on first deployment - it's only used for future zero-downtime updates.

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
.\helpers.ps1 -Action RestartWebApp -ResourceGroupName "rg-skycms-dev" -WebAppName "editor-skycms-dev-abc12345"
.\helpers.ps1 -Action ScaleWebApp -ResourceGroupName "rg-skycms-dev" -Instances 2
.\helpers.ps1 -Action SwapSlot -ResourceGroupName "rg-skycms-dev" -WebAppName "editor-skycms-dev-abc12345"
```

**Available Actions:**
- `ViewLogs` - Stream web app logs
- `RestartWebApp` - Restart the editor application
- `ScaleWebApp` - Adjust instance count
- `SwapSlot` - Manually swap staging to production
- `GetConnectionString` - Display SQL connection info
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
- App Service and deployment slots
- App Service Plan
- Azure SQL Database and SQL Server (ALL data lost)
- Key Vault and all secrets (soft-deleted, recoverable)
- Storage accounts and blobs
- Managed identities
- The entire resource group

---

## 🔧 Customization

### Environment Variables

Edit [bicep/modules/webApp.bicep](bicep/modules/webApp.bicep) to add environment variables:

```bicep
{
  name: 'CUSTOM_VARIABLE'
  value: 'custom_value'
}
```

### App Service SKU

Modify in [bicep/main.bicep](bicep/main.bicep) for production:

```bicep
skuName: environment == 'prod' ? 'P2v3' : 'P1v3'  // Scale up for production
skuTier: 'PremiumV3'
capacity: max(2, minReplicas)  // At least 2 instances for HA
```

### Azure SQL SKU

For production, upgrade in [bicep/modules/sqlDatabase.bicep](bicep/modules/sqlDatabase.bicep):

```bicep
skuName: 'S1'  // Standard tier
tier: 'Standard'
capacity: 20   // 20 DTUs
```

---

## 🆚 AWS vs Azure Comparison

| Feature | AWS | Azure |
|---------|-----|-------|
| **Compute** | ECS Fargate + ALB + CloudFront | App Service (all-in-one) |
| **Database** | RDS MySQL | Azure SQL Database |
| **Secrets** | Secrets Manager | Key Vault + Key Vault References |
| **Storage** | S3 + CloudFront | Blob Storage (+ optional Front Door) |
| **Auth** | IAM Roles | Managed Identity + RBAC |
| **IaC** | AWS CDK (TypeScript) | Bicep |
| **HTTPS** | Manual CloudFront config | Built-in with auto-certs |
| **Deployment** | Manual blue/green | Deployment slots + auto-swap |
| **Health Checks** | Target group health | Built-in health monitoring |
| **Cost (Dev)** | ~$40-60/month | ~$60-90/month |

**Azure Advantages:**
- Simpler networking (no VPC/subnets required)
- Built-in HTTPS endpoints with auto-renewal
- Zero-downtime deployments via slots
- Integrated health monitoring
- Unified RBAC model

---

## 📖 Additional Documentation

- [Azure App Service Docs](https://learn.microsoft.com/azure/app-service/)
- [App Service Deployment Slots](https://learn.microsoft.com/azure/app-service/deploy-staging-slots)
- [Azure SQL Database](https://learn.microsoft.com/azure/azure-sql/database/)
- [Key Vault References in App Service](https://learn.microsoft.com/azure/app-service/app-service-key-vault-references)
- [Bicep Documentation](https://learn.microsoft.com/azure/azure-resource-manager/bicep/)
- [Azure Key Vault Best Practices](https://learn.microsoft.com/azure/key-vault/general/best-practices)

---

## 🐛 Troubleshooting

| Problem | Solution |
|---------|----------|
| Deployment fails | Check Azure CLI is logged in: `az account show` |
| SQL connection fails | Verify firewall rules in Azure Portal; check Key Vault secrets |
| App Service not starting | Check logs: `az webapp log tail --name <app-name> --resource-group <rg-name>` |
| Health check failing | Verify `/___healthz` endpoint responds with HTTP 200 |
| Key Vault reference error | Check Managed Identity has "Key Vault Secrets Officer" role |
| Slot swap fails | Health check must pass in staging before swap |
| Static website not enabled | Run: `az storage blob service-properties update --account-name <name> --static-website` |

---

## 💡 Tips

- **First Deployment:** Production slot is ready immediately - complete setup wizard without delays
- **Updates After Setup:** Deploy new images to staging slot for automatic zero-downtime updates
- **Development:** Use Basic/Standard tier for lower costs; upgrade to Premium for production
- **Production:** Enable SQL geo-redundant backup and increase SKU to S1 or higher
- **Deployment Workflow:** 
  ```bash
  # Push new image to staging
  az webapp config container set --name <app-name> --resource-group <rg-name> \
    --docker-custom-image-name toiyabe/sky-editor:v2 --slot staging
  
  # Auto-swap happens after health check passes
  # Or manually swap: az webapp deployment slot swap --name <app-name> --resource-group <rg-name> --slot staging
  ```
- **Monitoring:** Enable Application Insights for detailed telemetry and performance monitoring
- **Custom Domains:** Add custom domains via App Service portal with free managed certificates
- **Health Checks:** Monitor `/___healthz` endpoint in Application Insights
- **Cost Control:** Set up Azure Cost Management alerts and auto-shutdown for dev environments

---

## 📝 License

Same as SkyCMS project license.

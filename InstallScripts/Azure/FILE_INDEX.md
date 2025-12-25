# SkyCMS Azure Deployment - File Index

Complete reference for all files in the Azure deployment infrastructure.

## 📁 Directory Structure

```
InstallScripts/Azure/
├── bicep/
│   ├── main.bicep                      # Main orchestration template
│   ├── modules/
│   │   ├── containerApp.bicep          # Container Apps module
│   │   ├── keyVault.bicep              # Key Vault module
│   │   ├── mysql.bicep                 # MySQL Flexible Server module
│   │   └── storage.bicep               # Blob Storage module
│   └── parameters/
│       ├── README.md                   # Parameter file documentation
│       ├── dev.bicepparam              # Dev environment parameters
│       └── prod.bicepparam             # Prod environment parameters
├── deploy-skycms.ps1                   # Interactive deployment script
├── destroy-skycms.ps1                  # Teardown script
├── validate-bicep.ps1                  # Bicep validation script
├── helpers.ps1                         # Common management operations
├── README.md                           # Full documentation
├── QUICK_START.md                      # Quick start guide
├── AWS_VS_AZURE.md                     # Architecture comparison
├── DEPLOYMENT_SUMMARY.md               # Deployment summary
├── .gitignore                          # Git ignore rules
└── FILE_INDEX.md                       # This file
```

---

## 📝 File Descriptions

### Bicep Templates

#### [bicep/main.bicep](bicep/main.bicep)
**Purpose:** Main orchestration template that ties all modules together  
**Resources Created:**
- Managed Identity (for passwordless auth)
- Resource Group references
- Orchestrates all module deployments
- Defines outputs for deployment results

**Key Parameters:**
- `baseName` - Base name for all resources
- `environment` - Environment type (dev/staging/prod)
- `deployPublisher` - Whether to deploy Blob Storage
- `mysqlAdminPassword` - MySQL admin password (secure)
- `dockerImage` - Container image to deploy

**Outputs:**
- Editor URL (Container Apps endpoint)
- MySQL connection details
- Key Vault name
- Storage account details

---

#### [bicep/modules/containerApp.bicep](bicep/modules/containerApp.bicep)
**Purpose:** Azure Container Apps deployment for SkyCMS Editor  
**Resources Created:**
- Container Apps Environment
- Container App (Editor application)
- Auto-scaling rules
- Environment variables configuration
- Secret references to Key Vault

**Features:**
- HTTPS endpoint (automatic)
- Auto-scaling (1-3 replicas by default)
- Pulls from Docker Hub
- Managed Identity integration
- Key Vault secret injection

**Environment Variables Set:**
- `CosmosAllowSetup=true`
- `MultiTenantEditor=false`
- `ASPNETCORE_ENVIRONMENT=Development`
- `BlobServiceProvider=Azure`
- MySQL connection details
- Connection string from Key Vault

---

#### [bicep/modules/keyVault.bicep](bicep/modules/keyVault.bicep)
**Purpose:** Azure Key Vault for secrets management  
**Resources Created:**
- Key Vault instance
- RBAC role assignment (Secrets Officer)

**Features:**
- RBAC-based authorization
- Soft delete enabled (7-day retention)
- Optional purge protection (prod)
- Public network access (can be restricted)

**Secrets Stored:**
- MySQL connection string
- (Can be extended for other secrets)

---

#### [bicep/modules/mysql.bicep](bicep/modules/mysql.bicep)
**Purpose:** Azure Database for MySQL - Flexible Server  
**Resources Created:**
- MySQL Flexible Server
- Server configuration (TLS enforcement)
- Firewall rules
- Database creation

**Features:**
- MySQL 8.0.21
- TLS required (secure connections only)
- Burstable B1ms SKU (dev) or higher (prod)
- 7-30 day backups (environment-dependent)
- Auto-grow storage enabled

**Firewall Rules:**
- Allow Azure services (0.0.0.0)
- Allow all IPs (dev only - should be restricted for prod)

**Outputs:**
- Server FQDN
- Connection string template
- Database name

---

#### [bicep/modules/storage.bicep](bicep/modules/storage.bicep)
**Purpose:** Azure Blob Storage for static website hosting  
**Resources Created:**
- Storage Account (StorageV2)
- Blob Service configuration

**Features:**
- Static website hosting enabled
- HTTPS only
- TLS 1.2 minimum
- Hot access tier
- 7-day soft delete for blobs

**Note:** Static website must be enabled via Azure CLI after deployment

**Outputs:**
- Storage account name
- Primary web endpoint
- CLI command to enable static website

---

### PowerShell Scripts

#### [deploy-skycms.ps1](deploy-skycms.ps1)
**Purpose:** Interactive deployment script  
**Size:** ~250 lines

**What It Does:**
1. Checks prerequisites (Azure CLI, login status)
2. Prompts for deployment parameters
3. Creates resource group (if needed)
4. Deploys Bicep templates
5. Enables static website hosting (if publisher deployed)
6. Displays deployment results and next steps

**Required:**
- Azure CLI installed
- Azure subscription access
- PowerShell 5.1+

**Interactive Prompts:**
- Resource group name
- Azure region
- Base name
- Environment type
- Docker image
- MySQL password
- Scaling settings
- Publisher deployment option

---

#### [destroy-skycms.ps1](destroy-skycms.ps1)
**Purpose:** Teardown script to delete all resources  
**Size:** ~180 lines

**What It Does:**
1. Lists available resource groups
2. Prompts for resource group to delete
3. Shows all resources to be deleted
4. Requires typed confirmation (safety)
5. Deletes resource group (async)

**Safety Features:**
- Must type resource group name to confirm
- Shows all resources before deletion
- Double confirmation prompt
- Can be run with `-Force` to skip prompts

---

#### [validate-bicep.ps1](validate-bicep.ps1)
**Purpose:** Validate Bicep templates before deployment  
**Size:** ~150 lines

**What It Does:**
1. Checks Azure CLI and Bicep installation
2. Runs `az bicep build` on all templates
3. Validates syntax and schema
4. Optional: Runs what-if analysis

**Usage:**
```powershell
# Validate syntax only
.\validate-bicep.ps1

# Validate + what-if analysis
.\validate-bicep.ps1 -ResourceGroupName "rg-skycms-dev"
```

---

### Documentation

#### [README.md](README.md)
**Purpose:** Complete documentation  
**Size:** ~600 lines

**Sections:**
- Overview and architecture
- Quick start guide
- What gets deployed
- Security features
- Post-deployment steps
- Customization options
- Teardown instructions
- Troubleshooting
- Cost estimates
- AWS comparison

---

#### [helpers.ps1](helpers.ps1)
**Purpose:** Common management operations script  
**Size:** ~250 lines

**What It Does:**
1. Provides menu-driven interface for common tasks
2. View container app logs
3. Restart container app
4. Scale container app (change min/max replicas)
5. Get MySQL connection information
6. Enable static website on storage
7. Upload files to blob storage
8. List all resources

**Usage:**
```powershell
# Interactive menu
.\helpers.ps1

# Direct commands
.\helpers.ps1 -Action ViewLogs -ResourceGroupName "rg-skycms" -ContainerAppName "ca-skycms-editor-dev"
.\helpers.ps1 -Action ScaleContainerApp -MinReplicas 0 -MaxReplicas 5
```

**Available Actions:**
- ViewLogs
- RestartContainerApp
- ScaleContainerApp
- GetConnectionString
- EnableStaticWebsite
- UploadToStorage
- ListResources

---

#### [QUICK_START.md](QUICK_START.md)
**Purpose:** Condensed quick start guide  
**Size:** ~150 lines

**Focus:**
- One-command deployment
- What you'll be asked
- Expected output
- Next steps
- Common issues

---

#### [AWS_VS_AZURE.md](AWS_VS_AZURE.md)
**Purpose:** Architecture comparison  
**Size:** ~400 lines

**Comparison Topics:**
- Infrastructure components
- Deployment architecture
- Code examples
- Networking complexity
- Security features
- Cost analysis
- Scaling approaches
- Migration path

---

## 🚀 Quick Reference

### Deploy SkyCMS
```powershell
cd InstallScripts\Azure
.\deploy-skycms.ps1
```

### Validate Templates
```powershell
.\validate-bicep.ps1
```

### Delete Everything
```powershell
.\destroy-skycms.ps1
```

### Manual Deployment (without script)
```powershell
az group create --name rg-skycms --location eastus

az deployment group create `
    --resource-group rg-skycms `
    --template-file bicep/main.bicep `
    --parameters baseName=skycms environment=dev mysqlAdminPassword='SecurePass123!'
```

---

## 📊 Resource Dependencies

```
Managed Identity
    ↓
Key Vault (requires Managed Identity for RBAC)
    ↓
MySQL (credentials stored in Key Vault)
    ↓
Storage Account (optional, accessed by Managed Identity)
    ↓
Container App (depends on all above)
```

---

## 🔧 Customization Guide

### Change Container Image
Edit [bicep/main.bicep](bicep/main.bicep):
```bicep
param dockerImage string = 'your-registry/your-image:tag'
```

### Add Environment Variables
Edit [bicep/modules/containerApp.bicep](bicep/modules/containerApp.bicep):
```bicep
{
  name: 'NEW_VARIABLE'
  value: 'value'
}
```

### Adjust Scaling
Edit [bicep/main.bicep](bicep/main.bicep):
```bicep
param minReplicas int = 0  // Scale to zero
param maxReplicas int = 10 // Handle more traffic
```

### Change MySQL SKU
Edit [bicep/modules/mysql.bicep](bicep/modules/mysql.bicep):
```bicep
skuName: 'Standard_D2ds_v4'  // More powerful
skuTier: 'GeneralPurpose'
```

---

## 📈 Deployment Timeline

| Phase | Duration | Activity |
|-------|----------|----------|
| **Validation** | 10-30 sec | Bicep syntax check |
| **Resource Group** | 5-10 sec | Create if not exists |
| **Key Vault** | 30-60 sec | Deploy vault + RBAC |
| **MySQL** | 8-12 min | Longest step (server provisioning) |
| **Storage** | 30-60 sec | If publisher enabled |
| **Container Apps** | 2-3 min | Environment + app deployment |
| **Container Start** | 1-2 min | Pull image + start container |

**Total:** 10-15 minutes

---

## 💰 Cost Breakdown (Dev Environment)

| Resource | Monthly Cost | Notes |
|----------|--------------|-------|
| Container Apps | $15-20 | 0.5 vCPU, 1GB RAM, minimal traffic |
| Container Apps Environment | $5 | Shared platform |
| MySQL (B1ms) | $10-15 | Burstable instance |
| Key Vault | $0.50 | Secrets storage |
| Storage Account | $1-5 | Usage-based |
| **Total** | **$30-45** | Can scale to zero for lower costs |

---

## 🐛 Common Issues

### Deployment Fails
- **Check:** Azure CLI logged in: `az account show`
- **Check:** Subscription has quota for resources
- **Check:** Resource names are globally unique (storage)

### MySQL Connection Fails
- **Check:** Firewall rules in Azure Portal
- **Check:** TLS certificate is trusted
- **Check:** Connection string has correct FQDN

### Container App Not Starting
- **Check:** Logs in Azure Portal
- **Check:** Docker image exists and is public
- **Check:** Environment variables are correct

---

## 📚 Additional Resources

- [Azure Container Apps Documentation](https://learn.microsoft.com/azure/container-apps/)
- [Bicep Language Reference](https://learn.microsoft.com/azure/azure-resource-manager/bicep/)
- [Azure Database for MySQL](https://learn.microsoft.com/azure/mysql/flexible-server/)
- [Azure Key Vault Best Practices](https://learn.microsoft.com/azure/key-vault/general/best-practices)

---

*Last Updated: December 25, 2025*

# Quick Start: Azure Deployment

## TL;DR - One Command to Deploy

```powershell
cd D:\source\SkyCMS\InstallScripts\Azure

# Optional: Validate templates first
.\validate-bicep.ps1

# Deploy
.\deploy-skycms.ps1
```

---

## What You'll Be Asked

```
📋 DEPLOYMENT CONFIGURATION:
   Resource Group Name [rg-skycms-dev]:
   Azure Region [eastus]:
   Base Name (3-10 chars) [skycms]:
   Environment (dev/staging/prod) [dev]:
   Docker Image [toiyabe/sky-editor:latest]:
   MySQL Database Name [skycms]:
   MySQL Admin Password: ********
   Minimum Container Replicas [1]:
   Maximum Container Replicas [3]:
   Deploy Publisher (Blob Storage)? (y/n) [Y]:

✅ CONFIRMATION:
   Proceed with deployment? (y/n) [Y]:
```

---

## What Gets Deployed

### Always (Editor + Database):
- ✅ Azure Container Apps (serverless container platform)
- ✅ MySQL Flexible Server (managed database with TLS)
- ✅ Azure Key Vault (secrets management)
- ✅ Managed Identity (passwordless auth)
- ✅ HTTPS endpoint (automatic, no extra config)

### Optional (Publisher):
- ✅ Blob Storage (static website hosting)
- ✅ Public HTTPS endpoint for static content

---

## Expected Output

```
========================================
 🚀 ACCESS INFORMATION
========================================

📝 EDITOR APPLICATION:
   URL:        https://ca-skycms-editor-dev.kindocean-abc123.eastus.azurecontainerapps.io
   FQDN:       ca-skycms-editor-dev.kindocean-abc123.eastus.azurecontainerapps.io

🗄️  DATABASE:
   Server:     mysql-skycms-xyz456.mysql.database.azure.com
   Database:   skycms
   Username:   skycms_admin

🔐 SECRETS:
   Key Vault:  kv-skycms-abc123

📦 PUBLISHER (Static Website):
   Storage:    stskycmsabc123
   URL:        https://stskycmsabc123.z13.web.core.windows.net

========================================
 📋 NEXT STEPS
========================================

1. Wait 1-2 minutes for Container App to fully start
2. Visit the Editor URL above
3. Complete the SkyCMS setup wizard
4. Upload publisher files to blob storage
```

---

## After Deployment

### 1. Access SkyCMS Editor
- Wait 1-2 minutes for container startup
- Visit the Editor URL from output
- Complete the setup wizard

### 2. Configure Storage (if Publisher deployed)
The setup wizard will ask for Azure Blob Storage credentials:
- **Storage Account Name:** From output (e.g., `stskycmsabc123`)
- **Container Name:** `$web` (for static website)
- **Authentication:** Use Managed Identity (automatic)

### 3. Upload Static Files (Optional)
```powershell
# Using Azure CLI
az storage blob upload-batch `
    --account-name stskycmsabc123 `
    --source ./website `
    --destination '$web' `
    --auth-mode login
```

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Script fails at login | Run `az login` manually first |
| Deployment timeout | Check Azure Portal for deployment status |
| Container not starting | Wait 2-3 minutes, then check logs: `.\helpers.ps1 -Action ViewLogs` |
| Static website 404 | Enable static hosting: `.\helpers.ps1 -Action EnableStaticWebsite` |
| Database connection fails | Check MySQL firewall rules in Portal |
| "Base name must be alphanumeric" | Use only lowercase letters and numbers (no hyphens) |

---

## Cost Estimate

**Development Environment:**
- Container Apps: ~$15-20/month (minimal traffic)
- MySQL (Burstable): ~$10-15/month
- Key Vault: ~$0.50/month
- Blob Storage: ~$1-5/month (usage-based)

**Total: ~$30-40/month**

💡 **Cost Savings:** Set `minReplicas: 0` to scale to zero when idle!

---

## Teardown

```powershell
# Delete everything
.\destroy-skycms.ps1

# Follow prompts to confirm deletion
```

⚠️ **WARNING:** This deletes ALL data permanently!

---

## Key Differences from AWS

| Feature | AWS | Azure |
|---------|-----|-------|
| **Deployment Tool** | CDK (TypeScript) | Bicep (native IaC) |
| **Compute** | ECS + ALB + CloudFront | Container Apps (simpler) |
| **Endpoint** | CloudFront custom URL | Built-in HTTPS |
| **Networking** | VPC required | Optional (simpler) |
| **Secrets** | Secrets Manager | Key Vault + RBAC |
| **Cost** | ~$40-60/month | ~$30-40/month |

**Azure is simpler:** Fewer resources needed for same functionality!

---

## Next Steps

- Review full README.md for customization options
- Explore Bicep templates in `bicep/` folder
- Set up custom domain (optional)
- Configure Application Insights for monitoring
- Add custom environment variables

---

## Support

- 📖 [Full Documentation](README.md)
- 🐛 [Troubleshooting Guide](README.md#troubleshooting)
- 💡 [Bicep Modules](bicep/modules/)

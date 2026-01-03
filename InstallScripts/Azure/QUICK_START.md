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
- ✅ Azure App Service (managed container hosting)
- ✅ App Service Plan (Premium v3 for production-ready performance)
- ✅ Azure SQL Database (managed database with TLS)
- ✅ Azure Key Vault (secrets management with RBAC)
- ✅ Managed Identity (passwordless auth)
- ✅ Deployment Slot (staging for zero-downtime updates)
- ✅ HTTPS endpoint (automatic, no extra config)
- ✅ Health monitoring (`/___healthz` endpoint)

### Optional (Publisher):
- ✅ Blob Storage (static website hosting)
- ✅ Public HTTPS endpoint for static content

---

## Understanding Deployment Slots

**First Deployment (What Happens Now):**
- Production slot gets deployed first with your container image
- Editor URL is immediately accessible at `https://<name>.azurewebsites.net`
- Complete the setup wizard right away - no waiting!
- Staging slot is created but remains empty until your first update

**Future Deployments (Zero-Downtime Updates):**
1. Deploy new container image to staging slot
2. Staging slot starts up and runs health check on `/___healthz`
3. Once health check passes, staging auto-swaps to production
4. Old production version moves to staging (instant rollback available)
5. Users never see downtime or errors

**When to Deploy Updates:**
- After initial setup is complete and SkyCMS is configured
- Push new images to staging slot using Azure CLI or CI/CD pipelines
- Staging validates before going live automatically

---

## Expected Output

```
========================================
 🚀 ACCESS INFORMATION
========================================

📝 EDITOR APPLICATION:
   URL:        https://editor-skycms-dev-abc12345.azurewebsites.net
   FQDN:       editor-skycms-dev-abc12345.azurewebsites.net
   Staging:    https://editor-skycms-dev-abc12345-staging.azurewebsites.net

🗄️  DATABASE:
   Server:     sql-skycms-xyz456.database.windows.net
   Database:   skycms
   Username:   (stored in Key Vault)

🔐 SECRETS:
   Key Vault:  kv-skycms-abc123
   Secrets:    ApplicationDbContextConnection, StorageConnectionString

📦 PUBLISHER (Static Website):
   Storage:    stskycmsabc123
   URL:        https://stskycmsabc123.z13.web.core.windows.net

========================================
 📋 NEXT STEPS
========================================

1. Health check ensures app is ready (30-60 seconds)
2. Visit the Editor URL above
3. Complete the SkyCMS setup wizard
4. Upload publisher files to blob storage
5. Deploy updates to staging slot for zero-downtime
```

---

## After Deployment

### 1. Access SkyCMS Editor
- Health check ensures the app is ready within 30-60 seconds
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
| App not starting | Check health check status and logs: `.\helpers.ps1 -Action ViewLogs` |
| Static website 404 | Enable static hosting: `.\helpers.ps1 -Action EnableStaticWebsite` |
| Database connection fails | Check SQL firewall rules and Key Vault secrets in Portal |
| "Base name must be alphanumeric" | Use only lowercase letters and numbers (no hyphens) |
| Key Vault access denied | Verify Managed Identity has "Key Vault Secrets Officer" role |
| Health check failing | Ensure container exposes port 8080 and `/___healthz` responds |

---

## Cost Estimate

**Development Environment:**
- App Service (P1v3): ~$55-75/month
- Azure SQL (Basic): ~$5-15/month
- Key Vault: ~$0.50/month
- Blob Storage: ~$1-5/month (usage-based)

**Total: ~$60-95/month**

💡 **Cost Savings:** Use B-series App Service Plan for dev (~$13/month) or Standard tier (~$50/month)!

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
| **Compute** | ECS + ALB + CloudFront | App Service (simpler) |
| **Endpoint** | CloudFront custom URL | Built-in HTTPS + auto-certs |
| **Networking** | VPC required | Optional (simpler) |
| **Secrets** | Secrets Manager | Key Vault + RBAC |
| **Database** | RDS MySQL | Azure SQL Database |
| **Deployment** | Manual blue/green | Deployment slots + auto-swap |
| **Health Checks** | Target group health | Built-in + `/___healthz` |
| **Cost** | ~$40-60/month | ~$60-95/month |

**Azure is simpler:** Fewer resources, integrated health checks, zero-downtime deployments!

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

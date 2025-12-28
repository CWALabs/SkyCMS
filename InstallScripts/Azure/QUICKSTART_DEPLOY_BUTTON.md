# Quick Start - Deploy Button

After clicking "Deploy to Azure" and completing the deployment, follow these steps to access and configure your SkyCMS infrastructure.

---

## ⏱️ Deployment Progress

**Typical deployment time:** 10-15 minutes

The Azure Portal will show real-time progress:
- ✅ Container App Environment (2-3 min)
- ✅ MySQL Flexible Server (5-7 min) - *slowest component*
- ✅ Key Vault (1 min)
- ✅ Storage Account (1 min) - *if Publisher enabled*
- ✅ Container App (1-2 min)

---

## 🚀 After Deployment Completes

### 1. Find Your Deployment Outputs

In Azure Portal:
1. Go to **Resource Groups** → Your resource group name
2. Click **Deployments** tab
3. Click the **latest deployment** (should be successful ✓)
4. Scroll down to **Outputs** section

You'll see:
```
editorUrl              https://app-xxxxxxxx.azurecontainerapps.io
editorFqdn             app-xxxxxxxx.azurecontainerapps.io
mysqlServerFqdn        mysql-xxxxxxxx.mysql.database.azure.com
mysqlAdminUsername     sqladmin
keyVaultName           kv-xxxxxxxx
storageAccountName     stxxxxxxxx (if Publisher enabled)
staticWebsiteUrl       https://stxxxxxxxx.z13.web.core.windows.net (if enabled)
```

---

## 🌐 Access Your SkyCMS Editor

### Step 1: Open the Editor URL
Copy the **editorUrl** from deployment outputs above:
```
https://app-xxxxxxxx.azurecontainerapps.io
```

### Step 2: Wait for Container to Start
- First access may take 30-60 seconds
- Container App is starting up for the first time
- You may see a blank page initially

### Step 3: Complete SkyCMS Setup Wizard
Once loaded, you'll see the SkyCMS setup wizard:
1. **Site Configuration**
   - Site name: Choose a name for your site
   - Administrator email: Your email address

2. **Database Configuration**
   - Server: `mysql-xxxxxxxx.mysql.database.azure.com`
   - Username: `sqladmin`
   - Password: The password you entered during deployment
   - Database: `skycms` (default)

3. **Completion**
   - Wait for initialization
   - Login with admin credentials

---

## 🔐 Accessing Secrets in Key Vault

Your database password and other secrets are stored securely in Azure Key Vault.

### To Retrieve Secrets:
1. Go to **Key Vault** in your resource group
2. Click **Secrets** in the left menu
3. Click the secret you need (e.g., `mysqlAdminPassword`)
4. Click the current version
5. Click **Show Secret Value** to reveal

---

## 📦 Publisher Setup (If Enabled)

If you enabled the Publisher (static website):

### Step 1: Upload Your Publisher Files
```powershell
# In PowerShell, navigate to your site build directory
cd path/to/your/published/site

# Upload files to blob storage
az storage blob upload-batch `
  --account-name "stxxxxxxxx" `
  --source . `
  --destination '$web' `
  --auth-mode login `
  --overwrite
```

### Step 2: Access Your Static Website
Use the **staticWebsiteUrl** from outputs:
```
https://stxxxxxxxx.z13.web.core.windows.net
```

---

## 📊 Monitor Your Deployment

### Container App Logs
```powershell
az containerapp logs show `
  --name "ca-skycms" `
  --resource-group "YOUR-RESOURCE-GROUP" `
  --follow
```

### Database Connection Test
```bash
mysql -h mysql-xxxxxxxx.mysql.database.azure.com `
      -u sqladmin `
      -p
```

---

## 💰 Cost Management

Your estimated monthly costs:
| Component | Cost |
|-----------|------|
| Container Apps | ~$15-30 |
| MySQL Flexible Server | ~$10-20 |
| Key Vault | ~$0.50 |
| Storage (Publisher) | ~$1-5 |
| **Total** | **~$30-55** |

### Cost Optimization Tips
- ✅ Dev environment is already optimized (1 replica, burstable MySQL)
- 💡 For prod, increase min replicas based on traffic
- 💡 Use auto-scaling rules for variable workloads
- 💡 Enable server-side encryption for compliance

---

## 🔧 Troubleshooting

### Container App Not Responding
```powershell
# Check deployment status
az containerapp show `
  --name "ca-skycms" `
  --resource-group "YOUR-RESOURCE-GROUP"

# Restart the app
az containerapp update `
  --name "ca-skycms" `
  --resource-group "YOUR-RESOURCE-GROUP" `
  --set-env-vars RESTART_TIMESTAMP=$(Get-Date -Format 'u')
```

### Can't Connect to MySQL
1. Verify password is correct
2. Check firewall rules: MySQL → Networking → Firewall Rules
3. Ensure "Allow public access from any Azure service" is enabled
4. Test connectivity:
   ```bash
   mysql -h mysql-xxxxxxxx.mysql.database.azure.com -u sqladmin -p
   ```

### Container App Stuck in Provisioning
- Deployments usually take 10-15 minutes
- Check deployment logs in Azure Portal
- Wait another 5 minutes (MySQL provisioning is slow)
- If still failing, check error messages in deployment details

---

## 📚 Advanced Management

### Using Helper Scripts

For advanced management tasks, use the helper script:

```powershell
# View logs
.\helpers.ps1 -Action ViewLogs

# Restart container
.\helpers.ps1 -Action RestartContainerApp

# Scale replicas
.\helpers.ps1 -Action ScaleContainerApp

# Get MySQL connection info
.\helpers.ps1 -Action GetConnectionString
```

### Managing with Azure CLI

```powershell
# Scale container app
az containerapp update `
  --name "ca-skycms" `
  --resource-group "YOUR-RESOURCE-GROUP" `
  --min-replicas 2 `
  --max-replicas 5

# View resource group resources
az resource list --resource-group "YOUR-RESOURCE-GROUP"

# Check costs
az costmanagement query --timeframe "MonthToDate" ...
```

---

## 🧹 Cleanup

When you're done testing or want to delete everything:

```powershell
# Delete the entire resource group
az group delete --name "YOUR-RESOURCE-GROUP" --yes --no-wait

# Alternatively, use the destroy script
.\destroy-skycms.ps1 -ResourceGroupName "YOUR-RESOURCE-GROUP" -Force
```

⚠️ **Warning:** This permanently deletes all resources and data!

---

## 📞 Getting Help

### Common Issues

| Issue | Solution |
|-------|----------|
| 503 Service Unavailable | Wait 2-3 minutes for startup |
| Connection Timeout to MySQL | Check firewall rules in MySQL settings |
| Disk space issues | Scale up Container App CPU/Memory |
| High costs | Reduce replicas or use burstable MySQL tier |

### Useful Links
- [Azure Container Apps Docs](https://learn.microsoft.com/azure/container-apps/)
- [MySQL Flexible Server Docs](https://learn.microsoft.com/azure/mysql/flexible-server/)
- [Azure Key Vault Docs](https://learn.microsoft.com/azure/key-vault/)
- [Azure Storage Documentation](https://learn.microsoft.com/azure/storage/)

### Need More Help?
1. Check deployment logs in Azure Portal
2. Review error messages in Container App logs
3. Verify all parameters match expectations
4. Test connectivity with Azure CLI commands

---

## ✅ Next Steps

1. ✅ Access your SkyCMS editor
2. ✅ Complete SkyCMS setup wizard
3. ✅ Configure your content (if Publisher enabled)
4. ✅ Monitor the application
5. ✅ Set up backups (optional)
6. ✅ Configure custom domain (optional)
7. ✅ Set up monitoring and alerts (optional)

---

**Congratulations! Your SkyCMS infrastructure is now running on Azure!** 🎉

For advanced configuration and custom deployments, refer to the main [README.md](./README.md) and PowerShell deployment scripts.

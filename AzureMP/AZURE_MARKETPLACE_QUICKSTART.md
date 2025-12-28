# Azure Marketplace Quick Start Guide

**Welcome to SkyCMS!** This guide will help you get started after deploying from the Azure Marketplace.

---

## What Just Happened?

Your Azure deployment created:
- ✅ **Azure Container Apps** - Running the SkyCMS Editor
- ✅ **Azure Database for MySQL** - Storing your content and users
- ✅ **Azure Key Vault** - Securing database credentials
- ✅ **Azure Blob Storage** (optional) - Static website hosting
- ✅ **Managed Identity** - Passwordless authentication between services

**Total deployment time**: 10-15 minutes

---

## Step 1: Access Your SkyCMS Editor (2 minutes)

1. **Find your Editor URL** in the deployment outputs:
   ```
   https://ca-skycms-editor-[your-name].azurecontainerapps.io
   ```

2. **Wait 1-2 minutes** for the container to fully start

3. **Open the URL** in your browser

4. You should see the **SkyCMS Setup Wizard**

---

## Step 2: Run the Setup Wizard (5 minutes)

The wizard will guide you through:

### 📦 Storage Configuration
- Choose **Azure Blob Storage** (if deployed)
- Connection string is pre-filled from your deployment
- Set the public CDN URL for your static website

### 👤 Admin Account
- Create your administrator account
- Use a strong password (12+ characters)
- This account has full system access

### 🌐 Publisher Settings
- Set your website name and public URL
- Choose a default design template
- Configure whether authentication is required

### 📧 Email (Optional)
- Configure SendGrid, Azure Communication Services, or SMTP
- Test email delivery
- Skip if not needed initially

### 🚀 CDN (Optional)
- Configure Azure Front Door, Cloudflare, or other CDN
- Set up cache purging
- Skip if not needed initially

### ✅ Review & Complete
- Confirm all settings
- Click **Complete Setup**
- **Restart the app** when prompted

---

## Step 3: Sign In & Create Content (5 minutes)

1. **After restart**, visit your Editor URL again
2. **Sign in** with the admin account you created
3. **Create your first page**:
   - Click **Pages** → **New Page**
   - Choose **Live Editor** (WYSIWYG) or **Designer** (drag-and-drop)
   - Add your content
   - Click **Save** then **Publish**

4. **View your published page**:
   - If using Blob Storage: `https://[storage-account].z13.web.core.windows.net`
   - Content is live globally!

---

## Common Post-Deployment Tasks

### Enable Static Website on Blob Storage

If you deployed Blob Storage but it's not enabled:

```powershell
# Using Azure CLI
az storage blob service-properties update \
    --account-name <your-storage-account> \
    --static-website \
    --index-document index.html \
    --404-document 404.html
```

Or use the Azure Portal:
1. Navigate to your Storage Account
2. Go to **Data management** → **Static website**
3. Click **Enabled**
4. Set **Index document**: `index.html`
5. Set **Error document**: `404.html`
6. Click **Save**

### Configure Custom Domain

1. In Azure Portal, go to your Container App
2. Navigate to **Settings** → **Custom domains**
3. Click **Add custom domain**
4. Follow the wizard to add your domain and SSL certificate

### View Application Logs

```powershell
# Using Azure CLI
az containerapp logs show \
    --name <your-container-app-name> \
    --resource-group <your-resource-group> \
    --follow
```

Or use Azure Portal:
1. Navigate to your Container App
2. Go to **Monitoring** → **Log stream**
3. View real-time logs

### Scale Your Application

```powershell
# Scale to handle more traffic
az containerapp update \
    --name <your-container-app-name> \
    --resource-group <your-resource-group> \
    --min-replicas 2 \
    --max-replicas 10
```

Or use Azure Portal:
1. Navigate to your Container App
2. Go to **Application** → **Scale**
3. Adjust min/max replicas
4. Click **Save**

### Enable Application Insights

1. Create an Application Insights resource
2. Add connection string to Container App environment variables:
   - Name: `APPLICATIONINSIGHTS_CONNECTION_STRING`
   - Value: Your Application Insights connection string
3. Restart the Container App

---

## Cost Optimization Tips

### Development/Testing
- **Scale to zero**: Set `minReplicas: 0` to avoid idle costs
- **Use Burstable MySQL**: B1ms tier is sufficient for testing
- **Disable geo-redundancy**: Standard LRS storage for non-production

### Production
- **Enable autoscaling**: Only pay for what you use
- **Use Azure Front Door**: Better caching = lower origin costs
- **Enable MySQL backups**: 7-day retention included
- **Set up cost alerts**: Get notified before overspending

**Estimated monthly costs**:
- Development: $30-45/month
- Production (medium): $110-220/month

---

## Troubleshooting

### Container App Won't Start

**Check logs**:
```powershell
az containerapp logs show --name <app-name> --resource-group <rg-name> --follow
```

**Common causes**:
- Database connection string incorrect
- MySQL firewall blocking connections
- Key Vault permissions not set

**Solution**: Verify connection strings and firewall rules in Azure Portal

### Setup Wizard Returns 400 Error

**Cause**: Antiforgery token validation failed (common with proxies)

**Solution**: This should be fixed in latest version. If you see this:
1. Check container app environment variables
2. Ensure `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`
3. Restart the container app

### Database Connection Fails

**Check**:
1. MySQL server is running (check Azure Portal)
2. Firewall allows Azure services (or your IP)
3. TLS connection is enabled in connection string
4. Managed Identity has proper RBAC role

**Test connection**:
```bash
mysql -h <server>.mysql.database.azure.com -u <username> -p
```

### Static Website Shows 404

**Check**:
1. Static website is enabled on storage account
2. Files were published to `$web` container
3. Index document is set to `index.html`
4. Blob public access is configured correctly

---

## Next Steps

### 📚 Learn More
- [Full Documentation](https://docs-sky-cms.com)
- [Video Tutorials](https://www.youtube.com/@Sky-cms) *(coming soon)*
- [Azure Deployment Guide](https://github.com/CWALabs/SkyCMS/blob/main/InstallScripts/Azure/README.md)

### 🎨 Customize Your Site
- [Layouts Guide](https://cwalabs.github.io/SkyCMS/Layouts/Readme.html)
- [Templates Guide](https://cwalabs.github.io/SkyCMS/Templates/Readme.html)
- [Widgets Overview](https://cwalabs.github.io/SkyCMS/Widgets/README.html)

### 🚀 Advanced Features
- [CDN Integration](https://cwalabs.github.io/SkyCMS/CloudflareEdgeHosting.html)
- [Multi-Tenant Setup](https://cwalabs.github.io/SkyCMS/Configuration/MultiTenant.html)
- [Performance Optimization](https://cwalabs.github.io/SkyCMS/ADVANCED-OPTIMIZATIONS.html)

### 💬 Get Help
- [GitHub Discussions](https://github.com/CWALabs/SkyCMS/discussions) - Ask questions
- [GitHub Issues](https://github.com/CWALabs/SkyCMS/issues) - Report bugs
- [Support Options](https://github.com/CWALabs/SkyCMS/blob/main/SUPPORT.md) - Commercial support

---

## Security Checklist

Before going to production:

- [ ] Change admin password to a strong unique password
- [ ] Enable multi-factor authentication (configure OAuth)
- [ ] Review user roles and permissions
- [ ] Enable MySQL geo-redundant backups
- [ ] Configure Azure Front Door WAF (Web Application Firewall)
- [ ] Set up Azure Monitor alerts
- [ ] Review Key Vault access policies
- [ ] Enable storage account encryption
- [ ] Configure custom domain with SSL
- [ ] Set up cost alerts and budgets

---

**Need Help?** See [SUPPORT.md](./SUPPORT.md) for all support options.

**Security Concern?** See [SECURITY.md](./SECURITY.md) for reporting vulnerabilities.

**Questions about data?** See [PRIVACY.md](./PRIVACY.md) for privacy information.

---

**Congratulations!** 🎉 You're now ready to build amazing websites with SkyCMS on Azure.

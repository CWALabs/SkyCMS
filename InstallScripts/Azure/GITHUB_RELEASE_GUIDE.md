# How to Create a GitHub Release for Azure Deployment

## Step 1: Create the Package

Run the packaging script:
```powershell
cd InstallScripts\Azure
.\create-release-package.ps1
```

This creates `skycms-azure-deployment-v1.0.0.zip` (update version in script as needed).

## Step 2: Create GitHub Release

### Via GitHub Web Interface (Easiest):

1. **Go to your repository** on GitHub.com

2. **Click "Releases"** (on the right sidebar)

3. **Click "Draft a new release"**

4. **Fill in the release form**:
   - **Tag version**: `azure-v1.0.0` (create new tag)
   - **Release title**: `SkyCMS Azure Deployment v1.0.0`
   - **Description**: 
     ```markdown
     ## SkyCMS Azure Deployment Package
     
     Deploy SkyCMS to Azure using Bicep Infrastructure as Code with a single PowerShell script.
     
     ### What's Included
     - Interactive deployment script (`deploy-skycms.ps1`)
     - Cleanup script (`destroy-skycms.ps1`)
     - Validation script (`validate-bicep.ps1`)
     - Helper utilities script (`helpers.ps1`)
     - Bicep infrastructure templates
     - Quick start guide
     - Installation guide
     
     ### Prerequisites
     - Azure CLI configured
     - PowerShell 5.1 or later
     - Docker image available (default: toiyabe/sky-editor:latest)
     - Active Azure subscription
     
     ### Quick Start
     1. Download and extract `skycms-azure-deployment-v1.0.0.zip`
     2. Run `.\deploy-skycms.ps1`
     3. Follow interactive prompts
     
     ### What Gets Deployed
     - Azure App Service (Premium v3) with deployment slots
     - Azure SQL Database with TLS encryption
     - Azure Key Vault for secrets management
     - Managed Identity for passwordless authentication
     - Health monitoring on `/___healthz` endpoint
     - Blob Storage for static website (optional)
     - Azure Communication Services for email (optional)
     - Application Insights for monitoring (optional)
     
     ### Key Features
     - ✅ Zero-downtime deployments via staging slot with auto-swap
     - ✅ Health checks ensure app readiness before traffic routing
     - ✅ Key Vault references for secure secret management
     - ✅ Production slot deployed first - immediate access for setup
     - ✅ Built-in HTTPS with automatic certificate management
     - ✅ Comprehensive logging and diagnostics
     
     ### Estimated Cost
     - **Development:** ~$60-95/month (P1v3 App Service Plan)
     - **Production:** ~$100-150/month (P2v3 with geo-redundancy)
     
     See INSTALL.md inside the package for full details.
     ```

5. **Attach the zip file**:
   - Drag and drop `skycms-azure-deployment-v1.0.0.zip` into the release assets area
   - Or click "Attach binaries" and select the file

6. **Publish release**
   - For production: Click "Publish release"
   - For testing: Check "Set as a pre-release" first

## Step 3: Users Download

Users can now:
1. Go to your repository's Releases page
2. Download `skycms-azure-deployment-v1.0.0.zip`
3. Extract it anywhere on their machine
4. Run `.\deploy-skycms.ps1`

**No need to clone the entire repository!**

## Alternative: Using GitHub CLI

If you have GitHub CLI installed:

```powershell
# Create the package first
.\create-release-package.ps1

# Create release and upload
gh release create azure-v1.0.0 `
  skycms-azure-deployment-v1.0.0.zip `
  --title "SkyCMS Azure Deployment v1.0.0" `
  --notes "Standalone Azure deployment package with Bicep IaC. See INSTALL.md for instructions. Includes zero-downtime deployment slots, Key Vault integration, and health monitoring."
```

## Updating for New Versions

1. Update version in `create-release-package.ps1` (line 4)
2. Run `.\create-release-package.ps1`
3. Create new GitHub release with new tag (e.g., `azure-v1.1.0`)
4. Upload new zip file
5. Update release notes with what's new

## Version History

| Version | Date | Changes |
|---------|------|---------|
| v1.0.0 | Jan 2026 | Initial release with App Service, Key Vault, deployment slots |

## Best Practices

### Versioning
- Use semantic versioning: `major.minor.patch`
- Prefix tags with `azure-` to distinguish from AWS releases
- Example: `azure-v1.0.0`, `azure-v1.1.0`, `azure-v2.0.0`

### Release Notes
- Clearly state what's new or changed
- Include breaking changes prominently
- Update cost estimates if infrastructure changes
- Link to migration guides if needed

### Testing Before Release
1. Extract the ZIP to a fresh directory
2. Test deployment on a clean subscription
3. Verify all scripts work without errors
4. Test teardown script
5. Verify documentation accuracy

### File Checklist
Ensure the package includes:
- ✅ `deploy-skycms.ps1` - Main deployment script
- ✅ `destroy-skycms.ps1` - Cleanup script
- ✅ `validate-bicep.ps1` - Template validation
- ✅ `helpers.ps1` - Management utilities
- ✅ `bicep/main.bicep` - Main orchestration template
- ✅ `bicep/modules/*.bicep` - All module templates
- ✅ `README.md` - Full documentation
- ✅ `QUICK_START.md` - Quick reference
- ✅ `INSTALL.md` - Installation guide (auto-generated)

## Communication

### Announcement Templates

#### Slack/Discord:
```
🚀 SkyCMS Azure Deployment v1.0.0 Released!

Deploy SkyCMS to Azure in minutes with our new standalone package:
• Zero-downtime deployments via staging slots
• Key Vault integration for secure secrets
• Health monitoring built-in
• Production-ready in ~10 minutes

Download: [Release URL]
```

#### Twitter/X:
```
🎉 SkyCMS Azure deployment package is here! Deploy production-ready CMS to Azure with Bicep IaC in 10 minutes. Includes deployment slots, Key Vault, health monitoring, and more. #Azure #DevOps #CMS
```

### Documentation Updates

After releasing:
1. Update main README.md to link to latest release
2. Update installation documentation
3. Add release to changelog
4. Update any version-specific docs

## Troubleshooting Package Creation

| Issue | Solution |
|-------|----------|
| Script not found errors | Verify all scripts exist in InstallScripts/Azure |
| Bicep modules missing | Check bicep/modules directory exists |
| ZIP too large | Ensure node_modules, bin, obj folders excluded |
| Permission errors | Run PowerShell as Administrator |

## Support After Release

Monitor for:
- Download statistics
- GitHub Issues related to Azure deployment
- User feedback on deployment time
- Cost estimates accuracy
- Documentation gaps

Create FAQ document based on common questions.

---

**Next Steps:**
1. Run `.\create-release-package.ps1`
2. Test the generated ZIP file
3. Create GitHub release
4. Announce to community
5. Monitor for issues

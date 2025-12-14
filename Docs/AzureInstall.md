{% include nav.html %}


# Installing SkyCMS on Azure

The following describes how to install SkyCMS on Microsoft Azure.

## Quick Installation using Deploy Button

The easiest way to install SkyCMS is using the Deploy button located in the main README.md file.

### Prerequisites

- An Azure subscription ([Get one free](https://azure.microsoft.com/en-us/pricing/purchase-options/azure-account/))

### Installation Steps

1. **Navigate to the Repository**
    - Go to the SkyCMS repository on GitHub.
    - Locate the main README.md file.

2. **Click the Deploy Button**
    - Find the "Deploy to Azure" button in the README.md.
    - Click the button to start the deployment process.

3. **Configure Deployment**
    - You'll be redirected to the Azure portal.
    - Sign in with your Azure credentials if prompted.
    - Select your Azure subscription.
    - Choose or create a resource group.
    - Add an administrator email address. This can be yours.
    - Add an email provider information (optional for dev/test)
    - Choose basic or premium app plan.
    - Choose locally or geographically redundant storage.
    - Click "Review + create."

![Azure deploy dialog](./AzureDiaglog.png)

1. **Deploy**
    - Review your configuration settings
    - Click "Deploy" to start the installation
    - Wait for the deployment to complete

2. **Access Your Installation**
    - Once deployment is finished, open the resource groups where you installed Sky.
    - Find the editor web app. The name prefix will start with "ed".
    - Browse to `https://<your-editor>.azurewebsites.net/Setup` and run the **single-tenant setup wizard** (enabled via `CosmosAllowSetup=true`). Multi-tenant deployments should leave this flag false and use DynamicConfig instead.
    - Wizard steps: Storage → Admin account → Publisher URL/title/layout → (optional) Email provider → (optional) CDN → Review & Complete. It validates storage and database connectivity and creates the first Administrator user.
    - After finishing the wizard, restart the app (App Service restarts itself after a configuration change) and sign in with the admin email/password you specified.
    - Create your first page and choose a starter design.
    - At your website's home page, select the "Menu" button, then "Public Website".

### Next Steps

After successful deployment:
- Configure your CMS settings
- Set up user accounts
- Begin creating content

### Related Documentation

- **[Storage Configuration](./StorageConfig.md)** - Configure Azure Blob, AWS S3, or Cloudflare R2 storage
- **[Database Configuration](./DatabaseConfig.md)** - Database provider setup (Cosmos DB, SQL Server, MySQL)
- **[Cloudflare Edge Hosting](./CloudflareEdgeHosting.md)** - Deploy origin-less static sites with Cloudflare R2
- **[Quick Start Guide](./QuickStart.md)** - Get started quickly
- **[Main Documentation Hub](./README.md)** - Browse all SkyCMS documentation

### Clean Up

If SkyCMS was created in a new Resource Group, simply delete the resource group to remove all Sky and all its resources.  Otherwise, using the Azure portal, delete the Sky resources there.

For additional configuration options and troubleshooting, refer to the documentation in the `/docs` folder.

{% include nav.html %}

# 5-Minute Quick Start

Use the new setup wizard for **single-tenant** installs. It walks you through storage, admin user, publisher URL, optional email, and optional CDN. Set `CosmosAllowSetup=true` and make sure `ConnectionStrings:ApplicationDbContextConnection` points to a reachable database before you begin.

## Local trial (Docker)

```bash
# No cloud account required; enables the wizard for single-tenant setup
docker run -d -p 8080:8080 \
  -e CosmosAllowSetup=true \
  -e ConnectionStrings__ApplicationDbContextConnection="Data Source=/data/skycms.db" \
  toiyabe/sky-editor:latest
```

1) Open `http://localhost:8080/Setup`
2) Complete the wizard: Storage → Admin → Publisher → (optional) Email → (optional) CDN → Review → Complete
3) Restart the container when prompted, then sign in with the admin account you just created.

## Azure (one-click deploy)

1) Use the **Deploy to Azure** button in the main README to provision resources.
2) Browse to your Editor site, e.g., `https://<editor-app>.azurewebsites.net/Setup`.
3) Run the same single-tenant setup wizard steps above. Azure deploys with the wizard enabled; if you plan to run multi-tenant, leave `CosmosAllowSetup=false` and follow the DynamicConfig docs instead.

## Wizard checklist

- Storage: Azure Blob / S3 / R2 connection string and public/CDN URL (validated).
- Admin: email + strong password for the first Administrator account.
- Publisher: public URL, site title, starter design, optional auth requirement.
- Email (optional): SendGrid, Azure Communication Services, or SMTP; test send available.
- CDN (optional): Azure CDN/Front Door, Cloudflare, or Sucuri.
- Review & complete: confirm, finish, restart app to apply. Wizard disables itself after completion.

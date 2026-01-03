---
title: Azure Communication Services Email
description: Configure Azure Communication Services for transactional email delivery
keywords: Azure-Communication-Services, email, Azure, transactional-emails, configuration
audience: [developers, administrators]
version: 2.0
last_updated: "2026-01-03"
stage: stable
read_time: 6
---

# Azure Communication Services Email with SkyCMS

Azure Communication Services is a cloud-based communication platform managed by Microsoft Azure. Its email service integrates seamlessly with other Azure services and provides reliable transactional email delivery.

## When to use this
- You run on Azure and want managed email without third-party SMTP.
- You need straightforward Azure-native auth (connection string) and sender domain options.

## Why this matters
- Email is required for password resets and notifications; correct domain verification is mandatory.
- Azure-managed domain offers quick start; custom domain improves branding/deliverability.

## Key takeaways
- Set `ConnectionStrings__AzureCommunicationConnection` + `AdminEmail`; verify sender domain/email.
- Managed domain is fastest; custom domain requires DNS records (SPF/DKIM/DMARC).
- Store connection string in Key Vault/secret store; rotate keys.

## Prerequisites
- Azure subscription; Communication Services resource; managed or custom domain verified.

## Quick path
1. Create Communication Services resource; get connection string.
2. Choose managed domain or verify custom domain; obtain sender email.
3. Set env/appsettings (or wizard) with connection string + sender; send test email.

## Values you need

- **Azure Communication Services Connection String**: Contains endpoint and access key
- **Admin Email**: Sender email address (must be verified in Azure)

## Create an Azure Communication Services Resource

### Prerequisites

- Active Azure subscription
- Azure CLI or Azure Portal access

### Using Azure Portal

1. Go to [Azure Portal](https://portal.azure.com)
2. Click **+ Create a resource**
3. Search for **Communication Services**
4. Click **Create**
5. Fill in:
   - **Subscription**: Choose your subscription
   - **Resource Group**: Create new or select existing
   - **Name**: Descriptive name (e.g., `skycms-email`)
   - **Data Location**: Choose region closest to your users
6. Click **Review + Create** → **Create**
7. Wait for deployment to complete

### Using Azure CLI

```powershell
# Create resource group
az group create --name skycms-rg --location eastus

# Create Communication Services resource
az communication create `
  --name skycms-email `
  --resource-group skycms-rg
```

## Set Up Email Domain (Sender Verification)

Email requires a verified sender domain or email address.

### Option 1: Use Azure-Managed Domain (Easiest)

1. In Azure Portal, open your Communication Services resource
2. Go **Emails** → **Email domains**
3. Click **Connect email domain**
4. Select **Add an Azure-managed domain**
5. Enter a domain prefix (e.g., `noreply`)
6. Azure auto-generates a domain like `noreply.azurecomm.net`
7. Click **Create**
8. Once provisioned, note the **From email address** (e.g., `DoNotReply@noreply.azurecomm.net`)

### Option 2: Connect Your Own Domain (Production)

1. Go **Emails** → **Email domains**
2. Click **Connect email domain**
3. Select **Add your own domain**
4. Enter your domain (e.g., `yourdomain.com`)
5. Follow DNS verification steps
6. After verification, create a **sender username** (e.g., `noreply`)
7. Full sender address becomes `noreply@yourdomain.com`

This requires adding DNS records provided by Azure to your domain registrar.

## Get Your Connection String

1. In Azure Portal, open your Communication Services resource
2. Go **Settings** → **Keys**
3. Copy the **Connection string** (starts with `endpoint=https://...`)
4. Save it securely

## Configure in SkyCMS

### Method 1: Environment Variables (recommended)

```powershell
# Set before starting SkyCMS
$env:ConnectionStrings__AzureCommunicationConnection = "endpoint=https://skycms-email.communication.azure.com/;accesskey=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx=="
$env:AdminEmail = "DoNotReply@noreply.azurecomm.net"
```

### Method 2: appsettings.json (not recommended for production)

```json
{
  "ConnectionStrings": {
    "AzureCommunicationConnection": "endpoint=https://skycms-email.communication.azure.com/;accesskey=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx=="
  },
  "AdminEmail": "DoNotReply@noreply.azurecomm.net"
}
```

### Method 3: Setup Wizard

1. Deploy SkyCMS with `CosmosAllowSetup=true`
2. Navigate to `https://yourdomain.com/___setup`
3. Complete setup through **Step 4: Email Configuration**
4. Select **Azure Communication Services**
5. Paste your **Connection String**
6. Enter your **Sender Email** (the verified email from your domain)
7. Click **Test Email** to verify
8. Complete the wizard

## Test email sending

### From the Setup Wizard

During setup, after entering credentials:

1. Scroll to **Test Recipient Email**
2. Enter a test email address (any email you can check)
3. Click **Test Email Settings**
4. Check the test inbox (may take 1-2 minutes)
5. If successful, you'll see the test email; if failed, error details appear

### From Azure Portal

1. Open your Communication Services resource
2. Go **Troubleshooting** → **Send test email**
3. Enter your test email address
4. Click **Send**
5. Check your inbox

### Manually with Azure CLI

```powershell
# Install Azure Communication Services SDK (one time)
dotnet add package Azure.Communication.Email

# Example C# code
using Azure;
using Azure.Communication.Email;

var connectionString = "endpoint=https://skycms-email.communication.azure.com/;accesskey=xxxxx";
var client = new EmailClient(new Uri("https://skycms-email.communication.azure.com/"), new AzureKeyCredential("xxxxx"));

var emailMessage = new EmailMessage(
    senderAddress: "DoNotReply@noreply.azurecomm.net",
    recipientAddress: "test@example.com",
    content: new EmailContent("Test Subject") {
        PlainText = "This is a test email from SkyCMS.",
        Html = "<p>This is a test email from SkyCMS.</p>"
    });

EmailSendOperation emailSendOperation = await client.SendAsync(
    WaitUntil.Completed,
    emailMessage);
```

## Azure Communication Services features and considerations

### Advantages

- **Azure integration**: Works seamlessly with other Azure services
- **Managed domain**: Azure-managed domain available for quick setup
- **Reliability**: Built on Microsoft's infrastructure
- **Pricing**: Reasonable per-email costs (typically $0.0001 per email)
- **Logging and diagnostics**: Integration with Azure Monitor

### Limitations

- **No free tier**: All emails have a cost (starts at ~$1/month for 1000 emails)
- **Domain verification required**: Must verify sender email/domain
- **Limited advanced features**: Fewer features than SendGrid (no open tracking by default)
- **Azure-specific**: Requires Azure subscription and knowledge of Azure

### Pricing

- **Pay-as-you-go**: Charged per email sent (typically $0.0001-$0.0003 per email)
- **No setup fees**
- **Hourly billing**: Charges apply as emails are sent

## DNS Configuration (Custom Domain)

If using your own domain, add these DNS records (provided by Azure):

- **SPF record**: For sender verification
- **DKIM record**: For domain authentication
- **DMARC record** (optional): For email authentication policy

After adding records, allow 24-48 hours for DNS propagation.

## Troubleshooting

### "Invalid connection string" error

- Verify the connection string is copied completely (includes both `endpoint=` and `accesskey=`)
- Check there are no extra spaces or line breaks

### "Sender email not verified" error

- Verify the sender email matches a verified domain in **Emails** → **Email domains**
- Check the email format is correct (e.g., `DoNotReply@noreply.azurecomm.net`)

### "Unauthorized" or "403" response

- Verify the access key from the connection string is correct
- Check resource is in the same Azure subscription

### Email not received

- Check recipient email is correct and accepts external emails
- Verify sender email is verified in Azure
- Check Azure Monitor logs for send failures

### Slow email delivery

- Azure usually delivers within seconds, but can take a few minutes during high load
- Check Azure resource limits haven't been exceeded

## Best practices

- **Use your own domain** in production for better branding and deliverability
- **Configure SPF/DKIM** for your domain for authentication
- **Monitor costs** via Azure Cost Management
- **Use managed identity** instead of connection strings if running in Azure
- **Never commit connection strings** to source control; use environment variables or Key Vault
- **Monitor delivery failures** in Azure Monitor and Application Insights
- **Set up alerts** for high email volumes or delivery failures

## See Also

- [Azure Communication Services Email Documentation](https://learn.microsoft.com/en-us/azure/communication-services/concepts/email/email-overview)
- [Azure Communication Services Email SDK](https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/email/send-email-advanced)
- [Email Provider Overview](./Email-Overview.md)
- [Email Configuration Reference](./Email-Configuration-Reference.md)
- [Main Configuration Guide](./README.md)

---
title: SendGrid Email Configuration
description: Configure SendGrid email provider with API keys for transactional emails
keywords: SendGrid, email, Twilio, transactional-emails, API, configuration
audience: [developers, administrators]
---

# SendGrid Email with SkyCMS

SendGrid (owned by Twilio) is a cloud-based email delivery platform with reliable sending, detailed analytics, and advanced features like templates and delivery tracking.

## Values you need

- **SendGrid API Key**: A restricted API key with "Mail Send" permission
- **Admin Email**: Your organization's sender email address (verified in SendGrid)

## Create a SendGrid Account

1. Go to [SendGrid.com](https://sendgrid.com)
2. Click **Sign Up** and create a free or paid account
3. Complete email verification
4. Confirm your identity (may require SMS or credit card)

## Create a Sender Email Address (Verified)

1. In SendGrid dashboard, go **Settings** → **Sender Authentication**
2. Choose **Single Sender Verification** (easiest for development) or **Domain Authentication** (recommended for production)

### Single Sender Verification (Development)

1. Click **Create New Sender**
2. Fill in:
   - **From Email Address**: The email that will send messages (e.g., `noreply@yourdomain.com`)
   - **From Name**: Display name (e.g., "My CMS")
   - **Reply-to Email**: Where replies go
   - **Company Address, City, State, ZIP**: Your organization details
3. Click **Create**
4. Check your email; click the verification link in SendGrid's confirmation email
5. Once verified, the status shows **Verified**

### Domain Authentication (Production)

1. Click **Authenticate your domain**
2. Enter your domain (e.g., `yourdomain.com`)
3. SendGrid provides DNS records to add to your domain registrar
4. After DNS propagates (typically 24 hours), SendGrid confirms
5. Greatly improves deliverability for high-volume senders

## Create an API Key (least privilege)

1. In SendGrid dashboard, go **Settings** → **API Keys**
2. Click **Create API Key**
3. Fill in:
   - **API Key Name**: Something descriptive (e.g., "SkyCMS Production")
   - **API Key Permissions**: Select **Restricted Access**
4. Under **Mail Send**, toggle **ON** for:
   - ✅ **Mail Send**
   - ✅ **Full Access** (or specific scopes if needed)
5. Leave other permissions off (least privilege)
6. Click **Create & View**
7. **Copy and save** the API key immediately (visible only once)

## Get your SendGrid information

- **API Key**: From the API Keys page (as created above)
- **From Email**: Your verified sender email from the Sender Authentication page
- **From Name**: Display name you set for the sender

## Configure in SkyCMS

### Method 1: Environment Variables (recommended)

```powershell
# Set before starting SkyCMS
$env:CosmosSendGridApiKey = "SG.xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
$env:AdminEmail = "noreply@yourdomain.com"
```

### Method 2: appsettings.json (not recommended for production)

```json
{
  "CosmosSendGridApiKey": "SG.xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "AdminEmail": "noreply@yourdomain.com"
}
```

### Method 3: Setup Wizard

1. Deploy SkyCMS with `CosmosAllowSetup=true`
2. Navigate to `https://yourdomain.com/___setup`
3. Complete setup through **Step 4: Email Configuration**
4. Select **SendGrid**
5. Paste your **API Key**
6. Enter your **Sender Email** (e.g., `noreply@yourdomain.com`)
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

### Manually from PowerShell

```powershell
# Use SendGrid's API directly
$headers = @{
    "Authorization" = "Bearer SG.xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
    "Content-Type" = "application/json"
}

$body = @{
    personalizations = @(
        @{
            to = @(@{ email = "test@example.com" })
        }
    )
    from = @{ email = "noreply@yourdomain.com"; name = "My CMS" }
    subject = "Test Email"
    content = @(
        @{ 
            type = "text/plain"
            value = "This is a test email from SkyCMS."
        }
    )
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://api.sendgrid.com/v3/mail/send" `
    -Method Post `
    -Headers $headers `
    -Body $body
```

## SendGrid features and considerations

### Advantages

- **Reliable delivery**: High uptime and bounce/spam monitoring
- **Detailed analytics**: Track opens, clicks, bounces, spam reports
- **Free tier**: 100 emails/day for development
- **API-based**: No SMTP server configuration needed
- **Sandbox mode**: Test without sending real emails
- **Templates**: Use SendGrid templates for consistent branding

### Limitations

- **Free tier limits**: Only 100 emails/day and limited support
- **Paid tier required** for production use (starts ~$10/month for 30k emails)
- **Requires API management**: Keys must be kept secure

### Sandbox Mode (Testing)

In development, enable **Sandbox Mode** to test email functionality without sending real emails:

**Cannot set via environment variables**; must be configured in code or setup wizard.

Once enabled, emails appear to send successfully but are discarded by SendGrid.

## Troubleshooting

### "Invalid API Key" error

- Verify the API key is copied correctly (no extra spaces)
- Check the API key has **Mail Send** permission
- Regenerate the API key if unsure

### "From email not verified" error

- The sender email must be verified in SendGrid
- Go **Settings** → **Sender Authentication** and verify the email

### Email not received

- Check spam/junk folder
- Verify recipient email is correct and accepts emails
- Check SendGrid dashboard **Activity** to see send status
- Enable SPF/DKIM DNS records for better deliverability

### "Unauthorized" or "403" response

- API key lacks proper permissions
- Create a new API key with **Mail Send** permission enabled

## Best practices

- **Use Domain Authentication** in production for better deliverability
- **Enable SPF and DKIM** records for your domain
- **Monitor bounce rates** and unsubscribe feedback
- **Rotate API keys** periodically for security
- **Use restricted API keys** (Mail Send only) following least privilege
- **Never commit API keys** to source control; use environment variables
- **Monitor SendGrid's uptime status** for issues

## See Also

- [SendGrid API Documentation](https://docs.sendgrid.com/for-developers/sending-email/api-getting-started)
- [SendGrid Sandbox Mode](https://docs.sendgrid.com/for-developers/sending-email/sandbox-mode)
- [Email Provider Overview](./Email-Overview.md)
- [Email Configuration Reference](./Email-Configuration-Reference.md)
- [Main Configuration Guide](./README.md)

---
title: No-Op Email Provider (Development/Testing)
description: Development-only email provider that logs email attempts without sending for local development and testing
keywords: email, no-op, development, testing, email-provider
audience: [developers, administrators]
---

# No-Op Email Provider (Development/Testing)

The **No-Op Email Provider** ("No Operation") is a development-only email provider that logs all email send attempts without actually sending emails. Use this for:

- **Local development** when you don't want to send real emails
- **Testing** authentication flows without external dependencies
- **CI/CD pipelines** where external email services aren't needed
- **Demo environments** where email delivery isn't important

## How No-Op Email Works

When No-Op is active:

- Email send calls **appear to succeed** (no errors thrown)
- Emails are **logged** to the application's logging system
- **No actual emails are sent**
- Perfect for testing ASP.NET Identity features like password reset flows

## When No-Op is Used

SkyCMS automatically uses No-Op email when:

1. **No other provider is configured** (SMTP, Azure Communication, SendGrid all missing)
2. **All configuration is empty** and you're in development mode

## Configuration

### Automatic (Default)

Simply don't configure any other email provider:

```powershell
# Don't set any of these:
# - SmtpEmailProviderOptions__*
# - CosmosSendGridApiKey
# - ConnectionStrings__AzureCommunicationConnection
```

### Explicit (Optional)

You can explicitly request No-Op in code (usually not needed):

```csharp
// In Program.cs
services.AddNoOpEmailSender();
```

Or through appsettings.json by ensuring no email provider is configured:

```json
{
  "AdminEmail": "admin@yourdomain.com"
  // No SmtpEmailProviderOptions
  // No CosmosSendGridApiKey
  // No AzureCommunicationConnection
}
```

## Viewing "Sent" Emails

Since No-Op doesn't send emails, you won't find them in an inbox. Instead, monitor these:

### Application Logs

Check your application's logs (console, file, or Application Insights):

```
[Info] Email send attempted to: user@example.com, Subject: Password Reset
[Info] Email send attempted to: admin@yourdomain.com, Subject: System Notification
```

### Debug Output

During local development, emails appear in Visual Studio's Debug output:

```
NoOpEmailService: Processed email to user@example.com
```

### Email Service Response

SkyCMS logs include the send result:

```
SendResult.StatusCode: OK (200)
SendResult.Message: "NoOpEmailSender"
```

## Testing ASP.NET Identity Features

### Password Reset Flow

1. Request a password reset (No-Op logs it, doesn't send)
2. Check logs for the reset token
3. Extract the token from logs and construct the reset URL manually
4. Use the URL to test password reset without needing email

### Email Confirmation

1. Register a new account (No-Op logs confirmation email)
2. Check logs for confirmation link
3. Extract and use the link to test confirmation flow

### Account Lockout

1. Trigger lockout scenarios
2. No-Op logs the lockout notifications
3. Verify the flow works without external dependencies

## Development Environment Setup

### Local Development

```powershell
# PowerShell - no email configuration needed
$env:AdminEmail = "admin@yourdomain.local"
# Start SkyCMS with setup wizard or manually
dotnet run --configuration Development
```

Check console output for logged emails.

### Docker Development

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0

# Minimal configuration - No-Op is default
ENV AdminEmail="admin@yourdomain.local"
ENV CosmosAllowSetup="true"

COPY . /app
WORKDIR /app
ENTRYPOINT ["dotnet", "run"]
```

### Docker Compose

```yaml
services:
  skycms:
    image: skycms:latest
    environment:
      AdminEmail: "admin@yourdomain.local"
      CosmosAllowSetup: "true"
    ports:
      - "5000:80"
```

## Logging Configuration

### Configure Detailed Logging (appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Sky.Editor.Services.Email": "Debug"
    }
  }
}
```

### View Logs in Visual Studio

1. **View** → **Output** → **Debug**
2. Set filter to show "Sky.Editor" logs
3. Perform an action that triggers email (password reset, etc.)
4. Observe email send attempts in the output

### View Logs in Console

```powershell
# On Windows
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --configuration Development 2>&1 | grep -i email

# On Linux/macOS
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --configuration Development 2>&1 | grep -i email
```

## When to Switch from No-Op to a Real Provider

Before deploying to production, migrate to a real email provider:

### Pre-Production Testing

1. **Staging environment**: Switch to SendGrid/SMTP with test sender
2. **Send test emails**: Verify the configuration works
3. **Monitor delivery**: Confirm emails arrive as expected

### Production Deployment

Set environment variables **before** starting the application:

```powershell
# Example: Switch from No-Op to SendGrid
$env:CosmosSendGridApiKey = "SG.xxxxxxxxxxxxx"
$env:AdminEmail = "noreply@yourdomain.com"
# Then start SkyCMS
```

## No-Op Provider Features and Limitations

### Advantages

- **No configuration required**: Works out of the box
- **No external dependencies**: No API keys, SMTP servers, or Azure accounts needed
- **No costs**: Free for unlimited "sends"
- **Perfect for development**: Fast iteration without waiting for real emails
- **Debugging**: Logs all email content for inspection

### Limitations

- **Emails never sent**: Can't test actual delivery
- **No real-world testing**: Recipient behavior unknown
- **Development only**: Never use in production
- **No deliverability metrics**: Can't track bounces, opens, etc.
- **Logs required for inspection**: Must check logs to see what would be sent

## Migration Path

### From Development (No-Op) to Production

1. **Create email account** (Gmail, Office 365, SendGrid, etc.)
2. **Get credentials** (API key, SMTP settings, connection string)
3. **Test in staging** with real email to confirm settings
4. **Deploy to production** with the configured provider
5. **Monitor delivery** with real users

Example progression:

```powershell
# Step 1: Development (No-Op, automatic)
# - No configuration needed
# - Check logs for "sent" emails

# Step 2: Staging (SMTP test server)
$env:SmtpEmailProviderOptions__Host = "smtp.gmail.com"
$env:SmtpEmailProviderOptions__Port = "587"
$env:SmtpEmailProviderOptions__UserName = "test-account@gmail.com"
$env:SmtpEmailProviderOptions__Password = "app-password"
$env:AdminEmail = "test-account@gmail.com"

# Step 3: Production (SMTP or SendGrid)
$env:SmtpEmailProviderOptions__Host = "smtp.office365.com"
$env:SmtpEmailProviderOptions__Port = "587"
$env:SmtpEmailProviderOptions__UserName = "noreply@yourdomain.com"
$env:SmtpEmailProviderOptions__Password = "${{ secrets.OFFICE365_EMAIL_PASSWORD }}"
$env:AdminEmail = "noreply@yourdomain.com"
```

## Troubleshooting

### "Where are my emails?" (No emails received)

- This is normal for No-Op! No emails are actually sent
- Check application logs instead
- Before production, switch to a real provider

### "How do I enable real emails?"

- Don't use No-Op in production
- Configure a real provider (SendGrid, SMTP, Azure Communication)
- See [Email Provider Overview](./Email-Overview.md)

### "I need to test email in production"

- **Don't use No-Op in production**
- Use a dedicated email address with one real provider
- Or use SendGrid sandbox mode for testing

## See Also

- [Email Provider Overview](./Email-Overview.md) - Choose a real provider
- [Email Configuration Reference](./Email-Configuration-Reference.md) - All configuration options
- [SMTP Email Setup](./Email-SMTP.md) - Easy provider to get started
- [Main Configuration Guide](./README.md)

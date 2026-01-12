# Configuration Guide - Sky.Cms.Api.Shared

## Overview

The Sky.Cms.Api.Shared API uses a configuration-driven approach. All settings are defined in the `ContactApiConfig` class and can be configured via `appsettings.json` or environment variables.

## Configuration Structure

### appsettings.json Example

```json
{
  "ContactApi": {
    "AdminEmail": "support@yourdomain.com",
    "MaxMessageLength": 5000,
    "RequireCaptcha": true,
    "CaptchaProvider": "turnstile",
    "CaptchaSiteKey": "your-turnstile-site-key",
    "CaptchaSecretKey": "your-turnstile-secret-key"
  }
}
```

## Configuration Options

### AdminEmail
- **Type**: `string`
- **Required**: Yes
- **Description**: The email address where contact form submissions will be sent
- **Example**: `"support@yourdomain.com"`
- **Environment Variable**: `ContactApi__AdminEmail`

### MaxMessageLength
- **Type**: `int`
- **Default**: `5000`
- **Required**: No
- **Description**: Maximum allowed length for the message field in characters
- **Validation**: Applied at the model level; requests exceeding this will be rejected
- **Environment Variable**: `ContactApi__MaxMessageLength`

### RequireCaptcha
- **Type**: `bool`
- **Default**: `false`
- **Required**: No
- **Description**: Whether CAPTCHA validation is required for contact form submissions
- **Note**: If `true`, one of the CAPTCHA providers must be configured
- **Environment Variable**: `ContactApi__RequireCaptcha`

### CaptchaProvider
- **Type**: `string` (optional)
- **Allowed Values**: `"turnstile"` or `"recaptcha"`
- **Default**: Auto-detected based on available keys
- **Description**: Which CAPTCHA provider to use
- **Auto-Detection Order**:
  1. Checks for Turnstile keys (TURNSTILE_SITE_KEY, TURNSTILE_SECRET_KEY)
  2. Checks for reCAPTCHA keys (RECAPTCHA_SITE_KEY, RECAPTCHA_SECRET_KEY)
  3. Defaults to `null` if neither is configured
- **Environment Variable**: `ContactApi__CaptchaProvider`

### CaptchaSiteKey
- **Type**: `string` (optional)
- **Description**: Public CAPTCHA key
- **For reCAPTCHA**: Your reCAPTCHA v3 site key
- **For Turnstile**: Your Cloudflare Turnstile site key
- **Environment Variables**:
  - `RECAPTCHA_SITE_KEY` or `ContactApi__CaptchaSiteKey`
  - `TURNSTILE_SITE_KEY` or `ContactApi__CaptchaSiteKey`

### CaptchaSecretKey
- **Type**: `string` (optional)
- **Description**: Private CAPTCHA key for server-side validation
- **For reCAPTCHA**: Your reCAPTCHA secret key
- **For Turnstile**: Your Cloudflare Turnstile secret key
- **Security**: Should never be committed to source control; use secrets management
- **Environment Variables**:
  - `RECAPTCHA_SECRET_KEY` or `ContactApi__CaptchaSecretKey`
  - `TURNSTILE_SECRET_KEY` or `ContactApi__CaptchaSecretKey`

## Environment Variable Configuration

Instead of `appsettings.json`, you can use environment variables. The format follows ASP.NET Core conventions:

```bash
# Linux/macOS
export ContactApi__AdminEmail="support@yourdomain.com"
export ContactApi__MaxMessageLength="5000"
export ContactApi__RequireCaptcha="true"
export TURNSTILE_SITE_KEY="your-turnstile-site-key"
export TURNSTILE_SECRET_KEY="your-turnstile-secret-key"
```

```powershell
# Windows PowerShell
$env:ContactApi__AdminEmail = "support@yourdomain.com"
$env:ContactApi__MaxMessageLength = "5000"
$env:ContactApi__RequireCaptcha = "true"
$env:TURNSTILE_SITE_KEY = "your-turnstile-site-key"
$env:TURNSTILE_SECRET_KEY = "your-turnstile-secret-key"
```

## Configuration Scenarios

### Scenario 1: Production with Turnstile CAPTCHA

```json
{
  "ContactApi": {
    "AdminEmail": "support@yourdomain.com",
    "MaxMessageLength": 5000,
    "RequireCaptcha": true,
    "CaptchaProvider": "turnstile",
    "CaptchaSiteKey": "YOUR_TURNSTILE_SITE_KEY",
    "CaptchaSecretKey": "YOUR_TURNSTILE_SECRET_KEY"
  }
}
```

### Scenario 2: Production with reCAPTCHA

```json
{
  "ContactApi": {
    "AdminEmail": "support@yourdomain.com",
    "MaxMessageLength": 5000,
    "RequireCaptcha": true,
    "CaptchaProvider": "recaptcha",
    "CaptchaSiteKey": "YOUR_RECAPTCHA_SITE_KEY",
    "CaptchaSecretKey": "YOUR_RECAPTCHA_SECRET_KEY"
  }
}
```

### Scenario 3: Development (No CAPTCHA)

```json
{
  "ContactApi": {
    "AdminEmail": "dev-support@localhost",
    "MaxMessageLength": 5000,
    "RequireCaptcha": false
  }
}
```

### Scenario 4: Auto-Detected CAPTCHA

```json
{
  "ContactApi": {
    "AdminEmail": "support@yourdomain.com",
    "MaxMessageLength": 5000,
    "RequireCaptcha": true
  }
}
```

With environment variables:
```bash
export TURNSTILE_SITE_KEY="your-site-key"
export TURNSTILE_SECRET_KEY="your-secret-key"
```

The `CaptchaProvider` will be automatically set to `"turnstile"`.

## Rate Limiting Configuration

Rate limiting is configured in `ContactApiServiceExtensions.cs` and is not user-configurable via `appsettings.json`:

- **Limit**: 5 requests per minute
- **Window**: 60 seconds
- **Partition Key**: Client IP address
- **Queue Limit**: 0 (excess requests are rejected immediately)

To modify rate limits, you would need to change the code in:
```csharp
public static void ConfigureContactApiRateLimiting(RateLimiterOptions options)
{
    // Modify the FixedWindowRateLimiterOptions here
    new FixedWindowRateLimiterOptions
    {
        PermitLimit = 5,           // Change this
        Window = TimeSpan.FromMinutes(1),  // Or this
    }
}
```

## Secrets Management

### Development

For local development with secrets, use the .NET User Secrets manager:

```bash
# Navigate to the host application directory
cd YourWebApp

# Set secrets
dotnet user-secrets set "ContactApi:CaptchaSiteKey" "your-dev-key"
dotnet user-secrets set "ContactApi:CaptchaSecretKey" "your-dev-secret"

# List secrets
dotnet user-secrets list
```

### Production

For production deployments:

**Azure App Service**: Use Application Settings in Azure Portal or Azure CLI
```bash
az webapp config appsettings set --name myapp --resource-group mygroup \
  --settings ContactApi__AdminEmail="support@yourdomain.com" \
  ContactApi__CaptchaSecretKey="your-secret-key"
```

**Docker/Kubernetes**: Use environment variables or secrets management systems
```yaml
env:
  - name: ContactApi__AdminEmail
    value: "support@yourdomain.com"
  - name: ContactApi__CaptchaSecretKey
    valueFrom:
      secretKeyRef:
        name: contact-api-secrets
        key: captcha-secret
```

**GitHub Secrets** (for CI/CD): Store in repository secrets and use in workflows

## Configuration Validation

The API validates configuration at startup:

1. **AdminEmail is required** - Application will fail to start without a valid email
2. **CAPTCHA keys must match provider** - If `RequireCaptcha` is true, appropriate keys must be configured
3. **Message length is reasonable** - `MaxMessageLength` defaults to 5000 if not specified

If configuration is invalid, check application logs for detailed error messages.

## Troubleshooting Configuration Issues

### "CAPTCHA validation failed"
- Verify CAPTCHA keys are correctly configured
- Ensure keys match the provider type (don't mix Turnstile and reCAPTCHA keys)
- Check that `RequireCaptcha` is set to true

### "Email delivery failed"
- Verify `AdminEmail` is a valid email address
- Check email service configuration in the host application
- Review email service logs for failures

### Rate limiting rejecting legitimate requests
- Verify client IP address isn't being masked by a proxy
- Check rate limit configuration if custom changes were made
- Review logs for actual request rates

### No CAPTCHA appearing in client
- Verify `CaptchaSiteKey` is configured and correct
- Ensure the JavaScript library endpoint is accessible: `/_api/contact/skycms-contact.js`
- Check browser console for JavaScript errors

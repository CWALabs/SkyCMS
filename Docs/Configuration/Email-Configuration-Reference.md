---
title: Email Configuration Reference
description: Complete reference for all email provider configuration options and environment variables
keywords: email, configuration-reference, environment-variables, SendGrid, SMTP, Azure
audience: [developers, administrators]
---

# Email Configuration Reference

Complete reference for all email provider configuration options, environment variables, and settings.

## Configuration Priority

SkyCMS checks for email providers in this order (first available is used):

1. **SMTP** (if fully configured)
2. **Azure Communication Services** (if connection string exists)
3. **SendGrid** (if API key exists)
4. **No-Op** (default fallback)

## SMTP Configuration

### Environment Variables

SkyCMS supports TWO sets of environment variable names for SMTP:

**Option 1: Simplified Names (Recommended)**
```powershell
# Simple names used by EmailConfigurationService
$env:SmtpHost = "smtp.gmail.com"
$env:SmtpPort = "587"
$env:SmtpUsername = "user@example.com"
$env:SmtpPassword = "password"
$env:SenderEmail = "user@example.com"  # Or use AdminEmail
```

**Option 2: ASP.NET Configuration Names**
```powershell
# Standard ASP.NET configuration binding names
$env:SmtpEmailProviderOptions__Host = "smtp.gmail.com"
$env:SmtpEmailProviderOptions__Port = "587"
$env:SmtpEmailProviderOptions__UserName = "user@example.com"
$env:SmtpEmailProviderOptions__Password = "password"
$env:SmtpEmailProviderOptions__UsesSsl = "false"
$env:AdminEmail = "user@example.com"
```

**Note**: Use **either** set of names, not both. Simplified names are checked first by `EmailConfigurationService`.

### appsettings.json

```json
{
  "SmtpEmailProviderOptions": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UserName": "user@example.com",
    "Password": "password",
    "UsesSsl": false
  },
  "AdminEmail": "user@example.com"
}
```

### Settings Reference

#### Simplified Environment Variables (Recommended)

| Setting | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `SmtpHost` | String | ✅ Yes | N/A | SMTP server address (e.g., `smtp.gmail.com`) |
| `SmtpPort` | Integer | No | `587` | SMTP port (typically `587` for TLS, `465` for SSL) |
| `SmtpUsername` | String | ✅ Yes | N/A | Email address or account name for authentication |
| `SmtpPassword` | String | ✅ Yes | N/A | Password or app-specific password |
| `SenderEmail` | String | ✅ Yes* | N/A | Default sender email address |
| `AdminEmail` | String | ✅ Yes* | N/A | Fallback sender email (used if SenderEmail not set) |

*Either `SenderEmail` OR `AdminEmail` must be set.

#### ASP.NET Configuration Names (Alternative)

| Setting | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `SmtpEmailProviderOptions:Host` | String | ✅ Yes | N/A | SMTP server address (e.g., `smtp.gmail.com`) |
| `SmtpEmailProviderOptions:Port` | Integer | ✅ Yes | N/A | SMTP port (typically `587` for TLS, `465` for SSL) |
| `SmtpEmailProviderOptions:UserName` | String | ✅ Yes | N/A | Email address or account name for authentication |
| `SmtpEmailProviderOptions:Password` | String | ✅ Yes | N/A | Password or app-specific password |
| `SmtpEmailProviderOptions:UsesSsl` | Boolean | No | `false` | `false` = TLS (default), `true` = SSL (legacy) |
| `AdminEmail` | String | ✅ Yes | N/A | Default sender email address |

### Example Configurations

**Gmail (with app password)**:
```json
{
  "SmtpEmailProviderOptions": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UserName": "your-email@gmail.com",
    "Password": "your-16-char-app-password",
    "UsesSsl": false
  },
  "AdminEmail": "your-email@gmail.com"
}
```

**Office 365**:
```json
{
  "SmtpEmailProviderOptions": {
    "Host": "smtp.office365.com",
    "Port": 587,
    "UserName": "your-email@company.com",
    "Password": "your-password",
    "UsesSsl": false
  },
  "AdminEmail": "your-email@company.com"
}
```

**Amazon SES**:
```json
{
  "SmtpEmailProviderOptions": {
    "Host": "email-smtp.us-east-1.amazonaws.com",
    "Port": 587,
    "UserName": "AKIAIOSFODNN7EXAMPLE",
    "Password": "BHK7d+SomeVeryLongSMTPPassword",
    "UsesSsl": false
  },
  "AdminEmail": "verified-sender@yourdomain.com"
}
```

## Azure Communication Services Configuration

### Environment Variables

SkyCMS supports TWO ways to set the Azure connection string:

**Option 1: Direct Configuration (Recommended)**
```powershell
# Used by EmailConfigurationService - simpler
$env:ConnectionStrings__AzureEmailConnectionString = "endpoint=https://yourresource.communication.azure.com/;accesskey=xxxxxxxxxxxxx=="
$env:SenderEmail = "DoNotReply@yourdomain.azurecomm.net"
```

**Option 2: ASP.NET Connection String**
```powershell
# Standard ASP.NET connection string format
$env:ConnectionStrings__AzureCommunicationConnection = "endpoint=https://yourresource.communication.azure.com/;accesskey=xxxxxxxxxxxxx=="
$env:AdminEmail = "DoNotReply@yourdomain.azurecomm.net"
```

**Note**: The code checks for `AzureEmailConnectionString` first, then falls back to `AzureCommunicationConnection`.

### appsettings.json

```json
{
  "ConnectionStrings": {
    "AzureCommunicationConnection": "endpoint=https://yourresource.communication.azure.com/;accesskey=xxxxxxxxxxxxx=="
  },
  "AdminEmail": "DoNotReply@yourdomain.azurecomm.net"
}
```

### Settings Reference

| Setting | Type | Required | Description |
| --- | --- | --- | --- |
| `ConnectionStrings:AzureCommunicationConnection` | String | ✅ Yes | Full connection string from Azure Communications resource (includes endpoint and access key) |
| `AdminEmail` | String | ✅ Yes | Verified sender email address from your Azure Communication Services domain |

### Connection String Format

```
endpoint=https://{resource-name}.communication.azure.com/;accesskey={access-key}==
```

**Components**:
- `endpoint`: HTTPS URL of your Azure Communication Services resource
- `accesskey`: Base64-encoded access key from Azure Portal (Settings → Keys)

### Example Connection String

```
endpoint=https://skycms-email.communication.azure.com/;accesskey=AbCdEfGhIjKlMnOpQrStUvWxYz1234567890AbCdEfG==
```

## SendGrid Configuration

### Environment Variables

```powershell
# SendGrid API key - only one variable name supported
$env:CosmosSendGridApiKey = "SG.xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"

# Sender email - either name works
$env:SenderEmail = "noreply@yourdomain.com"  # Preferred
# OR
$env:AdminEmail = "noreply@yourdomain.com"  # Fallback
```

**Note**: `SenderEmail` is checked first; if not set, `AdminEmail` is used.

### appsettings.json

```json
{
  "CosmosSendGridApiKey": "SG.xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "AdminEmail": "noreply@yourdomain.com"
}
```

### Settings Reference

| Setting | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `CosmosSendGridApiKey` | String | ✅ Yes | N/A | SendGrid API key with "Mail Send" permission |
| `AdminEmail` | String | ✅ Yes | N/A | Verified sender email address in SendGrid |

### API Key Format

SendGrid API keys start with `SG.` followed by a long string of characters:

```
SG.xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx...xxx
```

**Important**: 
- Must have **Mail Send** permission
- Visible only once when created
- Cannot be recovered if lost (must regenerate)

## No-Op Configuration

### No Configuration Needed

The No-Op provider is automatic when no other provider is configured:

```powershell
# Just don't set any email provider config
# It will default to No-Op
```

### Explicit Configuration (optional)

```json
{
  "AdminEmail": "admin@yourdomain.local"
  // No SmtpEmailProviderOptions
  // No CosmosSendGridApiKey
  // No AzureCommunicationConnection
}
```

| Setting | Type | Required | Description |
| --- | --- | --- | --- |
| `AdminEmail` | String | ✅ Yes | Used in logs for context, though emails aren't actually sent |

## Global Settings (All Providers)

### AdminEmail (Required)

```powershell
$env:AdminEmail = "noreply@yourdomain.com"
```

```json
{
  "AdminEmail": "noreply@yourdomain.com"
}
```

**Purpose**: 
- Default sender email for all emails sent by SkyCMS
- Must be a valid email address
- Required even if using No-Op provider

## Environment Variable Examples

### Complete SMTP Example (Simplified)

```powershell
# Windows PowerShell - Simplified names (RECOMMENDED)
$env:SmtpHost = "smtp.gmail.com"
$env:SmtpPort = "587"
$env:SmtpUsername = "your-email@gmail.com"
$env:SmtpPassword = "app-password-16-chars"
$env:SenderEmail = "your-email@gmail.com"
```

### Complete SMTP Example (ASP.NET Format)

```powershell
# Windows PowerShell - ASP.NET configuration names
$env:SmtpEmailProviderOptions__Host = "smtp.gmail.com"
$env:SmtpEmailProviderOptions__Port = "587"
$env:SmtpEmailProviderOptions__UserName = "your-email@gmail.com"
$env:SmtpEmailProviderOptions__Password = "app-password-16-chars"
$env:AdminEmail = "your-email@gmail.com"
```

### Complete SendGrid Example

```powershell
$env:CosmosSendGridApiKey = "SG.xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
$env:SenderEmail = "noreply@yourdomain.com"  # Or AdminEmail
```

### Complete Azure Communication Example

```powershell
# Recommended format
$env:ConnectionStrings__AzureEmailConnectionString = "endpoint=https://skycms-email.communication.azure.com/;accesskey=xxxxx=="
$env:SenderEmail = "DoNotReply@noreply.azurecomm.net"

# Alternative format (also supported)
$env:ConnectionStrings__AzureCommunicationConnection = "endpoint=https://skycms-email.communication.azure.com/;accesskey=xxxxx=="
$env:AdminEmail = "DoNotReply@noreply.azurecomm.net"
```

## User Secrets (Development)

For local development, use `dotnet user-secrets`:

```powershell
# Initialize user secrets (one time)
dotnet user-secrets init

# Set SMTP configuration
dotnet user-secrets set "SmtpEmailProviderOptions:Host" "smtp.gmail.com"
dotnet user-secrets set "SmtpEmailProviderOptions:Port" "587"
dotnet user-secrets set "SmtpEmailProviderOptions:UserName" "your-email@gmail.com"
dotnet user-secrets set "SmtpEmailProviderOptions:Password" "app-password"
dotnet user-secrets set "AdminEmail" "your-email@gmail.com"

# Or set SendGrid
dotnet user-secrets set "CosmosSendGridApiKey" "SG.xxxxx"
dotnet user-secrets set "AdminEmail" "noreply@yourdomain.com"
```

User secrets are stored in:
- **Windows**: `%APPDATA%\Microsoft\UserSecrets\{app-id}\secrets.json`
- **Linux/macOS**: `~/.microsoft/usersecrets/{app-id}/secrets.json`

## Docker Environment Variables

### Dockerfile

```dockerfile
ENV SmtpEmailProviderOptions__Host="smtp.gmail.com" \
    SmtpEmailProviderOptions__Port="587" \
    SmtpEmailProviderOptions__UserName="your-email@gmail.com" \
    SmtpEmailProviderOptions__Password="app-password" \
    AdminEmail="your-email@gmail.com"
```

### docker-compose.yml

```yaml
services:
  skycms:
    environment:
      SmtpEmailProviderOptions__Host: "smtp.gmail.com"
      SmtpEmailProviderOptions__Port: "587"
      SmtpEmailProviderOptions__UserName: "your-email@gmail.com"
      SmtpEmailProviderOptions__Password: "app-password"
      AdminEmail: "your-email@gmail.com"
```

## Kubernetes (ConfigMap + Secret)

### ConfigMap (non-sensitive)

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: skycms-email-config
data:
  AdminEmail: "noreply@yourdomain.com"
  SmtpEmailProviderOptions__Host: "smtp.gmail.com"
  SmtpEmailProviderOptions__Port: "587"
  SmtpEmailProviderOptions__UserName: "your-email@gmail.com"
```

### Secret (sensitive data)

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: skycms-email-secret
type: Opaque
stringData:
  SmtpEmailProviderOptions__Password: "app-password"
  CosmosSendGridApiKey: "SG.xxxxx"
  ConnectionStrings__AzureCommunicationConnection: "endpoint=...;accesskey=..."
```

### Pod Usage

```yaml
spec:
  containers:
  - name: skycms
    envFrom:
    - configMapRef:
        name: skycms-email-config
    - secretRef:
        name: skycms-email-secret
```

## Validation

### Required Fields Check

SkyCMS validates on startup:

- **SMTP**: All of `Host`, `Port`, `UserName`, `Password` must be present
- **Azure Communication**: `AzureCommunicationConnection` must be non-empty
- **SendGrid**: `CosmosSendGridApiKey` must be non-empty
- **All providers**: `AdminEmail` must be set

If validation fails, the application logs an error and may fail to start.

### Test Configuration

SkyCMS includes a test configuration method in the setup wizard:

1. Enter provider credentials
2. Click **Test Email Settings**
3. Receive immediate feedback on validity

## Common Configuration Errors

| Error | Cause | Solution |
| --- | --- | --- |
| "No email configuration found" | No provider configured | Set at least one provider's required fields |
| "Invalid SMTP port" | Port not a number or invalid range | Use 587 (TLS), 465 (SSL), or 25 (legacy) |
| "Invalid API key" | SendGrid key malformed | Verify API key starts with `SG.` and is complete |
| "Connection refused" | Server/port incorrect | Verify SMTP host and port with provider |
| "Authentication failed" | Wrong credentials | Double-check username and password |
| "AdminEmail not set" | Missing required field | Set `AdminEmail` environment variable |

## See Also

- [Email Provider Overview](./Email-Overview.md) - Provider comparison
- [Email SendGrid Setup](./Email-SendGrid.md) - SendGrid-specific guide
- [Email Azure Communication Setup](./Email-AzureCommunicationServices.md) - Azure setup
- [Email SMTP Setup](./Email-SMTP.md) - SMTP provider guide
- [Email No-Op Setup](./Email-None.md) - Development provider
- [Installation Guide](../Installation/MinimumRequiredSettings.md) - Configuration in setup

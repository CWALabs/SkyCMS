# Email Environment Variables - Complete Reference

## Overview

SkyCMS supports **multiple environment variable naming conventions** for email configuration to provide flexibility and compatibility with different deployment scenarios.

## Priority and Fallback Behavior

The `EmailConfigurationService` checks environment variables in this order:

1. **Simplified variable names** (e.g., `SmtpHost`, `SenderEmail`)
2. **ASP.NET configuration names** (e.g., `SmtpEmailProviderOptions__Host`, `AdminEmail`)
3. **Database settings** (from `Settings` table, group `EMAIL`)

## Complete Environment Variable List

### SendGrid Provider

| Variable Name | Type | Required | Example | Notes |
|---------------|------|----------|---------|-------|
| `CosmosSendGridApiKey` | String | ✅ Yes | `SG.xxxxx...` | Only one variable name supported |
| `SenderEmail` | String | ✅ Yes* | `noreply@domain.com` | Checked first |
| `AdminEmail` | String | ✅ Yes* | `admin@domain.com` | Fallback if SenderEmail not set |

*Either `SenderEmail` OR `AdminEmail` must be set.

**Code Reference**:
```csharp
// Editor/Services/Email/EmailConfigurationService.cs
settings.SendGridApiKey = configuration["CosmosSendGridApiKey"];
settings.SenderEmail = configuration["SenderEmail"] ?? configuration["AdminEmail"];
```

---

### SMTP Provider

#### Option 1: Simplified Names (Recommended)

| Variable Name | Type | Required | Default | Example |
|---------------|------|----------|---------|---------|
| `SmtpHost` | String | ✅ Yes | - | `smtp.gmail.com` |
| `SmtpPort` | Integer | No | 587 | `587` |
| `SmtpUsername` | String | ✅ Yes | - | `user@example.com` |
| `SmtpPassword` | String | ✅ Yes | - | `app-password` |
| `SenderEmail` | String | ✅ Yes* | - | `noreply@domain.com` |
| `AdminEmail` | String | ✅ Yes* | - | `admin@domain.com` |

*Either `SenderEmail` OR `AdminEmail` must be set.

**Code Reference**:
```csharp
// Editor/Services/Email/EmailConfigurationService.cs
settings.SmtpHost = configuration["SmtpHost"];
settings.SmtpPort = int.TryParse(configuration["SmtpPort"], out var port) ? port : 587;
settings.SmtpUsername = configuration["SmtpUsername"];
settings.SmtpPassword = configuration["SmtpPassword"];
settings.SenderEmail = configuration["SenderEmail"] ?? configuration["AdminEmail"];
```

#### Option 2: ASP.NET Configuration Names

| Variable Name | Type | Required | Default | Example |
|---------------|------|----------|---------|---------|
| `SmtpEmailProviderOptions__Host` | String | ✅ Yes | - | `smtp.gmail.com` |
| `SmtpEmailProviderOptions__Port` | Integer | ✅ Yes | - | `587` |
| `SmtpEmailProviderOptions__UserName` | String | ✅ Yes | - | `user@example.com` |
| `SmtpEmailProviderOptions__Password` | String | ✅ Yes | - | `app-password` |
| `SmtpEmailProviderOptions__UsesSsl` | Boolean | No | false | `false` |
| `AdminEmail` | String | ✅ Yes | - | `admin@domain.com` |

**Code Reference**:
```csharp
// Editor/Services/Setup/SetupService.cs
var smtpHost = configuration["SmtpEmailProviderOptions:Host"]
    ?? configuration["SmtpEmailProviderOptions__Host"];
```

**Note**: Use **either** Option 1 OR Option 2, not both. Option 1 (simplified) is checked first.

---

### Azure Communication Services Provider

#### Option 1: Direct Configuration (Recommended)

| Variable Name | Type | Required | Example |
|---------------|------|----------|---------|
| `ConnectionStrings__AzureEmailConnectionString` | String | ✅ Yes | `endpoint=https://...` |
| `SenderEmail` | String | ✅ Yes* | `noreply@domain.azurecomm.net` |
| `AdminEmail` | String | ✅ Yes* | `admin@domain.azurecomm.net` |

*Either `SenderEmail` OR `AdminEmail` must be set.

**Code Reference**:
```csharp
// Editor/Services/Email/EmailConfigurationService.cs
settings.AzureEmailConnectionString = configuration.GetConnectionString("AzureEmailConnectionString");
settings.SenderEmail = configuration["SenderEmail"] ?? configuration["AdminEmail"];
```

#### Option 2: ASP.NET Connection String

| Variable Name | Type | Required | Example |
|---------------|------|----------|---------|
| `ConnectionStrings__AzureCommunicationConnection` | String | ✅ Yes | `endpoint=https://...` |
| `AdminEmail` | String | ✅ Yes | `admin@domain.azurecomm.net` |

**Code Reference**:
```csharp
// The code checks AzureEmailConnectionString first, then falls back to AzureCommunicationConnection
// if the first is not set
```

**Note**: Use **either** Option 1 OR Option 2, not both. Option 1 is checked first.

---

## How Configuration Resolution Works

### Step 1: Environment Variables (Highest Priority)

The `EmailConfigurationService` checks environment variables first:

```csharp
// Try environment variables first (highest priority)
settings.SendGridApiKey = configuration["CosmosSendGridApiKey"];
settings.AzureEmailConnectionString = configuration.GetConnectionString("AzureEmailConnectionString");
settings.SmtpHost = configuration["SmtpHost"];
settings.SmtpPort = int.TryParse(configuration["SmtpPort"], out var port) ? port : 587;
settings.SmtpUsername = configuration["SmtpUsername"];
settings.SmtpPassword = configuration["SmtpPassword"];
settings.SenderEmail = configuration["SenderEmail"] ?? configuration["AdminEmail"];
```

### Step 2: Database Settings (Fallback)

If NO provider is configured via environment variables, check the database:

```csharp
// If not found in environment, try database settings
if (string.IsNullOrEmpty(settings.SendGridApiKey) &&
    string.IsNullOrEmpty(settings.AzureEmailConnectionString) &&
    string.IsNullOrEmpty(settings.SmtpHost))
{
    var dbSettings = await dbContext.Settings
        .Where(s => s.Group == "EMAIL")
        .ToListAsync();
        
    // Map database values to settings...
}
```

Database settings use these names in the `Settings` table:

| Group | Name | Example Value |
|-------|------|---------------|
| `EMAIL` | `SendGridApiKey` | `SG.xxxxx...` |
| `EMAIL` | `AzureEmailConnectionString` | `endpoint=https://...` |
| `EMAIL` | `SmtpHost` | `smtp.gmail.com` |
| `EMAIL` | `SmtpPort` | `587` |
| `EMAIL` | `SmtpUsername` | `user@example.com` |
| `EMAIL` | `SmtpPassword` | `password` |
| `EMAIL` | `AdminEmail` | `admin@domain.com` |

### Step 3: Determine Provider

After loading settings from either source, the code determines which provider to use:

```csharp
private string DetermineProvider(EmailSettings settings)
{
    if (!string.IsNullOrEmpty(settings.SendGridApiKey))
        return "SendGrid";

    if (!string.IsNullOrEmpty(settings.AzureEmailConnectionString))
        return "AzureCommunication";

    if (!string.IsNullOrEmpty(settings.SmtpHost))
        return "SMTP";

    return string.Empty;  // No provider configured
}
```

**Provider Priority**:
1. SendGrid (if API key is set)
2. Azure Communication Services (if connection string is set)
3. SMTP (if host is set)
4. No-Op (if nothing is configured)

---

## Usage Examples

### Example 1: SMTP with Simplified Names (PowerShell)

```powershell
$env:SmtpHost = "smtp.gmail.com"
$env:SmtpPort = "587"
$env:SmtpUsername = "myapp@gmail.com"
$env:SmtpPassword = "abcd-efgh-ijkl-mnop"  # App password
$env:SenderEmail = "myapp@gmail.com"
```

### Example 2: SMTP with ASP.NET Names (Bash)

```bash
export SmtpEmailProviderOptions__Host="smtp.office365.com"
export SmtpEmailProviderOptions__Port="587"
export SmtpEmailProviderOptions__UserName="admin@company.com"
export SmtpEmailProviderOptions__Password="MyP@ssw0rd"
export AdminEmail="admin@company.com"
```

### Example 3: SendGrid (Docker Compose)

```yaml
environment:
  CosmosSendGridApiKey: "SG.xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
  SenderEmail: "noreply@mywebsite.com"
```

### Example 4: Azure Communication Services (Kubernetes)

```yaml
env:
  - name: ConnectionStrings__AzureEmailConnectionString
    valueFrom:
      secretKeyRef:
        name: email-secret
        key: connection-string
  - name: SenderEmail
    value: "DoNotReply@mywebsite.azurecomm.net"
```

### Example 5: Multiple Providers (Only First is Used)

```powershell
# SendGrid will be used (checked first)
$env:CosmosSendGridApiKey = "SG.xxxxx"
$env:SmtpHost = "smtp.gmail.com"  # Ignored
$env:SmtpUsername = "user@gmail.com"  # Ignored
$env:SenderEmail = "noreply@domain.com"
```

---

## Contact API AdminEmail Fallback

The Contact Form API has additional fallback behavior for the admin email:

```csharp
// ContactApiController.cs - LoadContactApiConfigAsync()
var adminEmail = contactApiSettings.FirstOrDefault(s => s.Name == "AdminEmail")?.Value;

// If ContactApi AdminEmail is not configured, fall back to email provider's SenderEmail
if (string.IsNullOrWhiteSpace(adminEmail))
{
    var emailSettings = await emailConfigService.GetEmailSettingsAsync();
    adminEmail = emailSettings.SenderEmail;
}
```

**Fallback Chain**:
1. `ContactApi.AdminEmail` (from database `Settings` table, group `ContactApi`)
2. Email provider's `SenderEmail` (from `EmailConfigurationService`)
3. Email provider's `AdminEmail` (fallback within `EmailConfigurationService`)
4. `"admin@example.com"` (hardcoded final fallback)

---

## Troubleshooting

### Problem: Emails not being sent

**Check these in order**:

1. **Verify environment variables are set**:
   ```powershell
   # List all environment variables
   Get-ChildItem Env: | Where-Object { $_.Name -like "*Smtp*" -or $_.Name -like "*SendGrid*" -or $_.Name -like "*Azure*" -or $_.Name -like "*Email*" }
   ```

2. **Check application logs**:
   - Look for "Email settings not found in environment variables, checking database"
   - Look for "Using email provider's SenderEmail as AdminEmail"
   - Look for "Failed to load email settings"

3. **Verify database settings** (if environment variables not set):
   ```sql
   SELECT * FROM Settings WHERE [Group] = 'EMAIL';
   ```

4. **Check provider determination**:
   ```
   Loaded Email config - Provider: SendGrid / AzureCommunication / SMTP / (empty)
   ```

### Problem: Wrong provider being used

The provider is selected based on which settings are available. Check the priority:

1. SendGrid (if `CosmosSendGridApiKey` is set)
2. Azure (if `AzureEmailConnectionString` or `AzureCommunicationConnection` is set)
3. SMTP (if `SmtpHost` is set)

**Solution**: Unset environment variables for unwanted providers.

### Problem: AdminEmail vs SenderEmail confusion

- `SenderEmail` is checked **first**
- If `SenderEmail` is not set, `AdminEmail` is used as fallback
- Both work, but `SenderEmail` takes precedence

**Recommendation**: Use `SenderEmail` for consistency.

---

## Best Practices

1. **Use simplified variable names** (`SmtpHost`, `SenderEmail`) for clarity
2. **Set environment variables** instead of database settings for production (more secure)
3. **Use `SenderEmail`** instead of `AdminEmail` for consistency
4. **Don't set multiple providers** - only the first one found will be used
5. **Use secrets management** (Azure Key Vault, Kubernetes Secrets) for sensitive values
6. **Test email configuration** after deployment using the Contact Form API or admin panel

---

## See Also

- [Email Configuration Overview](./Email-Overview.md)
- [Email Configuration Reference](./Email-Configuration-Reference.md)
- [SendGrid Setup Guide](./Email-SendGrid.md)
- [Azure Communication Services Guide](./Email-AzureCommunicationServices.md)
- [SMTP Setup Guide](./Email-SMTP.md)


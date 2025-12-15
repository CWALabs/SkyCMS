---
Owner: @github-username
Last Reviewed: YYYY-MM-DD
Status: Current | Needs Update | Deprecated
Related Docs: [Overview](./path-to-overview.md), [Configuration Reference](./config-reference.md)
---

# Setting Up [Provider Name] for [Topic]

Step-by-step guide for configuring and using [provider] for [topic] in SkyCMS.

---

## Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Step-by-Step Setup](#step-by-step-setup)
- [Configuration](#configuration)
- [Verification](#verification)
- [Troubleshooting](#troubleshooting)
- [Next Steps](#next-steps)

---

## Overview

[Provider Name] is [brief explanation of what it is and why to use it].

**Advantages:**
- Advantage 1
- Advantage 2
- Advantage 3

**Limitations:**
- Limitation 1
- Limitation 2

**Best for:**
- Use case 1
- Use case 2

---

## Prerequisites

### What You'll Need

Before starting, make sure you have:

- [ ] Active [Provider] account
- [ ] [Required credential/key] - Get from [where]
- [ ] [Required information] - Find in [location]
- [ ] SkyCMS running and accessible
- [ ] Admin access to SkyCMS

### Information to Gather

Before starting setup, gather this information from [Provider]:

| Information | Where to Find | Example |
|-------------|---------------|---------|
| **[Credential 1]** | [Location in provider] | `xxxx-yyyy-zzzz` |
| **[Credential 2]** | [Location in provider] | `key123abc` |
| **[Credential 3]** | [Location in provider] | `https://endpoint.provider.com` |

---

## Step-by-Step Setup

### Step 1: Create [Provider] Account/Project

If you don't already have a [Provider] account:

1. Go to [Provider website](https://provider.com)
2. Click "Sign Up" or "Create Account"
3. Enter required information
4. Verify email
5. Create your first [resource] (project, bucket, service, etc.)

**Example:** [Screenshots or detailed instructions if helpful]

---

### Step 2: Generate Credentials/Keys

[Provider] requires credentials to access your account from SkyCMS.

1. Log into [Provider] console
2. Navigate to **[Section Name]** → **[Subsection]**
3. Click "Create New Key" or "Generate Credentials"
4. Choose key type: [Options]
5. Name it something like: `SkyCMS-[Environment]`
6. Copy the generated key/secret
7. Store in secure location (you won't see this again)

**Security Note:** Never share these keys. Treat them like passwords.

---

### Step 3: Configure SkyCMS

Add [Provider] credentials to your SkyCMS configuration.

**Option A: Using appsettings.json (Development)**

Edit `appsettings.json` in your SkyCMS installation:

```json
{
  "[SectionName]": {
    "AccountId": "your-account-id-here",
    "AccessKey": "your-access-key-here",
    "SecretKey": "your-secret-key-here",
    "Endpoint": "https://your-endpoint.provider.com",
    "Region": "us-east-1"
  }
}
```

**Option B: Using Environment Variables (Production)**

Set these environment variables in your deployment:

```powershell
# PowerShell
$env:[SECTIONNAME]_ACCOUNTID = "your-account-id"
$env:[SECTIONNAME]_ACCESSKEY = "your-access-key"
$env:[SECTIONNAME]_SECRETKEY = "your-secret-key"
$env:[SECTIONNAME]_ENDPOINT = "https://your-endpoint.provider.com"
$env:[SECTIONNAME]_REGION = "us-east-1"
```

**Option C: Using Azure Key Vault (Recommended for Production)**

1. Store secrets in Azure Key Vault
2. Grant SkyCMS access via Managed Identity
3. SkyCMS automatically retrieves secrets

See [CONTRIBUTING.md: Secrets Management](../CONTRIBUTING.md) for details.

---

### Step 4: Test the Connection

Verify that SkyCMS can connect to [Provider].

**Using PowerShell:**

```powershell
# Test if you can reach the Provider endpoint
$endpoint = "https://your-endpoint.provider.com"
$response = Invoke-WebRequest -Uri $endpoint -UseBasicParsing
if ($response.StatusCode -eq 200) { 
    Write-Host "Connection successful!" 
} else { 
    Write-Host "Connection failed with status: $($response.StatusCode)" 
}
```

**In SkyCMS:**

1. Log into SkyCMS admin panel
2. Go to **Settings** → **Configuration**
3. Look for **[Topic] Settings**
4. Click "Test Connection"
5. Should see "Connection successful" message

---

### Step 5: [Additional Configuration if Needed]

[Any additional configuration specific to this provider]

1. Step detail
2. Step detail
3. Step detail

---

## Configuration

### Configuration Options

| Option | Required | Default | Description |
|--------|----------|---------|-------------|
| `AccountId` | Yes | - | Your [Provider] account ID |
| `AccessKey` | Yes | - | API access key from [Provider] |
| `SecretKey` | Yes | - | API secret key from [Provider] |
| `Endpoint` | No | [default] | Custom endpoint URL |
| `Region` | No | `us-east-1` | Service region |
| `Timeout` | No | `30s` | Connection timeout |
| `Retries` | No | `3` | Number of retry attempts |

### Advanced Configuration

For advanced usage:

```json
{
  "[SectionName]": {
    "AccountId": "...",
    "AccessKey": "...",
    "SecretKey": "...",
    "AdvancedOptions": {
      "MaxConnectionPoolSize": 50,
      "ConnectionTimeout": 30000,
      "RequestTimeout": 30000,
      "EnableEncryption": true,
      "LogRequests": false
    }
  }
}
```

---

## Verification

### Verify Setup is Complete

To confirm [Provider] is properly configured:

1. **Check Configuration**
   - Verify all required settings are present
   - Confirm values are correct
   - No typos in credentials

2. **Test Connection**
   - Run "Test Connection" in SkyCMS settings
   - Check SkyCMS logs for errors
   - Verify no firewall/network issues

3. **Test Functionality**
   - [Function test 1] should work
   - [Function test 2] should work
   - [Function test 3] should work

### Expected Behavior

When properly configured:
- ✓ No error messages in logs
- ✓ "Test Connection" returns success
- ✓ [Feature 1] works correctly
- ✓ [Feature 2] works correctly

---

## Troubleshooting

### "Invalid Credentials" Error

**Problem:** SkyCMS can't authenticate with [Provider]

**Causes:**
- Incorrect access key or secret
- Keys have been revoked/regenerated
- Wrong account ID
- Credentials expired

**Solution:**
1. Log into [Provider] console
2. Verify your current credentials
3. If keys were revoked, generate new ones
4. Update SkyCMS configuration with correct values
5. Test connection again

---

### "Connection Timeout" Error

**Problem:** SkyCMS can't reach [Provider]

**Causes:**
- Network connectivity issue
- Endpoint URL incorrect
- Firewall blocking connection
- [Provider] service down

**Solution:**
1. Verify endpoint URL is correct
2. Check internet connectivity: `ping provider.com`
3. Check firewall/proxy settings
4. Check [Provider] service status page
5. Verify no VPN blocking connection

---

### "Insufficient Permissions" Error

**Problem:** Credentials don't have required permissions

**Causes:**
- API key doesn't have required permissions
- Account doesn't have access to resource
- Permissions recently revoked

**Solution:**
1. Log into [Provider] console
2. Check IAM/permissions for API key
3. Grant required permissions: [List needed]
4. Generate new key if needed
5. Test connection again

---

### [Other Common Issue]

**Problem:** [Problem description]

**Causes:**
- Cause 1
- Cause 2

**Solution:**
1. Step 1
2. Step 2
3. Step 3

---

## Performance Tuning

For optimal performance with [Provider]:

### Connection Pooling

```json
{
  "[SectionName]": {
    "MaxConnectionPoolSize": 50,
    "IdleConnectionTimeout": 300000
  }
}
```

### Caching

```json
{
  "[SectionName]": {
    "CacheEnabled": true,
    "CacheTtlSeconds": 3600
  }
}
```

### Request Optimization

```json
{
  "[SectionName]": {
    "BatchSize": 100,
    "ParallelRequests": 5
  }
}
```

---

## See Also

- **[Overview: [Topic]](./path-to-overview.md)** - Compare all [topic] options
- **[Configuration Reference](./config-reference.md)** - General [topic] configuration
- **[[Provider] Official Documentation]** - [Provider's official docs]
- **[Troubleshooting Guide](../Troubleshooting.md)** - Help with common issues

---

## Next Steps

1. **Verify Setup** - Follow [Verification](#verification) section above
2. **Start Using** - Begin using [feature] with your new [Provider] integration
3. **Optimize** - Review [Performance Tuning](#performance-tuning) for best results
4. **Share Knowledge** - Document any provider-specific workarounds your team finds

---

**Last Updated:** YYYY-MM-DD  
**Owner:** @github-username  
**Provider:** [Provider Name]  
**Tested With:** SkyCMS [version]

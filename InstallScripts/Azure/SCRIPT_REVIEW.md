# Azure Install Scripts Review

**Date:** December 26, 2025  
**Reviewer:** Code Analysis

---

## Executive Summary

The Azure install scripts are **well-structured and professionally written** with good error handling, user guidance, and documentation. However, there are several syntax issues, potential bugs, and best practice improvements that should be addressed.

### Overall Assessment
- ✅ **Good:** Comprehensive pre-flight checks, user-friendly prompts, clear documentation
- ⚠️ **Attention Needed:** 5 critical/high priority issues, several best practice gaps
- ✅ **Best Practices:** Most Azure CLI best practices are followed

---

## Critical Issues

### 1. **deploy-skycms.ps1 - Incomplete Code Output** ❌
**Location:** Lines 300-339  
**Severity:** CRITICAL

The file ends abruptly at line 300 (339 total lines). The deployment results/outputs section is incomplete:

```powershell
# ============================================================================
# DISPLAY RESULTS
# ============================================================================

Write-Header "Deployment Complete!"

$outputs = $deployment.properties.outputs
```

**Missing Content:**
- Output value extraction and display (FQDN, connection strings, etc.)
- Output credential instructions for users
- Deployment summary/next steps

**Fix:** Complete the output section to display:
```powershell
Write-Host "Container App FQDN: $($outputs.containerAppFqdn.value)" -ForegroundColor Green
Write-Host "Storage Account:    $($outputs.storageAccountName.value)" -ForegroundColor Green
Write-Host "Key Vault Name:     $($outputs.keyVaultName.value)" -ForegroundColor Green
# ... and other critical outputs
```

---

### 2. **deploy-skycms.ps1 - MySQL Password Not Masked in Bicep Parameters** ⚠️
**Location:** Lines 260-275  
**Severity:** HIGH

The MySQL admin password is passed as a **plain text parameter** to the Bicep template. While the user input is masked (secure), the password appears in the deployment command and potentially in logs.

**Current Code:**
```powershell
$deploymentResult = az deployment group create `
    --name $deploymentName `
    --resource-group $resourceGroupName `
    --template-file $bicepFile `
    --parameters `
        baseName=$baseName `
        environment=$environment `
        dockerImage=$dockerImage `
        mysqlAdminPassword=$mysqlAdminPassword `  # ❌ Exposed in logs/history
```

**Best Practice Fix:**
- Use `--parameters @paramFile.json` with a temporary file instead
- Or use `--parameters mysqlAdminPassword=@/dev/stdin` (redirected input)
- Delete temporary parameter files immediately

**Recommended:**
```powershell
# Create temporary parameter file
$paramFile = New-TemporaryFile
@{
    "schema" = "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#"
    "contentVersion" = "1.0.0.0"
    "parameters" = @{
        "baseName" = @{ "value" = $baseName }
        "mysqlAdminPassword" = @{ "value" = $mysqlAdminPassword }
        # ... other parameters
    }
} | ConvertTo-Json | Set-Content $paramFile

$deploymentResult = az deployment group create `
    --parameters @$paramFile `
    # ...

Remove-Item $paramFile -Force  # Clean up
```

---

### 3. **helpers.ps1 - Undefined Behavior Without Action Parameter** ⚠️
**Location:** Lines 85-91  
**Severity:** MEDIUM

When no `-Action` parameter is provided, the script calls `Show-Menu` but doesn't properly handle the menu-based action flow.

**Current Code:**
```powershell
if ([string]::IsNullOrWhiteSpace($Action)) {
    Show-Menu
}

Get-ResourceGroup

switch ($Action) {  # ❌ $Action may still be empty after Show-Menu returns
```

**Issue:** The `Show-Menu` function sets `$script:Action` but this happens in a subscope. The switch statement might still have empty `$Action`.

**Fix:**
```powershell
if ([string]::IsNullOrWhiteSpace($Action)) {
    Show-Menu
    # After menu, $Action is set via $script:Action = ...
}

# Ensure Action is set
if ([string]::IsNullOrWhiteSpace($Action)) {
    Write-Host "No action selected" -ForegroundColor Red
    exit 1
}

Get-ResourceGroup

switch ($Action) {
```

---

### 4. **validate-bicep.ps1 - Hardcoded Bicep File Paths** ⚠️
**Location:** Lines 72-77  
**Severity:** MEDIUM

Bicep file paths are hardcoded. If module names change, the script breaks silently.

**Current Code:**
```powershell
$bicepFiles = @(
    "bicep\main.bicep",
    "bicep\modules\keyVault.bicep",
    "bicep\modules\mysql.bicep",
    "bicep\modules\containerApp.bicep",
    "bicep\modules\storage.bicep"  # ❌ If this file is renamed, validation passes but deployment fails
)
```

**Better Approach:**
```powershell
# Discover bicep files dynamically
$bicepDir = Join-Path $PSScriptRoot "bicep"
$bicepFiles = @(
    Join-Path $bicepDir "main.bicep"
)
$bicepFiles += Get-ChildItem -Path (Join-Path $bicepDir "modules") -Filter "*.bicep" -Recurse | ForEach-Object { $_.FullName }
```

---

### 5. **All Scripts - Missing Error Context in Catch Blocks** ⚠️
**Location:** Multiple locations across all scripts  
**Severity:** MEDIUM

Several try-catch blocks suppress error details:

**Examples:**
```powershell
function Test-AzureCLI {
    try {
        $null = az version 2>&1
        return $true
    } catch {
        return $false  # ❌ No error details logged
    }
}
```

**Better Practice:**
```powershell
function Test-AzureCLI {
    try {
        $null = az version 2>&1
        return $true
    } catch {
        Write-Verbose "Azure CLI test failed: $_"
        return $false
    }
}
```

---

## High Priority Issues

### 6. **validate-bicep.ps1 - What-If Analysis Uses Test Password** ⚠️
**Location:** Lines 120-145  
**Severity:** MEDIUM

The what-if analysis hardcodes a test password:

```powershell
$testPassword = "TestPassword123!"  # ❌ Weak, exposed in script
```

**Fix:**
```powershell
# Generate a random password for validation only
$testPassword = -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 16 | ForEach-Object {[char]$_})
```

---

### 7. **deploy-skycms.ps1 - No Retry Logic for Deployments** ⚠️
**Location:** Line 262  
**Severity:** MEDIUM

Azure deployments can fail transiently. No retry logic is implemented.

**Missing:**
```powershell
$maxRetries = 3
$retryCount = 0
$deployed = $false

while (-not $deployed -and $retryCount -lt $maxRetries) {
    try {
        $deploymentResult = az deployment group create ...
        $deployed = $true
    } catch {
        $retryCount++
        if ($retryCount -lt $maxRetries) {
            Write-Warning-Custom "Deployment failed, retrying in 10 seconds..."
            Start-Sleep -Seconds 10
        }
    }
}
```

---

## Best Practice Improvements

### 8. **Missing Input Validation** ⚠️
**Location:** deploy-skycms.ps1, lines 150-180  
**Severity:** LOW-MEDIUM

User inputs are not fully validated:

```powershell
# Base name validation is good:
if ($baseName -notmatch '^[a-z0-9]{3,10}$') { ... }

# But Docker image is NOT validated:
$dockerImage = Get-UserInput -Prompt "Docker Image" -Default "toiyabe/sky-editor:latest" -Required
# Should validate it's a valid image reference format
```

**Suggestion:**
```powershell
function Test-DockerImage {
    param([string]$Image)
    # Must be in format: [registry/]repo[:tag]
    return $Image -match '^[a-z0-9\-\.\/]+:[a-z0-9\-\.]+$' -or $Image -match '^[a-z0-9\-\.\/]+$'
}
```

---

### 9. **No Deployment Rollback Strategy** ⚠️
**Location:** deploy-skycms.ps1  
**Severity:** LOW

If deployment fails, there's no guidance on rollback or cleanup.

**Suggestion:** Add post-failure section:
```powershell
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Deployment failed" -ForegroundColor Red
    Write-Info "To retry, you can:"
    Write-Host "  1. Fix the issue and run .\deploy-skycms.ps1 again" -ForegroundColor Yellow
    Write-Host "  2. Check logs: az deployment group show --name $deploymentName --resource-group $resourceGroupName"
    Write-Host "  3. Cleanup: .\destroy-skycms.ps1"
    exit 1
}
```

---

### 10. **Missing Logging/Diagnostics** ⚠️
**Location:** All scripts  
**Severity:** LOW

Scripts don't write to log files. No audit trail for deployments.

**Suggestion:**
```powershell
$logFile = Join-Path $PSScriptRoot "logs\deployment-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
New-Item -ItemType Directory -Path (Split-Path $logFile) -Force | Out-Null

function Write-Log {
    param([string]$Text, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    "$timestamp [$Level] $Text" | Tee-Object -FilePath $logFile -Append
}
```

---

### 11. **destroy-skycms.ps1 - No Backup Before Deletion** ⚠️
**Location:** Line 180  
**Severity:** LOW-MEDIUM

The script deletes resources immediately without offering backup options.

**Suggestion:**
```powershell
Write-Host "Backup Options:" -ForegroundColor Yellow
$backupDb = Get-YesNoInput "Export MySQL database before deletion?" -Default $true
if ($backupDb) {
    # Offer database export
    Write-Info "To export: az mysql flexible-server export --name $serverName --resource-group $ResourceGroupName"
}
```

---

### 12. **helpers.ps1 - RestartContainerApp Command Syntax** ⚠️
**Location:** Line 137  
**Severity:** MEDIUM

The revision restart command may not work with current Azure CLI versions:

```powershell
az containerapp revision restart `  # ❌ Might be deprecated
    --name $ContainerAppName `
    --resource-group $ResourceGroupName
```

**Better Approach:**
```powershell
# Force new deployment by updating environment variable (triggers new revision)
az containerapp update `
    --name $ContainerAppName `
    --resource-group $ResourceGroupName `
    --set-env-vars RESTART_TIMESTAMP=$(Get-Date -Format 'u')
```

---

### 13. **Missing Function Documentation** ⚠️
**Location:** All helper functions  
**Severity:** LOW

Helper functions lack documentation:

```powershell
function Get-UserInput {
    # Missing comment-based help
    param(...)
}
```

**Add:**
```powershell
<#
.SYNOPSIS
    Prompts user for input with optional default and validation

.PARAMETER Prompt
    The prompt text to display

.PARAMETER Default
    Default value if user presses Enter without input

.PARAMETER Required
    If true, re-prompts if input is empty

.PARAMETER Secure
    If true, uses Read-Host -AsSecureString to hide input
#>
```

---

## Syntax Errors Found

### None Critical Syntax Errors
The PowerShell syntax appears to be correct. No compilation errors detected.

---

## Summary Table

| Issue # | Severity | Category | Status |
|---------|----------|----------|--------|
| 1 | CRITICAL | Incomplete code output | Needs immediate fix |
| 2 | HIGH | Password exposure in logs | Security concern |
| 3 | MEDIUM | Undefined action behavior | Logic error |
| 4 | MEDIUM | Hardcoded file paths | Maintenance issue |
| 5 | MEDIUM | Missing error context | Debugging issue |
| 6 | MEDIUM | Test password in script | Best practice |
| 7 | MEDIUM | No retry logic | Reliability |
| 8 | LOW-MED | Input validation gaps | Best practice |
| 9 | LOW | No rollback strategy | Operations |
| 10 | LOW | Missing logging | Observability |
| 11 | LOW-MED | No backup before delete | Safety |
| 12 | MEDIUM | Deprecated API usage | Compatibility |
| 13 | LOW | Missing documentation | Maintainability |

---

## Recommendations Priority Order

1. **IMMEDIATE:** Fix incomplete output section in deploy-skycms.ps1 (#1)
2. **IMMEDIATE:** Add password masking in deployment parameters (#2)
3. **SOON:** Fix action parameter handling in helpers.ps1 (#3)
4. **SOON:** Update containerapp restart command (#12)
5. **SOON:** Improve error handling in try-catch blocks (#5)
6. **RECOMMENDED:** Add input validation (#8)
7. **RECOMMENDED:** Implement deployment logging (#10)
8. **NICE-TO-HAVE:** Add function documentation (#13)

---

## Conclusion

The scripts demonstrate professional PowerShell development practices overall. The main concerns are:
- **Critical:** Incomplete output section needs completion
- **Security:** Password handling needs improvement
- **Reliability:** Missing retry logic and error context
- **Maintenance:** Hard-coded paths reduce flexibility

Addressing the critical and high-priority issues will significantly improve robustness and security.

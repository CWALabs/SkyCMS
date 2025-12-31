<#
.SYNOPSIS
    Determines which Azure regions support all required resources for SkyCMS deployment.

.DESCRIPTION
    Queries Azure to find regions that support the specific resource types needed for SkyCMS.
    Shows intersection of regions that support ALL required resources based on the database provider.

.PARAMETER DatabaseProvider
    The database provider to deploy (cosmos, mysql, or sql).

.PARAMETER ShowAll
    Show all regions for each resource type, not just the intersection.

.EXAMPLE
    .\Get-AvailableRegions.ps1 -DatabaseProvider mysql
    
.EXAMPLE
    .\Get-AvailableRegions.ps1 -DatabaseProvider cosmos -ShowAll
#>

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('cosmos', 'mysql', 'sql')]
    [string]$DatabaseProvider = 'mysql',
    
    [Parameter(Mandatory=$false)]
    [switch]$ShowAll
)

Write-Host "`n🔍 Checking Azure region availability for SkyCMS deployment..." -ForegroundColor Cyan
Write-Host "Database Provider: $DatabaseProvider`n" -ForegroundColor Yellow

# Define resource types based on database provider
$resourceTypes = @{
    'AppService' = @{
        Namespace = 'Microsoft.Web'
        ResourceType = 'sites'
        DisplayName = 'App Service (Web Apps)'
    }
    'Storage' = @{
        Namespace = 'Microsoft.Storage'
        ResourceType = 'storageAccounts'
        DisplayName = 'Storage Accounts'
    }
    'KeyVault' = @{
        Namespace = 'Microsoft.KeyVault'
        ResourceType = 'vaults'
        DisplayName = 'Key Vault'
    }
}

# Add database-specific resource type
switch ($DatabaseProvider) {
    'mysql' {
        $resourceTypes['Database'] = @{
            Namespace = 'Microsoft.DBforMySQL'
            ResourceType = 'flexibleServers'
            DisplayName = 'MySQL Flexible Server'
        }
    }
    'sql' {
        $resourceTypes['Database'] = @{
            Namespace = 'Microsoft.Sql'
            ResourceType = 'servers'
            DisplayName = 'Azure SQL Database'
        }
    }
    'cosmos' {
        $resourceTypes['Database'] = @{
            Namespace = 'Microsoft.DocumentDB'
            ResourceType = 'databaseAccounts'
            DisplayName = 'Cosmos DB'
        }
    }
}

# Query each resource type for available regions
$regionsByResource = @{}

foreach ($key in $resourceTypes.Keys) {
    $resource = $resourceTypes[$key]
    Write-Host "Checking $($resource.DisplayName)..." -ForegroundColor Gray
    
    try {
        $query = "resourceTypes[?resourceType=='$($resource.ResourceType)'].locations | [0]"
        $locations = az provider show --namespace $resource.Namespace --query $query -o json | ConvertFrom-Json
        
        if ($locations) {
            # Normalize location names (remove spaces, lowercase)
            $normalizedLocations = $locations | ForEach-Object { 
                $_ -replace '\s+', '' | ForEach-Object { $_.ToLower() }
            }
            $regionsByResource[$key] = $normalizedLocations
            
            if ($ShowAll) {
                Write-Host "  ✓ $($locations.Count) regions available" -ForegroundColor Green
                $locations | Sort-Object | ForEach-Object { Write-Host "    - $_" -ForegroundColor DarkGray }
            }
        } else {
            Write-Host "  ✗ No regions found" -ForegroundColor Red
            $regionsByResource[$key] = @()
        }
    }
    catch {
        Write-Host "  ✗ Error querying: $_" -ForegroundColor Red
        $regionsByResource[$key] = @()
    }
}

# Find intersection of all regions
Write-Host "`n📍 Finding regions that support ALL required resources..." -ForegroundColor Cyan

$commonRegions = $null
foreach ($key in $regionsByResource.Keys) {
    if ($null -eq $commonRegions) {
        $commonRegions = $regionsByResource[$key]
    } else {
        $commonRegions = $commonRegions | Where-Object { $regionsByResource[$key] -contains $_ }
    }
}

if ($commonRegions) {
    Write-Host "`n✅ $($commonRegions.Count) regions support all required resources:`n" -ForegroundColor Green
    
    # Get user's current regions (where they have resources)
    Write-Host "Checking your existing resource locations..." -ForegroundColor Gray
    $userRegions = @()
    try {
        $userRegions = az group list --query '[].location' -o json | ConvertFrom-Json | 
            Select-Object -Unique | 
            ForEach-Object { $_ -replace '\s+', '' | ForEach-Object { $_.ToLower() } }
    } catch {
        # Ignore errors
    }
    
    # Sort regions: user's existing regions first, then alphabetically
    $sortedRegions = @()
    $recommendedRegions = $commonRegions | Where-Object { $userRegions -contains $_ } | Sort-Object
    $otherRegions = $commonRegions | Where-Object { $userRegions -notcontains $_ } | Sort-Object
    
    if ($recommendedRegions) {
        Write-Host "🌟 RECOMMENDED (you have resources here):" -ForegroundColor Yellow
        foreach ($region in $recommendedRegions) {
            # Map normalized name back to display name
            $displayName = $region -replace '([a-z])([A-Z])', '$1 $2'
            Write-Host "   $region" -ForegroundColor Green
        }
        Write-Host ""
    }
    
    if ($otherRegions) {
        Write-Host "Other available regions:" -ForegroundColor DarkGray
        foreach ($region in $otherRegions) {
            Write-Host "   $region" -ForegroundColor White
        }
    }
    
    Write-Host "`n💡 Recommended deployment command:" -ForegroundColor Cyan
    $recommendedRegion = if ($recommendedRegions) { $recommendedRegions[0] } else { $commonRegions[0] }
    Write-Host "   az group create --name rg-skycms-dev --location $recommendedRegion" -ForegroundColor White
    Write-Host ""
    
} else {
    Write-Host "`n❌ No regions found that support ALL required resources!" -ForegroundColor Red
    Write-Host "This may indicate a subscription limitation or service availability issue.`n" -ForegroundColor Yellow
}

# Summary
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
Write-Host "Required Resources:" -ForegroundColor Cyan
foreach ($key in $resourceTypes.Keys) {
    $resource = $resourceTypes[$key]
    $count = $regionsByResource[$key].Count
    $status = if ($count -gt 0) { "✓" } else { "✗" }
    $color = if ($count -gt 0) { "Green" } else { "Red" }
    Write-Host "  $status $($resource.DisplayName): $count regions" -ForegroundColor $color
}
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`n" -ForegroundColor DarkGray

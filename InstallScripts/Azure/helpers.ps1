<#
.SYNOPSIS
    Helper commands for managing SkyCMS Azure deployment

.DESCRIPTION
    Common operations for SkyCMS on Azure without full deployment/teardown.
    
.EXAMPLE
    .\helpers.ps1 -Action ViewLogs
    .\helpers.ps1 -Action RestartContainerApp
    .\helpers.ps1 -Action GetConnectionString
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('ViewLogs', 'RestartContainerApp', 'ScaleContainerApp', 'GetConnectionString', 'EnableStaticWebsite', 'UploadToStorage', 'ListResources')]
    [string]$Action,
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$false)]
    [string]$ContainerAppName,
    
    [Parameter(Mandatory=$false)]
    [int]$MinReplicas,
    
    [Parameter(Mandatory=$false)]
    [int]$MaxReplicas,
    
    [Parameter(Mandatory=$false)]
    [string]$StorageAccountName,
    
    [Parameter(Mandatory=$false)]
    [string]$SourcePath
)

$ErrorActionPreference = 'Stop'

function Write-Info {
    param([string]$Text)
    Write-Host "ℹ️  $Text" -ForegroundColor Blue
}

function Write-Success {
    param([string]$Text)
    Write-Host "✅ $Text" -ForegroundColor Green
}

function Show-Menu {
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host " SkyCMS Azure Helper Commands" -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
    
    Write-Host "1. View Container App Logs" -ForegroundColor White
    Write-Host "2. Restart Container App" -ForegroundColor White
    Write-Host "3. Scale Container App" -ForegroundColor White
    Write-Host "4. Get MySQL Connection String" -ForegroundColor White
    Write-Host "5. Enable Static Website on Storage" -ForegroundColor White
    Write-Host "6. Upload Files to Blob Storage" -ForegroundColor White
    Write-Host "7. List All Resources in Resource Group" -ForegroundColor White
    Write-Host "Q. Quit" -ForegroundColor White
    Write-Host ""
    
    $choice = Read-Host "Select an option"
    
    switch ($choice) {
        '1' { $script:Action = 'ViewLogs' }
        '2' { $script:Action = 'RestartContainerApp' }
        '3' { $script:Action = 'ScaleContainerApp' }
        '4' { $script:Action = 'GetConnectionString' }
        '5' { $script:Action = 'EnableStaticWebsite' }
        '6' { $script:Action = 'UploadToStorage' }
        '7' { $script:Action = 'ListResources' }
        'Q' { exit 0 }
        default { Write-Host "Invalid option" -ForegroundColor Red; Show-Menu }
    }
}

function Get-ResourceGroup {
    if ([string]::IsNullOrWhiteSpace($script:ResourceGroupName)) {
        $rgs = az group list --query "[].name" -o json | ConvertFrom-Json
        Write-Host "`nAvailable Resource Groups:" -ForegroundColor Cyan
        $rgs | ForEach-Object { Write-Host "  - $_" -ForegroundColor White }
        $script:ResourceGroupName = Read-Host "`nEnter Resource Group Name"
    }
}

function Get-ContainerAppNameFromUser {
    if ([string]::IsNullOrWhiteSpace($script:ContainerAppName)) {
        $apps = az containerapp list --resource-group $ResourceGroupName --query "[].name" -o json | ConvertFrom-Json
        if ($apps.Count -gt 0) {
            Write-Host "`nContainer Apps in resource group:" -ForegroundColor Cyan
            $apps | ForEach-Object { Write-Host "  - $_" -ForegroundColor White }
        }
        $script:ContainerAppName = Read-Host "`nEnter Container App Name"
    }
}

# Main script
if ([string]::IsNullOrWhiteSpace($Action)) {
    Show-Menu
}

# Verify action was selected
if ([string]::IsNullOrWhiteSpace($Action)) {
    Write-Host "No action selected" -ForegroundColor Red
    exit 1
}

Get-ResourceGroup

switch ($Action) {
    'ViewLogs' {
        Get-ContainerAppNameFromUser
        Write-Info "Fetching logs for $ContainerAppName..."
        az containerapp logs show `
            --name $ContainerAppName `
            --resource-group $ResourceGroupName `
            --follow
    }
    
    'RestartContainerApp' {
        Get-ContainerAppNameFromUser
        Write-Info "Restarting $ContainerAppName..."
        # Force new revision by updating environment variable
        az containerapp update `
            --name $ContainerAppName `
            --resource-group $ResourceGroupName `
            --set-env-vars RESTART_TIMESTAMP=$(Get-Date -Format 'u')
        Write-Success "Container app restart triggered"
    }
    
    'ScaleContainerApp' {
        Get-ContainerAppNameFromUser
        if (-not $MinReplicas) {
            $MinReplicas = [int](Read-Host "Minimum replicas")
        }
        if (-not $MaxReplicas) {
            $MaxReplicas = [int](Read-Host "Maximum replicas")
        }
        
        Write-Info "Scaling $ContainerAppName to $MinReplicas-$MaxReplicas replicas..."
        az containerapp update `
            --name $ContainerAppName `
            --resource-group $ResourceGroupName `
            --min-replicas $MinReplicas `
            --max-replicas $MaxReplicas
        Write-Success "Scaling updated"
    }
    
    'GetConnectionString' {
        $mysqlServers = az mysql flexible-server list --resource-group $ResourceGroupName --query "[].name" -o json | ConvertFrom-Json
        if ($mysqlServers.Count -gt 0) {
            $serverName = $mysqlServers[0]
            $server = az mysql flexible-server show --name $serverName --resource-group $ResourceGroupName | ConvertFrom-Json
            Write-Host "`nMySQL Connection Information:" -ForegroundColor Cyan
            Write-Host "Server: $($server.fullyQualifiedDomainName)" -ForegroundColor White
            Write-Host "Port: 3306" -ForegroundColor White
            Write-Host "Username: $($server.administratorLogin)" -ForegroundColor White
            Write-Host "`nConnection String Template:" -ForegroundColor Cyan
            Write-Host "Server=$($server.fullyQualifiedDomainName);Port=3306;Uid=$($server.administratorLogin);Pwd=<PASSWORD>;Database=<DATABASE>;SslMode=Required;" -ForegroundColor White
        } else {
            Write-Host "No MySQL servers found in resource group" -ForegroundColor Red
        }
    }
    
    'EnableStaticWebsite' {
        if ([string]::IsNullOrWhiteSpace($StorageAccountName)) {
            $storageAccounts = az storage account list --resource-group $ResourceGroupName --query "[].name" -o json | ConvertFrom-Json
            if ($storageAccounts.Count -gt 0) {
                Write-Host "`nStorage Accounts:" -ForegroundColor Cyan
                $storageAccounts | ForEach-Object { Write-Host "  - $_" -ForegroundColor White }
            }
            $StorageAccountName = Read-Host "`nEnter Storage Account Name"
        }
        
        Write-Info "Enabling static website on $StorageAccountName..."
        az storage blob service-properties update `
            --account-name $StorageAccountName `
            --static-website `
            --index-document index.html `
            --404-document 404.html `
            --auth-mode login
        Write-Success "Static website enabled"
    }
    
    'UploadToStorage' {
        if ([string]::IsNullOrWhiteSpace($StorageAccountName)) {
            $storageAccounts = az storage account list --resource-group $ResourceGroupName --query "[].name" -o json | ConvertFrom-Json
            if ($storageAccounts.Count -gt 0) {
                Write-Host "`nStorage Accounts:" -ForegroundColor Cyan
                $storageAccounts | ForEach-Object { Write-Host "  - $_" -ForegroundColor White }
            }
            $StorageAccountName = Read-Host "`nEnter Storage Account Name"
        }
        
        if ([string]::IsNullOrWhiteSpace($SourcePath)) {
            $SourcePath = Read-Host "Enter source directory path"
        }
        
        if (-not (Test-Path $SourcePath)) {
            Write-Host "Source path not found: $SourcePath" -ForegroundColor Red
            exit 1
        }
        
        Write-Info "Uploading files from $SourcePath to $StorageAccountName..."
        az storage blob upload-batch `
            --account-name $StorageAccountName `
            --source $SourcePath `
            --destination '$web' `
            --auth-mode login `
            --overwrite
        Write-Success "Files uploaded"
    }
    
    'ListResources' {
        Write-Info "Resources in $ResourceGroupName..."
        az resource list --resource-group $ResourceGroupName --output table
    }
}

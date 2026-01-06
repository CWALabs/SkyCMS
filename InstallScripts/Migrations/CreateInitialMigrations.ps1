<#
.SYNOPSIS
    Creates initial Entity Framework migrations for SkyCMS across multiple database providers.

.DESCRIPTION
    Generates separate migrations for SQL Server, MySQL, and SQLite.
    Uses a unified design-time DbContext factory with environment variables.
    Auto-generates secure placeholder passwords for migration generation (not used for actual connections).

.PARAMETER Provider
    Specific provider to generate migrations for. Options: SqlServer, MySql, Sqlite, All (default: All)

.PARAMETER MySqlPassword
    Password for MySQL connection (default: auto-generated placeholder)

.EXAMPLE
    .\CreateInitialMigrations.ps1
    Generates migrations for all providers with auto-generated passwords

.EXAMPLE
    .\CreateInitialMigrations.ps1 -Provider SqlServer
    Generates migration only for SQL Server
#>

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("SqlServer", "MySql", "Sqlite", "All")]
    [string]$Provider = "All",

    [Parameter(Mandatory=$false)]
    [string]$MySqlPassword = ""
)

# Script configuration
$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$SolutionRoot = Split-Path -Parent (Split-Path -Parent $ScriptDir)
$EditorProject = Join-Path $SolutionRoot "Editor\Sky.Editor.csproj"

# Function to generate a secure random password
function New-RandomPassword {
    param(
        [int]$Length = 32
    )
    
    $chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*"
    $password = -join ((1..$Length) | ForEach-Object { $chars[(Get-Random -Maximum $chars.Length)] })
    return $password
}

# Verify prerequisites
Write-Host "Verifying prerequisites..."

if (-not (Test-Path $EditorProject)) {
    Write-Error "Sky.Editor project not found at: $EditorProject"
    exit 1
}

# Check if dotnet-ef tools are installed
$efToolsInstalled = dotnet tool list --global | Select-String "dotnet-ef"
if (-not $efToolsInstalled) {
    Write-Host "dotnet-ef tools not found. Installing..."
    dotnet tool install --global dotnet-ef
}

Write-Host "Prerequisites verified"
Write-Host ""

# Function to generate migration for a specific provider
function New-ProviderMigration {
    param(
        [string]$ProviderName,
        [string]$ProviderKey,
        [string]$MigrationName,
        [string]$ConnectionString,
        [string]$OutputDir
    )

    Write-Host "========================================"
    Write-Host "Generating migration for $ProviderName"
    Write-Host "========================================"
    Write-Host "   Provider Key: $ProviderKey"
    Write-Host "   Migration Name: $MigrationName"
    Write-Host "   Output: $OutputDir"
    Write-Host ""

    try {
        # Set environment variables for this migration
        $env:MIGRATION_PROVIDER = $ProviderKey
        $env:MIGRATION_CONNECTION_STRING = $ConnectionString

        # Generate migration using .NET CLI
        $arguments = @(
            "ef", "migrations", "add", $MigrationName,
            "--context", "ApplicationDbContext",
            "--output-dir", $OutputDir,
            "--project", $EditorProject,
            "--verbose"
        )

        Write-Host "   Executing: dotnet $($arguments -join ' ')"
        Write-Host ""

        & dotnet $arguments

        if ($LASTEXITCODE -eq 0) {
            Write-Host "Migration created successfully for $ProviderName"
        } else {
            throw "Migration generation failed with exit code $LASTEXITCODE"
        }
    }
    catch {
        Write-Host "Failed to create migration for $ProviderName"
        Write-Host "   Error: $_"
        throw
    }
    finally {
        # Clear environment variables
        Remove-Item Env:\MIGRATION_PROVIDER -ErrorAction SilentlyContinue
        Remove-Item Env:\MIGRATION_CONNECTION_STRING -ErrorAction SilentlyContinue
    }

    Write-Host ""
}

# Generate migrations based on provider selection
$successCount = 0
$failureCount = 0
$failedProviders = @()

try {
    if ($Provider -eq "All" -or $Provider -eq "SqlServer") {
        try {
            New-ProviderMigration `
                -ProviderName "SQL Server" `
                -ProviderKey "SqlServer" `
                -MigrationName "InitialCreate_SqlServer" `
                -ConnectionString "Server=(localdb)\mssqllocaldb;Database=SkyCMS_Migrations;Trusted_Connection=True;MultipleActiveResultSets=true" `
                -OutputDir "Data/Migrations/SqlServer"
            $successCount++
        }
        catch {
            $failureCount++
            $failedProviders += "SQL Server"
            Write-Host "Continuing with remaining providers..."
            Write-Host ""
        }
    }

    if ($Provider -eq "All" -or $Provider -eq "MySql") {
        try {
            # Auto-generate password if not provided
            if ([string]::IsNullOrWhiteSpace($MySqlPassword)) {
                $MySqlPassword = New-RandomPassword -Length 32
                Write-Host "Auto-generated placeholder password for MySQL migration generation"
                Write-Host "   (This password is only used for generating migration files, not for actual database connections)"
                Write-Host ""
            }

            New-ProviderMigration `
                -ProviderName "MySQL" `
                -ProviderKey "MySql" `
                -MigrationName "InitialCreate_MySql" `
                -ConnectionString "Server=localhost;Port=3306;Database=skycms_migrations;User=root;Password=$MySqlPassword;" `
                -OutputDir "Data/Migrations/MySql"
            $successCount++
        }
        catch {
            $failureCount++
            $failedProviders += "MySQL"
            Write-Host "Continuing with remaining providers..."
            Write-Host ""
        }
    }

    if ($Provider -eq "All" -or $Provider -eq "Sqlite") {
        try {
            New-ProviderMigration `
                -ProviderName "SQLite" `
                -ProviderKey "Sqlite" `
                -MigrationName "InitialCreate_Sqlite" `
                -ConnectionString "Data Source=skycms_migrations.db" `
                -OutputDir "Data/Migrations/Sqlite"
            $successCount++
        }
        catch {
            $failureCount++
            $failedProviders += "SQLite"
            Write-Host "SQLite migration failed..."
            Write-Host ""
        }
    }

    # Summary
    Write-Host "========================================"
    Write-Host "Migration Generation Summary"
    Write-Host "========================================"
    Write-Host "   Successful: $successCount"
    Write-Host "   Failed: $failureCount"
    
    if ($failureCount -gt 0) {
        Write-Host "   Failed providers: $($failedProviders -join ', ')"
    }
    Write-Host ""

    if ($successCount -gt 0) {
        Write-Host "Important Notes:"
        Write-Host "   - Migration files have been generated (not applied to databases)"
        Write-Host "   - Auto-generated passwords are placeholders only"
        Write-Host "   - Use actual database credentials when applying migrations"
        Write-Host ""
        Write-Host "Next steps:"
        Write-Host "  1. Review the generated migration files"
        Write-Host "  2. Commit the migrations to source control"
        Write-Host "  3. Configure real database connections in appsettings.json"
        Write-Host "  4. Test migrations against each database provider"
        Write-Host ""
    }

    if ($failureCount -gt 0 -and $successCount -eq 0) {
        throw "All migration generation attempts failed"
    }
}
catch {
    if ($successCount -eq 0) {
        Write-Host ""
        Write-Host "========================================"
        Write-Host "All migration generation attempts failed!"
        Write-Host "========================================"
        Write-Host "Error: $_"
        exit 1
    }
}

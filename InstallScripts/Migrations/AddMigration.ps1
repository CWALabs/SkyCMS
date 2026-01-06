<#
.SYNOPSIS
    Creates a new Entity Framework migration across all database providers for SkyCMS.

.DESCRIPTION
    Automates the creation of migrations for SQL Server, MySQL, and SQLite.
    Assumes you have already:
    1. Created the entity class
    2. Added the DbSet<T> to ApplicationDbContext
    3. Configured the entity in OnModelCreating (if needed)

.PARAMETER MigrationName
    Name of the migration without provider suffix (e.g., "AddUserProfile").
    The script will automatically append "_SqlServer", "_MySql", "_Sqlite".

.PARAMETER MySqlPassword
    Password for MySQL connection (default: auto-generated placeholder)

.EXAMPLE
    .\AddMigration.ps1 -MigrationName "AddUserProfile"
    Creates migrations: AddUserProfile_SqlServer, AddUserProfile_MySql, AddUserProfile_Sqlite

.EXAMPLE
    .\AddMigration.ps1 -MigrationName "AddBlogComments" -MySqlPassword "mypassword"
    Creates migrations with a specific MySQL password
#>

param(
    [Parameter(Mandatory=$true, HelpMessage="Migration name (e.g., 'AddUserProfile')")]
    [ValidateNotNullOrEmpty()]
    [string]$MigrationName,

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
    param([int]$Length = 32)
    
    $chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*"
    $password = -join ((1..$Length) | ForEach-Object { $chars[(Get-Random -Maximum $chars.Length)] })
    return $password
}

# Function to create migration for a specific provider
function New-ProviderMigration {
    param(
        [string]$ProviderName,
        [string]$ProviderKey,
        [string]$MigrationFullName,
        [string]$ConnectionString,
        [string]$OutputDir
    )

    Write-Host "========================================"
    Write-Host "Creating migration for $ProviderName"
    Write-Host "========================================"
    Write-Host "   Migration: $MigrationFullName"
    Write-Host "   Output: $OutputDir"
    Write-Host ""

    try {
        # Set environment variables
        $env:MIGRATION_PROVIDER = $ProviderKey
        $env:MIGRATION_CONNECTION_STRING = $ConnectionString

        # Generate migration using .NET CLI
        $arguments = @(
            "ef", "migrations", "add", $MigrationFullName,
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
            return $true
        } else {
            throw "Migration generation failed with exit code $LASTEXITCODE"
        }
    }
    catch {
        Write-Host "Failed to create migration for $ProviderName"
        Write-Host "   Error: $_"
        return $false
    }
    finally {
        # Clear environment variables
        Remove-Item Env:\MIGRATION_PROVIDER -ErrorAction SilentlyContinue
        Remove-Item Env:\MIGRATION_CONNECTION_STRING -ErrorAction SilentlyContinue
    }

    Write-Host ""
}

# Verify prerequisites
Write-Host "Verifying prerequisites..."
Write-Host ""

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

# Validate migration name (no spaces, no special characters except underscore)
if ($MigrationName -match '[^a-zA-Z0-9_]') {
    Write-Error "Migration name can only contain letters, numbers, and underscores"
    exit 1
}

Write-Host "========================================"
Write-Host "Creating migrations for: $MigrationName"
Write-Host "========================================"
Write-Host ""

$successCount = 0
$failureCount = 0
$failedProviders = @()

# SQL Server Migration
Write-Host "========================================"
Write-Host "1. SQL Server"
Write-Host "========================================"
Write-Host ""

$success = New-ProviderMigration `
    -ProviderName "SQL Server" `
    -ProviderKey "SqlServer" `
    -MigrationFullName "${MigrationName}_SqlServer" `
    -ConnectionString "Server=(localdb)\mssqllocaldb;Database=SkyCMS_Migrations;Trusted_Connection=True;MultipleActiveResultSets=true" `
    -OutputDir "Data/Migrations/SqlServer"

if ($success) {
    $successCount++
} else {
    $failureCount++
    $failedProviders += "SQL Server"
}

Write-Host ""

# MySQL Migration
Write-Host "========================================"
Write-Host "2. MySQL"
Write-Host "========================================"
Write-Host ""

# Auto-generate MySQL password if not provided
if ([string]::IsNullOrWhiteSpace($MySqlPassword)) {
    $MySqlPassword = New-RandomPassword -Length 32
    Write-Host "Auto-generated placeholder password for MySQL migration generation"
    Write-Host "   (This password is only used for generating migration files, not for actual database connections)"
    Write-Host ""
}

$success = New-ProviderMigration `
    -ProviderName "MySQL" `
    -ProviderKey "MySql" `
    -MigrationFullName "${MigrationName}_MySql" `
    -ConnectionString "Server=localhost;Port=3306;Database=skycms_migrations;User=root;Password=$MySqlPassword;" `
    -OutputDir "Data/Migrations/MySql"

if ($success) {
    $successCount++
} else {
    $failureCount++
    $failedProviders += "MySQL"
}

Write-Host ""

# SQLite Migration
Write-Host "========================================"
Write-Host "3. SQLite"
Write-Host "========================================"
Write-Host ""

$success = New-ProviderMigration `
    -ProviderName "SQLite" `
    -ProviderKey "Sqlite" `
    -MigrationFullName "${MigrationName}_Sqlite" `
    -ConnectionString "Data Source=skycms_migrations.db" `
    -OutputDir "Data/Migrations/Sqlite"

if ($success) {
    $successCount++
} else {
    $failureCount++
    $failedProviders += "SQLite"
}

Write-Host ""

# Summary
Write-Host "========================================"
Write-Host "Migration Generation Summary"
Write-Host "========================================"
Write-Host "   Migration Name: $MigrationName"
Write-Host "   Successful: $successCount"
Write-Host "   Failed: $failureCount"

if ($failureCount -gt 0) {
    Write-Host "   Failed providers: $($failedProviders -join ', ')"
}

Write-Host ""

if ($successCount -gt 0) {
    Write-Host "Important Notes:"
    Write-Host "   - Migration files have been generated in Editor/Data/Migrations/"
    Write-Host "   - Review the generated files before committing"
    Write-Host "   - Migrations will be automatically applied when app starts (if CosmosAllowSetup=true)"
    Write-Host ""
    Write-Host "Next steps:"
    Write-Host "  1. Review the generated migration files"
    Write-Host "  2. Test the migrations locally"
    Write-Host "  3. Commit to source control"
    Write-Host "  4. Deploy to production"
    Write-Host ""
}

if ($failureCount -gt 0 -and $successCount -eq 0) {
    Write-Host "All migration generation attempts failed!"
    exit 1
}

Write-Host "Migration generation completed!"

# Push secrets from JSON to GitHub Actions
# Requires GitHub CLI (gh) to be installed and authenticated

$jsonPath = "$env:APPDATA\Microsoft\UserSecrets\c44b0fbc-a20c-4a15-8e5b-1a9eb09e6ac1\secrets.json"

# Auto-detect repository from git remote
try {
    $gitRemote = git remote get-url origin 2>$null
    if ($gitRemote -match 'github\.com[:/](.+?)(?:\.git)?$') {
        $repo = $matches[1]
        Write-Host "Detected repository: $repo" -ForegroundColor BLack
    } else {
        throw "Could not parse GitHub repository from git remote"
    }
} catch {
    Write-Host "Error: Could not detect GitHub repository. Make sure you're in a git repository directory." -ForegroundColor Red
    Write-Host "You can manually set the repo by editing the script: `$repo = 'owner/repo-name'" -ForegroundColor Black
    exit 1
}

# Check if secrets.json exists
if (-not (Test-Path $jsonPath)) {
    Write-Host "Error: secrets.json not found at $jsonPath" -ForegroundColor Red
    exit 1
}

# Read and parse JSON (remove comments first)
try {
    $jsonContent = Get-Content $jsonPath -Raw
    # Remove single-line comments (// style)
    $jsonContent = $jsonContent -replace '(?m)^\s*//.*$', ''
    # Remove inline comments  
    $jsonContent = $jsonContent -replace '//.*$', ''
    
    $json = $jsonContent | ConvertFrom-Json
    
    Write-Host "Successfully parsed JSON file" -ForegroundColor Black
} catch {
    Write-Host "Error: Failed to parse JSON file: $_" -ForegroundColor Red
    exit 1
}

# Function to flatten nested JSON into key-value pairs
function Flatten-Json {
    param(
        [Parameter(Mandatory=$true)]
        $InputObject,
        [string]$Prefix = ""
    )
    
    $result = @{}
    
    # Get all properties
    $properties = $InputObject.PSObject.Properties
    
    Write-Host "  DEBUG: Processing object with prefix '$Prefix', found $($properties.Count) properties" -ForegroundColor DarkGray
    
    foreach ($property in $properties) {
        $propertyName = $property.Name
        $value = $property.Value
        
        $key = if ($Prefix) { "${Prefix}__${propertyName}" } else { $propertyName }
        
        Write-Host "    DEBUG: Property '$propertyName', Value type: $($value.GetType().Name)" -ForegroundColor DarkGray
        
        # Check if value is a nested object (has properties)
        if ($null -ne $value -and $value -is [PSCustomObject]) {
            Write-Host "    DEBUG: Recursing into nested object '$key'" -ForegroundColor DarkGray
            # Recursively flatten nested objects
            $nested = Flatten-Json -InputObject $value -Prefix $key
            foreach ($nestedKey in $nested.Keys) {
                $result[$nestedKey] = $nested[$nestedKey]
            }
        }
        elseif ($null -ne $value -and $value -isnot [System.Array]) {
            # Only add non-null scalar values
            $stringValue = $value.ToString()
            Write-Host "    DEBUG: Adding scalar value '$key' = '$($stringValue.Substring(0, [Math]::Min(50, $stringValue.Length)))...'" -ForegroundColor DarkGray
            $result[$key] = $stringValue
        }
    }
    
    return $result
}

# Check if gh CLI is available
try {
    $null = gh --version 2>&1
} catch {
    Write-Host "Error: GitHub CLI (gh) is not installed or not in PATH." -ForegroundColor Red
    Write-Host "Install it with: winget install --id GitHub.cli" -ForegroundColor Black
    exit 1
}

# Check if authenticated
try {
    $null = gh auth status 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error: Not authenticated with GitHub CLI. Run: gh auth login" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "Error: Not authenticated with GitHub CLI. Run: gh auth login" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Flattening JSON structure..." -ForegroundColor Black
$secrets = Flatten-Json -InputObject $json

Write-Host ""
Write-Host "Found $($secrets.Count) secrets to upload."
Write-Host ""

# Show all flattened keys
Write-Host "Flattened secrets:" -ForegroundColor Black
foreach ($key in $secrets.Keys | Sort-Object) {
    $valuePreview = $secrets[$key].Substring(0, [Math]::Min(40, $secrets[$key].Length))
    Write-Host "  - $key = $valuePreview..." -ForegroundColor DarkGray
}
Write-Host ""

$successCount = 0
$failCount = 0

# Push each secret to GitHub Actions
foreach ($key in $secrets.Keys) {
    $value = $secrets[$key]
    
    # Skip empty values
    if ([string]::IsNullOrWhiteSpace($value)) {
        Write-Host "Skipping empty secret: $key" -ForegroundColor Yellow
        continue
    }
    
    # Convert key to uppercase - KEEP double underscores for ASP.NET Core config
    $secretName = $key.ToUpper()
    
    Write-Host "Setting secret: $secretName" -NoNewline
    
    try {
        # Use GitHub CLI to set the secret
        $value | gh secret set $secretName --repo $repo 2>&1 | Out-Null
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host " [OK]" -ForegroundColor Black
            $successCount++
        } else {
            Write-Host " [FAILED]" -ForegroundColor Red
            $failCount++
        }
    } catch {
        Write-Host " [FAILED] (Exception: $_)" -ForegroundColor Red
        $failCount++
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Black
Write-Host "Summary:" -ForegroundColor Black
Write-Host "  Successfully set: $successCount secrets" -ForegroundColor Black
if ($failCount -gt 0) {
    Write-Host "  Failed: $failCount secrets" -ForegroundColor Black
} else {
    Write-Host "  Failed: $failCount secrets" -ForegroundColor Black
}
Write-Host "========================================" -ForegroundColor Black
Write-Host ""
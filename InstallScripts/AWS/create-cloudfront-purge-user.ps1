Param(
    [Parameter(Mandatory = $true)] [string] $DistributionId,
    [string] $UserName = "skycms-purge-user",
    [string] $PolicyName = "SkyCMSCloudFrontPurge",
    [string] $OutputPath
)

# Ensure AWS CLI is available
$awsCmd = Get-Command aws -ErrorAction SilentlyContinue
if (-not $awsCmd) {
    Write-Error "AWS CLI not found. Install AWS CLI v2 and open a new PowerShell window: https://aws.amazon.com/cli/"
    exit 1
}

# Get AWS Account ID
try {
    $AccountId = aws sts get-caller-identity --query Account --output text
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($AccountId)) {
        throw "Unable to resolve AWS account ID. Ensure your AWS credentials are configured (aws configure)."
    }
}
catch {
    Write-Error $_
    exit 1
}

# Build least-privilege policy scoped to the specified distribution
$policyJson = @"
{
  ""Version"": ""2012-10-17"",
  ""Statement"": [
    {
      ""Effect"": ""Allow"",
      ""Action"": [""cloudfront:CreateInvalidation""],
      ""Resource"": ""arn:aws:cloudfront::${AccountId}:distribution/${DistributionId}""
    }
  ]
}
"@

# Create or verify IAM user
$userExists = $false
aws iam get-user --user-name $UserName *> $null
if ($LASTEXITCODE -eq 0) { $userExists = $true }

if (-not $userExists) {
    Write-Host "Creating IAM user '$UserName'..."
    aws iam create-user --user-name $UserName | Out-Null
    if ($LASTEXITCODE -ne 0) { Write-Error "Failed to create IAM user."; exit 1 }
} else {
    Write-Host "IAM user '$UserName' already exists."
}

# Attach/Upsert inline policy
$tempPolicy = New-TemporaryFile
try {
    Set-Content -LiteralPath $tempPolicy -Value $policyJson -Encoding UTF8
    Write-Host "Attaching inline policy '$PolicyName' scoped to distribution '$DistributionId'..."
    aws iam put-user-policy --user-name $UserName --policy-name $PolicyName --policy-document file://$tempPolicy | Out-Null
    if ($LASTEXITCODE -ne 0) { Write-Error "Failed to attach inline policy."; exit 1 }
}
finally {
    Remove-Item -LiteralPath $tempPolicy -ErrorAction SilentlyContinue
}

# Ensure the user has fewer than 2 active access keys before creating a new one
$existingKeys = aws iam list-access-keys --user-name $UserName | ConvertFrom-Json
if ($LASTEXITCODE -ne 0) { Write-Error "Failed to list access keys."; exit 1 }
if ($existingKeys.AccessKey | Measure-Object | Select-Object -ExpandProperty Count -ErrorAction SilentlyContinue -OutVariable _ | Out-Null; $_ -ge 2) {
    Write-Warning "User '$UserName' already has 2 access keys. Delete one before creating a new key."
    Write-Host "Tip: aws iam delete-access-key --user-name $UserName --access-key-id <KeyId>"
    exit 2
}

# Create access key
Write-Host "Creating new access key for '$UserName'..."
$keyResult = aws iam create-access-key --user-name $UserName | ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or -not $keyResult) { Write-Error "Failed to create access key."; exit 1 }

$creds = [PSCustomObject]@{
    UserName          = $UserName
    DistributionId    = $DistributionId
    AccessKeyId       = $keyResult.AccessKey.AccessKeyId
    SecretAccessKey   = $keyResult.AccessKey.SecretAccessKey
    Note              = "Store these securely. Secret is shown only once."
}

# Output to console
Write-Host "\n=== SkyCMS CloudFront Purge Credentials ==="
$creds | Format-List | Out-String | Write-Host

# Optionally write to file (DO NOT COMMIT THIS FILE)
if ($OutputPath) {
    Write-Host "Writing credentials to '$OutputPath' (do NOT commit)."
    $creds | ConvertTo-Json -Depth 3 | Set-Content -Path $OutputPath -Encoding UTF8
}

Write-Host "\nDone. Provide 'AccessKeyId', 'SecretAccessKey', and 'DistributionId' to SkyCMS."

Param(
    [Parameter(Mandatory = $true)] 
    [string] $BucketName,
    
    [string] $UserName = "skycms-s3-access",
    [string] $PolicyName = "SkyCMS-S3-Access",
    [string] $Region = "us-east-1",
    [string] $OutputPath
)

# Ensure AWS CLI is available
$awsCmd = Get-Command aws -ErrorAction SilentlyContinue
if (-not $awsCmd) {
    Write-Error "AWS CLI not found. Install AWS CLI v2: https://aws.amazon.com/cli/"
    exit 1
}

Write-Host "Creating IAM credentials for SkyCMS S3 access..."
Write-Host "Bucket: $BucketName"
Write-Host "Region: $Region"
Write-Host ""

# Get AWS Account ID
try {
    $AccountId = aws sts get-caller-identity --query Account --output text
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($AccountId)) {
        throw "Unable to resolve AWS account ID. Ensure AWS credentials are configured (aws configure)."
    }
}
catch {
    Write-Error $_
    exit 1
}

# Build least-privilege policy scoped to the specific bucket
$policyJson = @"
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "ListBucket",
      "Effect": "Allow",
      "Action": ["s3:ListBucket", "s3:GetBucketLocation"],
      "Resource": "arn:aws:s3:::${BucketName}"
    },
    {
      "Sid": "ReadWriteObjects",
      "Effect": "Allow",
      "Action": [
        "s3:GetObject",
        "s3:PutObject",
        "s3:DeleteObject",
        "s3:GetObjectAcl",
        "s3:PutObjectAcl"
      ],
      "Resource": "arn:aws:s3:::${BucketName}/*"
    }
  ]
}
"@

# Create or verify IAM user
$userExists = $false
aws iam get-user --user-name $UserName 2>$null | Out-Null
if ($LASTEXITCODE -eq 0) { 
    $userExists = $true 
}

if (-not $userExists) {
    Write-Host "Creating IAM user '$UserName'..."
    aws iam create-user --user-name $UserName | Out-Null
    if ($LASTEXITCODE -ne 0) { 
        Write-Error "Failed to create IAM user."
        exit 1 
    }
    Write-Host "✅ User created"
} else {
    Write-Host "✅ IAM user '$UserName' already exists"
}

# Attach/Upsert inline policy
$tempPolicy = New-TemporaryFile
try {
    Set-Content -LiteralPath $tempPolicy -Value $policyJson -Encoding UTF8
    Write-Host "Attaching inline policy '$PolicyName' scoped to bucket '$BucketName'..."
    aws iam put-user-policy --user-name $UserName --policy-name $PolicyName --policy-document file://$tempPolicy | Out-Null
    if ($LASTEXITCODE -ne 0) { 
        Write-Error "Failed to attach inline policy."
        exit 1 
    }
    Write-Host "✅ Policy attached"
}
finally {
    Remove-Item -LiteralPath $tempPolicy -ErrorAction SilentlyContinue
}

# Check existing access keys
$existingKeys = aws iam list-access-keys --user-name $UserName --query 'AccessKeyMetadata[*].AccessKeyId' --output json | ConvertFrom-Json
if ($LASTEXITCODE -ne 0) { 
    Write-Error "Failed to list access keys."
    exit 1 
}

$keyCount = if ($existingKeys) { $existingKeys.Count } else { 0 }
if ($keyCount -ge 2) {
    Write-Warning "User '$UserName' already has 2 access keys (AWS limit)."
    Write-Host ""
    Write-Host "Existing keys:"
    $existingKeys | ForEach-Object { Write-Host "  - $_" }
    Write-Host ""
    Write-Host "To create a new key, delete one first:"
    Write-Host "  aws iam delete-access-key --user-name $UserName --access-key-id <KeyId>"
    exit 2
}

# Create access key
Write-Host "Creating new access key for '$UserName'..."
$keyResult = aws iam create-access-key --user-name $UserName | ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or -not $keyResult) { 
    Write-Error "Failed to create access key."
    exit 1 
}

$creds = [PSCustomObject]@{
    UserName        = $UserName
    BucketName      = $BucketName
    Region          = $Region
    AccessKeyId     = $keyResult.AccessKey.AccessKeyId
    SecretAccessKey = $keyResult.AccessKey.SecretAccessKey
    Note            = "⚠️  Store these securely. Secret is shown only once."
}

# Output to console
Write-Host ""
Write-Host "=========================================="
Write-Host "=== SkyCMS S3 Access Credentials ==="
Write-Host "=========================================="
Write-Host ""
Write-Host "User Name:        $($creds.UserName)"
Write-Host "Bucket:           $($creds.BucketName)"
Write-Host "Region:           $($creds.Region)"
Write-Host ""
Write-Host "Access Key ID:    $($creds.AccessKeyId)"
Write-Host "Secret Access Key: $($creds.SecretAccessKey)"
Write-Host ""
Write-Host "⚠️  IMPORTANT: Store these credentials securely!"
Write-Host "   The Secret Access Key cannot be retrieved again."
Write-Host ""

# Optionally write to file (DO NOT COMMIT THIS FILE)
if ($OutputPath) {
    Write-Host "Writing credentials to '$OutputPath'"
    Write-Host "⚠️  DO NOT commit this file to source control!"
    Write-Host ""
    
    $outputContent = @"
# SkyCMS S3 Access Credentials
# Created: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
# ⚠️  DO NOT COMMIT THIS FILE TO SOURCE CONTROL!

User Name:        $($creds.UserName)
Bucket:           $($creds.BucketName)
Region:           $($creds.Region)

Access Key ID:    $($creds.AccessKeyId)
Secret Access Key: $($creds.SecretAccessKey)

---
Use these credentials in the SkyCMS Setup Wizard:
1. Navigate to your editor URL at /___setup
2. In the Storage section, select "Amazon S3"
3. Enter:
   - Bucket Name: $($creds.BucketName)
   - Region: $($creds.Region)
   - Access Key ID: $($creds.AccessKeyId)
   - Secret Access Key: $($creds.SecretAccessKey)
"@
    
    Set-Content -Path $OutputPath -Value $outputContent -Encoding UTF8
    Write-Host "✅ Credentials saved to: $OutputPath"
    Write-Host ""
}

Write-Host "Next Steps:"
Write-Host "----------"
Write-Host "1. Copy the Access Key ID and Secret Access Key above"
Write-Host "2. Navigate to your SkyCMS Setup Wizard"
Write-Host "3. In the Storage configuration section:"
Write-Host "   - Provider: Amazon S3"
Write-Host "   - Bucket: $BucketName"
Write-Host "   - Region: $Region"
Write-Host "   - Access Key ID: (paste from above)"
Write-Host "   - Secret Access Key: (paste from above)"
Write-Host ""
Write-Host "Done! 🎉"

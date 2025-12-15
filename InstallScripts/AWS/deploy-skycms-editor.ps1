Param(
  [string] $StackName = "skycms-editor-fargate",
  [string] $Region = "us-east-1",
  [string] $TemplateFile = "skycms-editor-fargate.yml",
  [string] $ContainerImage = "skycms/editor:latest",
  [Parameter(Mandatory = $true)] [string] $StaticSiteBucketName,
  [string] $DBInstanceClass = "db.t4g.micro",
  [int] $DBAllocatedStorage = 20,
  [string] $DBName = "skycms",
  [string] $DBUsername = "skycms_admin",
  [string] $DBPassword,
  [string] $TaskCPU = "512",
  [string] $TaskMemory = "1024",
  [int] $DesiredCount = 1,
  [int] $ContainerPort = 80
)

# Requires AWS CLI v2
$awsCmd = Get-Command aws -ErrorAction SilentlyContinue
if (-not $awsCmd) {
  Write-Error "AWS CLI not found. Install AWS CLI v2: https://aws.amazon.com/cli/"
  exit 1
}

# If the stack exists and is in ROLLBACK_COMPLETE, prompt to delete before redeploying
try {
  $existingStatus = aws cloudformation describe-stacks `
    --stack-name $StackName `
    --region $Region `
    --query "Stacks[0].StackStatus" `
    --output text 2>$null

  if ($LASTEXITCODE -eq 0 -and $existingStatus -eq 'ROLLBACK_COMPLETE') {
    Write-Warning "Stack '$StackName' is in ROLLBACK_COMPLETE and cannot be updated."
    $ans = Read-Host "Delete and recreate the stack? (y/N)"
    if ($ans -match '^(y|yes)$') {
      Write-Host "Deleting stack '$StackName'..." -ForegroundColor Yellow
      aws cloudformation delete-stack --stack-name $StackName --region $Region
      if ($LASTEXITCODE -ne 0) { Write-Error "Failed to initiate stack deletion."; exit 1 }
      Write-Host "Waiting for stack deletion to complete..." -ForegroundColor Yellow
      aws cloudformation wait stack-delete-complete --stack-name $StackName --region $Region
      if ($LASTEXITCODE -ne 0) { Write-Error "Stack deletion did not complete successfully."; exit 1 }
      Write-Host "Stack deleted. Proceeding with fresh deployment..." -ForegroundColor Green
    } else {
      Write-Error "Cannot proceed while stack is ROLLBACK_COMPLETE. Please delete it and retry."
      exit 1
    }
  }
} catch { }

# Resolve template path relative to this script if not absolute, and validate it exists
if (-not [System.IO.Path]::IsPathRooted($TemplateFile)) {
  $TemplateFile = Join-Path $PSScriptRoot $TemplateFile
}
if (-not (Test-Path $TemplateFile)) {
  Write-Error "Invalid template path $TemplateFile"
  exit 1
}

# Prompt for DB password if not provided
if (-not $DBPassword) {
  $sec = Read-Host -AsSecureString -Prompt "Enter MySQL master password (min 8 characters)"
  $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($sec)
  try { $DBPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto($bstr) } finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr) }
  
  if ($DBPassword.Length -lt 8) {
    Write-Error "Password must be at least 8 characters."
    exit 1
  }
}

# Build parameter overrides as separate arguments so AWS CLI parses them correctly
$paramOverrides = @(
  "ContainerImage=$ContainerImage",
  "StaticSiteBucketName=$StaticSiteBucketName",
  "DBInstanceClass=$DBInstanceClass",
  "DBAllocatedStorage=$DBAllocatedStorage",
  "DBName=$DBName",
  "DBUsername=$DBUsername",
  "DBPassword=$DBPassword",
  "TaskCPU=$TaskCPU",
  "TaskMemory=$TaskMemory",
  "DesiredCount=$DesiredCount",
  "ContainerPort=$ContainerPort"
)

Write-Host "Deploying SkyCMS Editor stack '$StackName' in $Region..."
Write-Host "Configuration:"
Write-Host "  - Static Site Bucket: $StaticSiteBucketName"
Write-Host "  - Database: $DBName (will be pre-created)"
Write-Host "  - Database User: $DBUsername"
Write-Host "  - Fargate Task: $TaskCPU CPU / $TaskMemory MB RAM"
Write-Host ""
Write-Host "This will create: VPC, ECS Fargate cluster, RDS MySQL, ALB, Lambda initialization, and security groups."
Write-Host "⏱️  Deployment may take 15-20 minutes (RDS creation is slow)."
Write-Host "🔄 After completion, the setup wizard will be available at the EditorURL output."
Write-Host ""

aws cloudformation deploy `
  --template-file $TemplateFile `
  --stack-name $StackName `
  --region $Region `
  --capabilities CAPABILITY_IAM `
  --parameter-overrides $paramOverrides

if ($LASTEXITCODE -ne 0) { Write-Error "Deployment failed."; exit 1 }

Write-Host "`nFetching stack outputs..."
$outputs = aws cloudformation describe-stacks `
  --stack-name $StackName `
  --region $Region `
  --query "Stacks[0].Outputs[*].{Key:OutputKey,Value:OutputValue}" `
  --output json | ConvertFrom-Json

Write-Host "`n=========================================="
Write-Host "=== SkyCMS Editor Deployment Complete ==="
Write-Host "=========================================="
Write-Host ""

# Display key outputs
$editorUrl = ($outputs | Where-Object { $_.Key -eq 'EditorURL' }).Value
$setupUrl = ($outputs | Where-Object { $_.Key -eq 'SetupWizardURL' }).Value
$dbEndpoint = ($outputs | Where-Object { $_.Key -eq 'DatabaseEndpoint' }).Value
$bucket = ($outputs | Where-Object { $_.Key -eq 'StorageBucketName' }).Value

Write-Host "Editor URL: $editorUrl"
Write-Host "Setup Wizard: $setupUrl"
Write-Host ""
Write-Host "Database:"
Write-Host "  Endpoint: $dbEndpoint"
Write-Host "  Database: $DBName (pre-created)"
Write-Host "  Username: $DBUsername"
Write-Host ""
Write-Host "Storage:"
Write-Host "  Bucket: $bucket"
Write-Host "  Region: $Region"
Write-Host ""
Write-Host "⏱️  Next Steps:"
Write-Host "  1. Wait 2-3 minutes for ECS tasks to fully start"
Write-Host "  2. Visit the Setup Wizard URL above"
Write-Host "  3. Configure S3 credentials, admin account, and publisher settings"
Write-Host ""

Write-Host "📚 Documentation:"
Write-Host "  See $(Join-Path $PSScriptRoot 'PHASE1_SETUP_WIZARD.md') for detailed setup instructions"
Write-Host ""
Write-Host "🔑 Create S3 Access Credentials (required for setup wizard):"
Write-Host "  & \"$(Join-Path $PSScriptRoot 'create-s3-access-keys.ps1')\" -BucketName $bucket"
Write-Host ""

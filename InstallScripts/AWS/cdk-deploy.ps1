param(
  [string]$Region = "us-east-1",
  [string]$Image = "toiyabe/sky-editor:latest",
  [int]$DesiredCount = 1,
  [string]$DbName = "skycms"
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$cdkDir = Join-Path $PSScriptRoot 'cdk'

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SkyCMS Editor CDK Deployment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Parameters:" -ForegroundColor Green
Write-Host "  Region:       $Region"
Write-Host "  Image:        $Image"
Write-Host "  DesiredCount: $DesiredCount"
Write-Host "  DbName:       $DbName"
Write-Host ""
Write-Host "STACK: VPC + ECS + RDS MySQL with TLS" -ForegroundColor Magenta
Write-Host "No ALB, No CloudFront - testing database connection" -ForegroundColor Magenta
Write-Host ""

if (-not (Test-Path (Join-Path $cdkDir 'package.json'))) {
  throw "CDK project not found at $cdkDir"
}

Push-Location $cdkDir
try {
  Write-Host "Step 1: Installing dependencies (npm install) ..." -ForegroundColor Yellow
  npm install
  if ($LASTEXITCODE -ne 0) { throw "npm install failed" }

  # Get AWS account ID
  $accountId = aws sts get-caller-identity --query Account --output text
  if ($LASTEXITCODE -ne 0) { throw "Failed to get AWS account ID" }
  Write-Host "AWS Account: $accountId" -ForegroundColor Gray

  # Use node to invoke cdk CLI directly
  $cdkBin = Join-Path $cdkDir "node_modules\aws-cdk\bin\cdk"
  
  Write-Host ""
  Write-Host "Step 2: Bootstrapping CDK (if needed) ..." -ForegroundColor Yellow
  node $cdkBin bootstrap "aws://$accountId/$Region" --context image=$Image --context desiredCount=$DesiredCount --context dbName=$DbName
  if ($LASTEXITCODE -ne 0) { throw "cdk bootstrap failed" }

  Write-Host ""
  Write-Host "Step 3: Synthesizing CloudFormation template ..." -ForegroundColor Yellow
  node $cdkBin synth --context image=$Image --context desiredCount=$DesiredCount --context dbName=$DbName
  if ($LASTEXITCODE -ne 0) { throw "cdk synth failed" }

  Write-Host ""
  Write-Host "Step 4: Deploying SkyCmsMinimalStack (ECS + RDS) ..." -ForegroundColor Yellow
  Write-Host "This will take 5-10 minutes for VPC + ECS + RDS MySQL." -ForegroundColor Gray
  Write-Host ""
  node $cdkBin deploy SkyCmsMinimalStack --require-approval never --context image=$Image --context desiredCount=$DesiredCount --context dbName=$DbName
  if ($LASTEXITCODE -ne 0) { throw "cdk deploy failed" }

  Write-Host ""
  Write-Host "========================================" -ForegroundColor Green
  Write-Host "✅ SkyCmsMinimalStack Deployed!" -ForegroundColor Green
  Write-Host "========================================" -ForegroundColor Green
  Write-Host ""
  Write-Host "Stack Outputs (Cluster, Service, Database, LogGroup) are shown above." -ForegroundColor Cyan
  Write-Host ""
  Write-Host "To get the task public IP and test the container:" -ForegroundColor Yellow
  Write-Host ""
  Write-Host "⏳ Extraction CloudFront URL from outputs..." -ForegroundColor Yellow
  $stackOutputs = aws cloudformation describe-stacks --stack-name SkyCmsMinimalStack --region $Region --query "Stacks[0].Outputs" --output json | ConvertFrom-Json
  $cloudFrontUrl = ($stackOutputs | Where-Object { $_.OutputKey -eq "CloudFrontURL" }).OutputValue
  $dbEndpoint     = ($stackOutputs | Where-Object { $_.OutputKey -eq "DatabaseEndpoint" }).OutputValue
  $dbNameOut      = ($stackOutputs | Where-Object { $_.OutputKey -eq "DatabaseName" }).OutputValue
  $dbSecretArn    = ($stackOutputs | Where-Object { $_.OutputKey -eq "DatabaseCredentialsSecret" }).OutputValue
  $dbSgId         = ($stackOutputs | Where-Object { $_.OutputKey -eq "DbSecurityGroupId" }).OutputValue
  $mysqlConnOut   = ($stackOutputs | Where-Object { $_.OutputKey -eq "MySqlConnectionString" }).OutputValue
  
  if ($cloudFrontUrl) {
    Write-Host ""
    Write-Host "✅ DEPLOYMENT COMPLETE!" -ForegroundColor Green
    Write-Host ""
    Write-Host "🌐 SkyCMS Editor URL:" -ForegroundColor Cyan
    Write-Host "   $cloudFrontUrl" -ForegroundColor Green
    Write-Host ""
    Write-Host "📝 Note: CloudFront may take 1-2 minutes to propagate. TLS certificate is auto-generated." -ForegroundColor Gray
    Write-Host ""
  } else {
    Write-Host "⚠️  CloudFront URL not found in outputs. Check the stack manually." -ForegroundColor Yellow
  }

  # Echo MySQL connection string for validation and MySQL Workbench
  try {
    Write-Host ""; Write-Host "🔐 Retrieving database credentials from Secrets Manager..." -ForegroundColor Yellow
    $secretVal = aws secretsmanager get-secret-value --secret-id $dbSecretArn --region $Region --query SecretString --output text
    $secretObj = $secretVal | ConvertFrom-Json
    $dbUser = $secretObj.username
    $dbPwd  = $secretObj.password
    $mysqlConn = "Server=$dbEndpoint;Port=3306;Uid=$dbUser;Pwd=$dbPwd;Database=$dbNameOut;"
    $maskedConn = "Server=$dbEndpoint;Port=3306;Uid=$dbUser;Pwd=[REDACTED];Database=$dbNameOut;"
    Write-Host ""; Write-Host "✅ MySQL Connection String (for validation):" -ForegroundColor Green
    Write-Host "   $maskedConn" -ForegroundColor Cyan
    Write-Host ""; Write-Host "📝 Full connection string with unmasked password:" -ForegroundColor Yellow
    Write-Host "   (Copy from AWS Secrets Manager or use the script's stored value)" -ForegroundColor Gray
  } catch {
    if ($mysqlConnOut) {
      Write-Host ""; Write-Host "✅ MySQL Connection String (from stack output):" -ForegroundColor Green
      Write-Host "   $mysqlConnOut" -ForegroundColor Cyan
    } else {
      Write-Host "⚠️  Could not retrieve DB credentials; skipping echo of connection string." -ForegroundColor Yellow
    }
  }

  # Allow-list current machine's public IP for MySQL (tcp/3306)
  try {
    Write-Host ""; Write-Host "🌍 Detecting your public IP for RDS allow-list..." -ForegroundColor Yellow
    $myIp = (Invoke-WebRequest -Uri 'https://checkip.amazonaws.com' -UseBasicParsing).Content.Trim()
    if ($myIp -and $dbSgId) {
      Write-Host "Adding ingress rule to $dbSgId for $myIp/32 on 3306..." -ForegroundColor Yellow
      aws ec2 authorize-security-group-ingress --group-id $dbSgId --protocol tcp --port 3306 --cidr "$myIp/32" --region $Region | Out-Null
      Write-Host "✅ RDS MySQL now accessible from this computer ($myIp)." -ForegroundColor Green
    } else {
      Write-Host "⚠️  Missing Security Group ID or public IP; skipped allow-listing." -ForegroundColor Yellow
    }
  } catch {
    Write-Host "⚠️  Failed to add ingress rule: $_" -ForegroundColor Yellow
  }
}
catch {
  Write-Host ""
  Write-Host "❌ Deployment failed: $_" -ForegroundColor Red
  exit 1
}
finally {
  Pop-Location
}

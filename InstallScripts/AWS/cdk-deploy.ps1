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
  Write-Host "`$cluster = 'ClusterName from outputs above'" -ForegroundColor Gray
  Write-Host "`$region = '$Region'" -ForegroundColor Gray
  Write-Host "`$taskArn = aws ecs list-tasks --cluster `$cluster --region `$region --query 'taskArns[0]' -o text" -ForegroundColor Gray
  Write-Host "`$eni = aws ecs describe-tasks --cluster `$cluster --tasks `$taskArn --region `$region --query 'tasks[0].attachments[0].details[?name==``networkInterfaceId``].value' -o text" -ForegroundColor Gray
  Write-Host "aws ec2 describe-network-interfaces --network-interface-ids `$eni --region `$region --query 'NetworkInterfaces[0].Association.PublicIp' -o text" -ForegroundColor Gray
  Write-Host ""
  Write-Host "Then visit: http://<PUBLIC-IP>" -ForegroundColor Yellow
  Write-Host ""
}
catch {
  Write-Host ""
  Write-Host "❌ Deployment failed: $_" -ForegroundColor Red
  exit 1
}
finally {
  Pop-Location
}

$ErrorActionPreference = 'Stop'

# ============================================
# INTERACTIVE PROMPT HELPER FUNCTIONS
# ============================================

function Prompt-WithDefault {
  param(
    [string]$Prompt,
    [string]$Default
  )
  if ($Default) {
    $response = Read-Host "$Prompt [$Default]"
    if ([string]::IsNullOrWhiteSpace($response)) { return $Default }
    return $response
  } else {
    $response = Read-Host $Prompt
    return $response
  }
}

function Prompt-YesNo {
  param(
    [string]$Prompt,
    [bool]$Default = $false
  )
  $defaultChar = if ($Default) { "Y" } else { "N" }
  $response = Read-Host "$Prompt (y/n) [$defaultChar]"
  if ([string]::IsNullOrWhiteSpace($response)) { return $Default }
  return $response -match '^[Yy]'
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$cdkDir = Join-Path $PSScriptRoot 'cdk'

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SkyCMS Editor CDK Deployment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "This script will deploy SkyCMS Editor (and optionally Publisher) to AWS." -ForegroundColor Gray
Write-Host "Press Enter to accept default values shown in [brackets]." -ForegroundColor Gray
Write-Host ""

# ============================================
# COLLECT INTERACTIVE PARAMETERS
# ============================================

Write-Host "--- Editor Configuration ---" -ForegroundColor Cyan
$Region = Prompt-WithDefault "AWS Region" "us-east-1"
$Image = Prompt-WithDefault "Docker Image" "toiyabe/sky-editor:latest"
$DesiredCount = [int](Prompt-WithDefault "Desired ECS Task Count" "1")
$DbName = Prompt-WithDefault "Database Name" "skycms"
$StackName = Prompt-WithDefault "CDK Stack Name" "SkyCMS-Stack"

Write-Host ""
Write-Host "--- Publisher (S3 + CloudFront) ---" -ForegroundColor Cyan
$DeployPublisher = Prompt-YesNo "Deploy Publisher (S3 + CloudFront)?" $true

$PublisherDomainName = ""
$PublisherHostedZoneId = ""
$PublisherHostedZoneName = ""
$StorageSecretArn = ""

if ($DeployPublisher) {
  Write-Host ""
  Write-Host "Optional: Configure custom domain for Publisher CloudFront" -ForegroundColor Gray
  $UsePublisherCustomDomain = Prompt-YesNo "Use custom domain for Publisher?" $false
  if ($UsePublisherCustomDomain) {
    $PublisherDomainName = Prompt-WithDefault "Publisher Domain Name (e.g., www.example.com)" ""
    $PublisherHostedZoneId = Prompt-WithDefault "Route 53 Hosted Zone ID" ""
    $PublisherHostedZoneName = Prompt-WithDefault "Route 53 Hosted Zone Name (e.g., example.com)" ""
  }
}

Write-Host ""
Write-Host "--- CDN Caching Options ---" -ForegroundColor Cyan
$EditorCacheEnabled = $false  # Editor should never cache (dynamic app)
$PublisherCacheEnabled = $true  # Publisher should cache (static site)

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Configuration Summary" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Editor Stack Configuration:" -ForegroundColor Cyan
Write-Host "  Region:       $Region"
Write-Host "  Image:        $Image"
Write-Host "  DesiredCount: $DesiredCount"
Write-Host "  DbName:       $DbName"
Write-Host "  StackName:    $StackName"
Write-Host "  Editor Cache: $EditorCacheEnabled"
Write-Host ""
if ($DeployPublisher) {
  Write-Host "Publisher Configuration:" -ForegroundColor Cyan
  if ($PublisherDomainName) {
    Write-Host "  Domain:       $PublisherDomainName"
  }
  Write-Host "  Publisher Cache: $PublisherCacheEnabled"
  Write-Host ""
}
Write-Host "Ready to deploy?" -ForegroundColor Yellow
$Confirm = Prompt-YesNo "Proceed?" $true
if (-not $Confirm) {
  Write-Host "Deployment cancelled by user." -ForegroundColor Yellow
  exit 0
}
Write-Host ""

Push-Location $cdkDir
try {
  Write-Host "Installing dependencies (npm install)..." -ForegroundColor Yellow
  npm install
  if ($LASTEXITCODE -ne 0) { throw "npm install failed" }

  # Get AWS account ID
  $accountId = aws sts get-caller-identity --query Account --output text
  if ($LASTEXITCODE -ne 0) { throw "Failed to get AWS account ID" }
  Write-Host "AWS Account: $accountId" -ForegroundColor Gray

  # Use node to invoke cdk CLI directly
  $cdkBin = Join-Path $cdkDir "node_modules\aws-cdk\bin\cdk"
  
  # ============================================
  # DEPLOY UNIFIED STACK (PUBLISHER + EDITOR)
  # ============================================
  
  Write-Host ""
  Write-Host "========================================" -ForegroundColor Cyan
  Write-Host "DEPLOYING SkyCMS STACK" -ForegroundColor Cyan
  if ($DeployPublisher) {
    Write-Host "(Publisher: S3 + CloudFront | Editor: ECS + RDS + CloudFront)" -ForegroundColor Cyan
  } else {
    Write-Host "(Editor Only: ECS + RDS + CloudFront)" -ForegroundColor Cyan
  }
  Write-Host "========================================" -ForegroundColor Cyan
  Write-Host ""
  Write-Host "Bootstrapping CDK (if needed)..." -ForegroundColor Yellow
  $bootstrapCtx = @("--context", "image=$Image", "--context", "desiredCount=$DesiredCount", "--context", "dbName=$DbName", "--context", "stackName=$StackName", "--context", "editorCacheEnabled=$EditorCacheEnabled", "--context", "publisherCacheEnabled=$PublisherCacheEnabled")
  if ($CertificateArn) { $bootstrapCtx += @("--context", "certificateArn=$CertificateArn") }
  if ($DomainName) { $bootstrapCtx += @("--context", "domainName=$DomainName") }
  if ($HostedZoneId) { $bootstrapCtx += @("--context", "hostedZoneId=$HostedZoneId") }
  if ($HostedZoneName) { $bootstrapCtx += @("--context", "hostedZoneName=$HostedZoneName") }
  if ($DeployPublisher) { 
    $bootstrapCtx += @("--context", "deployPublisher=true") 
    if ($PublisherDomainName) { $bootstrapCtx += @("--context", "publisherDomainName=$PublisherDomainName") }
    if ($PublisherCertificateArn) { $bootstrapCtx += @("--context", "publisherCertificateArn=$PublisherCertificateArn") }
  }
  node $cdkBin bootstrap "aws://$accountId/$Region" $bootstrapCtx
  if ($LASTEXITCODE -ne 0) { throw "cdk bootstrap failed" }

  Write-Host ""
  Write-Host "Synthesizing CloudFormation template..." -ForegroundColor Yellow
  $synthCtx = @("--context", "image=$Image", "--context", "desiredCount=$DesiredCount", "--context", "dbName=$DbName", "--context", "stackName=$StackName", "--context", "editorCacheEnabled=$EditorCacheEnabled", "--context", "publisherCacheEnabled=$PublisherCacheEnabled")
  if ($CertificateArn) { $synthCtx += @("--context", "certificateArn=$CertificateArn") }
  if ($DomainName) { $synthCtx += @("--context", "domainName=$DomainName") }
  if ($HostedZoneId) { $synthCtx += @("--context", "hostedZoneId=$HostedZoneId") }
  if ($HostedZoneName) { $synthCtx += @("--context", "hostedZoneName=$HostedZoneName") }
  if ($DeployPublisher) { 
    $synthCtx += @("--context", "deployPublisher=true") 
    if ($PublisherDomainName) { $synthCtx += @("--context", "publisherDomainName=$PublisherDomainName") }
    if ($PublisherCertificateArn) { $synthCtx += @("--context", "publisherCertificateArn=$PublisherCertificateArn") }
  }
  node $cdkBin synth $synthCtx
  if ($LASTEXITCODE -ne 0) { throw "cdk synth failed" }

  Write-Host ""
  Write-Host "Deploying $StackName (ECS + RDS + CloudFront)..." -ForegroundColor Yellow
  Write-Host "This will take 5-10 minutes for VPC + ECS + RDS MySQL." -ForegroundColor Gray
  Write-Host ""
  $deployCtx = @("--require-approval", "never", "--context", "image=$Image", "--context", "desiredCount=$DesiredCount", "--context", "dbName=$DbName", "--context", "stackName=$StackName", "--context", "editorCacheEnabled=$EditorCacheEnabled", "--context", "publisherCacheEnabled=$PublisherCacheEnabled")
  if ($DeployPublisher) { 
    $deployCtx += @("--context", "deployPublisher=true") 
    if ($PublisherDomainName) { $deployCtx += @("--context", "publisherDomainName=$PublisherDomainName") }
    if ($PublisherCertificateArn) { $deployCtx += @("--context", "publisherCertificateArn=$PublisherCertificateArn") }
  }
  if ($DomainName) { $deployCtx += @("--context", "domainName=$DomainName") }
  if ($HostedZoneId) { $deployCtx += @("--context", "hostedZoneId=$HostedZoneId") }
  if ($HostedZoneName) { $deployCtx += @("--context", "hostedZoneName=$HostedZoneName") }
  node $cdkBin deploy $StackName $deployCtx
  if ($LASTEXITCODE -ne 0) { throw "cdk deploy failed" }

  # Get stack outputs from unified CDK stack
  $stackOutputs = aws cloudformation describe-stacks --stack-name $StackName --region $Region --query "Stacks[0].Outputs" --output json | ConvertFrom-Json
  $cloudFrontUrl = ($stackOutputs | Where-Object { $_.OutputKey -eq "CloudFrontURL" }).OutputValue
  $dbEndpoint    = ($stackOutputs | Where-Object { $_.OutputKey -eq "DatabaseEndpoint" }).OutputValue
  $dbNameOut     = ($stackOutputs | Where-Object { $_.OutputKey -eq "DatabaseName" }).OutputValue
  $dbSecretArn   = ($stackOutputs | Where-Object { $_.OutputKey -eq "DatabaseCredentialsSecret" }).OutputValue
  $dbSgId        = ($stackOutputs | Where-Object { $_.OutputKey -eq "DbSecurityGroupId" }).OutputValue
  $mysqlConnOut  = ($stackOutputs | Where-Object { $_.OutputKey -eq "MySqlConnectionString" }).OutputValue
  
  # Publisher outputs (if deployed)
  if ($DeployPublisher) {
    $publisherBucket = ($stackOutputs | Where-Object { $_.OutputKey -eq "PublisherBucketName" }).OutputValue
    $publisherUrl    = ($stackOutputs | Where-Object { $_.OutputKey -eq "PublisherCloudFrontURL" }).OutputValue
    $StorageSecretArn = ($stackOutputs | Where-Object { $_.OutputKey -eq "StorageSecretArn" }).OutputValue
  }

  Write-Host ""
  Write-Host "========================================" -ForegroundColor Green
  Write-Host "✅ DEPLOYMENT COMPLETE!" -ForegroundColor Green
  Write-Host "========================================" -ForegroundColor Green
  Write-Host ""
  
  if ($DeployPublisher) {
    Write-Host "📦 PUBLISHER (S3 + CloudFront):" -ForegroundColor Cyan
    Write-Host "   S3 Bucket: $publisherBucket" -ForegroundColor White
    Write-Host "   CloudFront URL: $publisherUrl" -ForegroundColor Green
    if ($PublisherDomainName) {
      Write-Host "   Custom Domain: https://$PublisherDomainName" -ForegroundColor White
    }
    Write-Host ""
    Write-Host "   📝 To upload website files:" -ForegroundColor Yellow
    Write-Host "   aws s3 sync ./website s3://$publisherBucket/" -ForegroundColor Gray
    Write-Host ""
  }
  
  Write-Host "📝 EDITOR (ECS + RDS + CloudFront):" -ForegroundColor Cyan
  Write-Host "   Stack Name: $StackName" -ForegroundColor White
  Write-Host "   CloudFront URL: $cloudFrontUrl" -ForegroundColor Green
  if ($DomainName) {
    Write-Host "   Custom Domain: https://$DomainName" -ForegroundColor White
  }
  Write-Host "   Database: $dbNameOut @ $dbEndpoint" -ForegroundColor White
  if ($StorageSecretArn) {
    Write-Host "   Storage Secret: $StorageSecretArn" -ForegroundColor White
  }
  if (-not $cloudFrontUrl) {
    Write-Host "⚠️  CloudFront URL not found in outputs. Check the stack manually." -ForegroundColor Yellow
  }
  Write-Host ""
  Write-Host "⏳ CloudFront may take 1-2 minutes to fully propagate. TLS certificate is auto-generated." -ForegroundColor Gray
  Write-Host ""

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
    if ($_.Exception.Message -like '*InvalidPermission.Duplicate*') {
      Write-Host "✅ Security group rule already exists for $myIp/32 on 3306." -ForegroundColor Gray
    } else {
      Write-Host "⚠️  Failed to add ingress rule: $_" -ForegroundColor Yellow
    }
  }

  # ============================================
  # FINAL DEPLOYMENT SUMMARY
  # ============================================
  
  Write-Host ""
  Write-Host "" 
  Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
  Write-Host "         🎉 DEPLOYMENT SUMMARY - SkyCMS Environment         " -ForegroundColor Cyan
  Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
  
  Write-Host ""
  Write-Host "📍 Region: $Region" -ForegroundColor White
  Write-Host ""
  
  if ($DeployPublisher) {
    Write-Host "📦 PUBLISHER (Static Website Hosting)" -ForegroundColor Yellow
    Write-Host "  S3 Bucket Name:        $publisherBucket" -ForegroundColor White
    Write-Host "  CloudFront DNS:        $publisherUrl" -ForegroundColor Green
    if ($PublisherDomainName) {
      Write-Host "  Custom Domain:         https://$PublisherDomainName" -ForegroundColor White
    }
    Write-Host "  IAM User:              skycms-s3-publisher-user-$StackName" -ForegroundColor White
    Write-Host ""
    Write-Host "  📝 To deploy static files:" -ForegroundColor Gray
    Write-Host "     aws s3 sync ./public s3://$publisherBucket/ --region $Region" -ForegroundColor Gray
    Write-Host ""
  }
  
  Write-Host "🖥️  EDITOR (CMS Application)" -ForegroundColor Yellow
  Write-Host "  CloudFront DNS:        $cloudFrontUrl" -ForegroundColor Green
  if ($DomainName) {
    Write-Host "  Custom Domain:         https://$DomainName" -ForegroundColor White
  }
  Write-Host "  ALB DNS (debug):       $(($stackOutputs | Where-Object { $_.OutputKey -eq 'ALBDomainName' }).OutputValue)" -ForegroundColor Gray
  Write-Host ""
  
  Write-Host "🗄️  DATABASE (MySQL)" -ForegroundColor Yellow
  Write-Host "  RDS Endpoint:          $dbEndpoint" -ForegroundColor White
  Write-Host "  Database Name:         $dbNameOut" -ForegroundColor White
  Write-Host "  Admin Username:        admin" -ForegroundColor White
  Write-Host "  Credentials Secret:    $dbSecretArn" -ForegroundColor Gray
  Write-Host ""
  
  if ($DeployPublisher) {
    Write-Host "🔐 STORAGE" -ForegroundColor Yellow
    Write-Host "  Storage Secret:        $StorageSecretArn" -ForegroundColor Gray
    Write-Host ""
  }
  
  Write-Host "📋 ENVIRONMENT INFO" -ForegroundColor Yellow
  Write-Host "  Stack Name:            $StackName" -ForegroundColor White
  Write-Host "  CDK Docker Image:      $Image" -ForegroundColor White
  Write-Host "  ECS Task Count:        $DesiredCount" -ForegroundColor White
  Write-Host "  ECS Cluster:           $(($stackOutputs | Where-Object { $_.OutputKey -eq 'ClusterName' }).OutputValue)" -ForegroundColor White
  Write-Host "  ECS Service:           $(($stackOutputs | Where-Object { $_.OutputKey -eq 'ServiceName' }).OutputValue)" -ForegroundColor White
  Write-Host "  CloudWatch Log Group:  $(($stackOutputs | Where-Object { $_.OutputKey -eq 'LogGroupName' }).OutputValue)" -ForegroundColor White
  Write-Host ""
  
  Write-Host "🔒 SECURITY & NETWORKING" -ForegroundColor Yellow
  Write-Host "  DB Security Group:     $(($stackOutputs | Where-Object { $_.OutputKey -eq 'DbSecurityGroupId' }).OutputValue)" -ForegroundColor Gray
  Write-Host "  Your Public IP:        $myIp (allowed for MySQL access)" -ForegroundColor Gray
  Write-Host ""
  
  Write-Host "🚀 NEXT STEPS" -ForegroundColor Cyan
  Write-Host "  1. Open the Editor at: $cloudFrontUrl" -ForegroundColor White
  if ($DeployPublisher) {
    Write-Host "  2. Upload your website files to S3 bucket: $publisherBucket" -ForegroundColor White
  }
  Write-Host "  3. Configure your MySQL client with:" -ForegroundColor White
  Write-Host "     Host: $dbEndpoint" -ForegroundColor Gray
  Write-Host "     User: admin" -ForegroundColor Gray
  Write-Host "     Password: [retrieve from AWS Secrets Manager]" -ForegroundColor Gray
  Write-Host "     Database: $dbNameOut" -ForegroundColor Gray
  Write-Host ""
  
  Write-Host "ℹ️  USEFUL COMMANDS" -ForegroundColor Cyan
  Write-Host ""
  
  Write-Host "  📦 PUBLISHER (S3 + CloudFront):" -ForegroundColor White
  Write-Host "    List S3 bucket contents:" -ForegroundColor Gray
  Write-Host "      aws s3 ls s3://$publisherBucket --recursive --region $Region" -ForegroundColor Gray
  Write-Host "    Upload static files to S3:" -ForegroundColor Gray
  Write-Host "      aws s3 sync ./public s3://$publisherBucket/ --region $Region" -ForegroundColor Gray
  Write-Host "    Invalidate CloudFront cache (after update):" -ForegroundColor Gray
  Write-Host "      aws cloudfront create-invalidation --distribution-id <DIST_ID> --paths '/*' --region $Region" -ForegroundColor Gray
  Write-Host "    Get CloudFront distribution ID:" -ForegroundColor Gray
  Write-Host "      aws cloudfront list-distributions --query \"DistributionList.Items[?DomainName=='$publisherUrl'].Id\" --output text" -ForegroundColor Gray
  Write-Host ""
  
  Write-Host "  🖥️  EDITOR (ECS + CloudFront + ALB):" -ForegroundColor White
  Write-Host "    List running ECS tasks:" -ForegroundColor Gray
  Write-Host "      aws ecs list-tasks --cluster $StackName --region $Region" -ForegroundColor Gray
  Write-Host "    View ECS task logs (realtime):" -ForegroundColor Gray
  Write-Host "      aws logs tail /ecs/$StackName/web --follow --region $Region" -ForegroundColor Gray
  Write-Host "    Get ECS task details:" -ForegroundColor Gray
  Write-Host "      aws ecs describe-tasks --cluster $StackName --tasks <TASK_ARN> --region $Region" -ForegroundColor Gray
  Write-Host "    View ALB health status:" -ForegroundColor Gray
  Write-Host "      aws elbv2 describe-target-health --target-group-arn <TG_ARN> --region $Region" -ForegroundColor Gray
  Write-Host ""
  
  Write-Host "  🗄️  DATABASE (RDS MySQL):" -ForegroundColor White
  Write-Host "    Get database credentials:" -ForegroundColor Gray
  Write-Host "      aws secretsmanager get-secret-value --secret-id $dbSecretArn --region $Region --query SecretString --output text | jq '.password'" -ForegroundColor Gray
  Write-Host "    Connect via MySQL CLI (requires mysql client):" -ForegroundColor Gray
  Write-Host "      mysql -h $dbEndpoint -u admin -p -D $dbNameOut" -ForegroundColor Gray
  Write-Host "    View RDS instance details:" -ForegroundColor Gray
  Write-Host "      aws rds describe-db-instances --db-instance-identifier <DB_NAME> --region $Region" -ForegroundColor Gray
  Write-Host "    Check database parameter group:" -ForegroundColor Gray
  Write-Host "      aws rds describe-db-parameters --db-parameter-group-name <PG_NAME> --region $Region" -ForegroundColor Gray
  Write-Host ""
  
  Write-Host "  🔍 GENERAL STACK MANAGEMENT:" -ForegroundColor White
  Write-Host "    View all stack outputs:" -ForegroundColor Gray
  Write-Host "      aws cloudformation describe-stacks --stack-name $StackName --region $Region --query 'Stacks[0].Outputs' --output table" -ForegroundColor Gray
  Write-Host "    Monitor stack creation/update events:" -ForegroundColor Gray
  Write-Host "      aws cloudformation describe-stack-events --stack-name $StackName --region $Region --query 'StackEvents[0:10]' --output table" -ForegroundColor Gray
  Write-Host "    Delete entire stack (when ready to cleanup):" -ForegroundColor Gray
  Write-Host "      aws cloudformation delete-stack --stack-name $StackName --region $Region" -ForegroundColor Gray
  Write-Host ""
  
  Write-Host "⏱️  Note: CloudFront may take 1-2 minutes to fully propagate." -ForegroundColor Gray
  Write-Host "         TLS certificate is auto-generated and applied." -ForegroundColor Gray
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

Param(
  [string] $EditorStackName = "skycms-editor-fargate",
  [string] $StaticSiteStackName = "skycms-static-site",
  [string] $Region = "us-east-1",
  [switch] $Force,
  [switch] $EmptyBucket,
  [switch] $DeleteAllIAMUsers
)

# Requires AWS CLI v2
$awsCmd = Get-Command aws -ErrorAction SilentlyContinue
if (-not $awsCmd) {
  Write-Error "AWS CLI not found. Install AWS CLI v2: https://aws.amazon.com/cli/"
  exit 1
}

Write-Host "=========================================="
Write-Host "=== SkyCMS Complete Infrastructure Deletion ==="
Write-Host "=========================================="
Write-Host ""
Write-Host "This will DELETE ALL SkyCMS resources:"
Write-Host ""
Write-Host "Editor Stack ($EditorStackName):"
Write-Host "  • ECS Fargate cluster and tasks"
Write-Host "  • RDS MySQL database (ALL DATA)"
Write-Host "  • Application Load Balancer"
Write-Host "  • CloudFront distribution"
Write-Host "  • VPC and networking"
Write-Host ""
Write-Host "Static Site Stack ($StaticSiteStackName):"
Write-Host "  • S3 bucket (ALL WEBSITE FILES)"
Write-Host "  • CloudFront distribution(s)"
Write-Host "  • Route 53 DNS records"
Write-Host ""

if ($DeleteAllIAMUsers) {
  Write-Host "IAM Users:"
  Write-Host "  • skycms-s3-access"
  Write-Host "  • skycms-purge-user"
  Write-Host ""
}

if (-not $Force) {
  Write-Host "⚠️⚠️⚠️  EXTREME WARNING  ⚠️⚠️⚠️"
  Write-Host ""
  Write-Host "This will PERMANENTLY DELETE:"
  Write-Host "  • All website content in S3"
  Write-Host "  • All database data in RDS"
  Write-Host "  • All CloudFront distributions"
  Write-Host "  • All infrastructure resources"
  Write-Host ""
  Write-Host "THIS CANNOT BE UNDONE!"
  Write-Host ""
  $confirm = Read-Host "Type 'DELETE EVERYTHING' (exactly, all caps) to confirm"
  if ($confirm -ne "DELETE EVERYTHING") {
    Write-Host "Deletion cancelled. (Good choice if you weren't sure!)"
    exit 0
  }
}

Write-Host ""
Write-Host "=========================================="
Write-Host "Step 1/2: Deleting Editor Stack"
Write-Host "=========================================="
Write-Host ""

# Delete Editor stack first (has dependencies on static site bucket)
& "$PSScriptRoot\destroy-skycms-editor.ps1" `
  -StackName $EditorStackName `
  -Region $Region `
  -Force `
  -DeleteIAMUser:$DeleteAllIAMUsers `
  -IAMUserName "skycms-s3-access"

if ($LASTEXITCODE -ne 0) {
  Write-Host ""
  Write-Host "⚠️  Editor stack deletion had issues. Continuing with static site..."
  Write-Host ""
}

Write-Host ""
Write-Host "=========================================="
Write-Host "Step 2/2: Deleting Static Site Stack"
Write-Host "=========================================="
Write-Host ""

# Delete Static site stack
& "$PSScriptRoot\destroy-static-site.ps1" `
  -StackName $StaticSiteStackName `
  -Region $Region `
  -Force `
  -EmptyBucket:$EmptyBucket `
  -DeleteCloudFrontPurgeUser:$DeleteAllIAMUsers `
  -CloudFrontPurgeUserName "skycms-purge-user"

if ($LASTEXITCODE -ne 0) {
  Write-Host ""
  Write-Host "⚠️  Static site stack deletion had issues."
  Write-Host ""
}

Write-Host ""
Write-Host "=========================================="
Write-Host "Complete Cleanup Finished! 🗑️"
Write-Host "=========================================="
Write-Host ""
Write-Host "All SkyCMS infrastructure has been removed from AWS."
Write-Host ""
Write-Host "💰 Cost savings: Resources are no longer incurring charges."
Write-Host ""
Write-Host "💡 To redeploy SkyCMS later, run the deployment scripts again:"
Write-Host "   1. .\deploy-static-site.ps1 (if you have this)"
Write-Host "   2. .\deploy-skycms-editor.ps1"
Write-Host ""

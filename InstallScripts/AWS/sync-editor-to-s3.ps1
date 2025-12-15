Param(
  [Parameter(Mandatory = $true)] [string] $EditorBuildPath,
  [string] $SiteStackName = "skycms-static-site",
  [string] $Region = "us-east-1",
  [string] $EditorPrefix = "editor/",
  [switch] $Invalidate
)

# Requires AWS CLI v2
$awsCmd = Get-Command aws -ErrorAction SilentlyContinue
if (-not $awsCmd) {
  Write-Error "AWS CLI not found. Install AWS CLI v2: https://aws.amazon.com/cli/"
  exit 1
}

if (-not (Test-Path -LiteralPath $EditorBuildPath)) {
  Write-Error "Editor build folder not found: $EditorBuildPath"
  exit 1
}

Write-Host "Resolving bucket and distribution from CloudFormation outputs..."
$outputs = aws cloudformation describe-stacks `
  --stack-name $SiteStackName `
  --region $Region `
  --query "Stacks[0].Outputs[*].{Key:OutputKey,Value:OutputValue}" `
  --output json | ConvertFrom-Json

$bucketName = ($outputs | Where-Object { $_.Key -eq 'WebsiteBucketName' }).Value
$distIdEditor = ($outputs | Where-Object { $_.Key -eq 'CloudFrontDistributionIdEditor' }).Value
$distIdMain   = ($outputs | Where-Object { $_.Key -eq 'CloudFrontDistributionId' }).Value
$cdnUrlEditor = ($outputs | Where-Object { $_.Key -eq 'CloudFrontURLEditor' }).Value
$cdnUrlMain   = ($outputs | Where-Object { $_.Key -eq 'CloudFrontURL' }).Value

# Prefer the dedicated editor distribution if present
$useEditorDist = -not [string]::IsNullOrWhiteSpace($distIdEditor)
$distId = if ($useEditorDist) { $distIdEditor } else { $distIdMain }
$cdnUrl = if ($useEditorDist -and $cdnUrlEditor) { $cdnUrlEditor } else { $cdnUrlMain }

if ([string]::IsNullOrWhiteSpace($bucketName)) { Write-Error "WebsiteBucketName output not found on stack '$SiteStackName'. Make sure the template was updated."; exit 1 }
if ([string]::IsNullOrWhiteSpace($distId)) { Write-Warning "CloudFront distribution ID not found. Invalidation step will be skipped unless provided elsewhere." }

Write-Host "Syncing $EditorBuildPath to s3://$bucketName/$EditorPrefix ..."
aws s3 sync "$EditorBuildPath" "s3://$bucketName/$EditorPrefix" --delete
if ($LASTEXITCODE -ne 0) { Write-Error "S3 sync failed."; exit 1 }

if ($Invalidate -and $distId) {
  $callerRef = "skycms-editor-" + [Guid]::NewGuid().ToString()
  if ($useEditorDist) {
    Write-Host "Creating CloudFront invalidation for /* on editor distribution $distId ..."
    aws cloudfront create-invalidation `
      --distribution-id $distId `
      --paths "/*" `
      --query "Invalidation.Id" `
      --output text | Write-Host
  } else {
    Write-Host "Creating CloudFront invalidation for /$EditorPrefix* on main distribution $distId ..."
    aws cloudfront create-invalidation `
      --distribution-id $distId `
      --paths "/$EditorPrefix*" `
      --query "Invalidation.Id" `
      --output text | Write-Host
  }
}

Write-Host "Done. Editor available at: $cdnUrl/ (default root) or $cdnUrl/index.html (if needed)."

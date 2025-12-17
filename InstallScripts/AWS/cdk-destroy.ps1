param(
  [string]$Region = "us-east-1"
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$cdkDir = Join-Path $PSScriptRoot 'cdk'

Write-Host "Using CDK app at: $cdkDir"

if (-not (Test-Path (Join-Path $cdkDir 'package.json'))) {
  throw "CDK project not found at $cdkDir"
}

Push-Location $cdkDir
try {
  $cdkBin = Join-Path $cdkDir "node_modules\aws-cdk\bin\cdk"
  Write-Host "Destroying SkyCmsMinimalStack ..."
  node $cdkBin destroy SkyCmsMinimalStack --force
}
finally {
  Pop-Location
}

param(
  [Parameter(Mandatory=$true)][string]$StackName,
  [Parameter(Mandatory=$false)][string]$Region = "us-west-2",
  [Parameter(Mandatory=$false)][string]$ParamsFile = "AWS/examples/skycms-sqlite-params.json"
)

aws cloudformation deploy `
  --region $Region `
  --template-file AWS/cloudformation-skycms.yaml `
  --stack-name $StackName `
  --parameter-overrides (Get-Content $ParamsFile -Raw | ConvertFrom-Json) `
  --capabilities CAPABILITY_NAMED_IAM

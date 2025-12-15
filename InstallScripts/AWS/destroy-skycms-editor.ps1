Param(
  [string] $StackName = "skycms-editor-fargate",
  [string] $Region = "us-east-1",
  [switch] $Force,
  [switch] $DeleteIAMUser,
  [string] $IAMUserName = "skycms-s3-access"
)

# Requires AWS CLI v2
$awsCmd = Get-Command aws -ErrorAction SilentlyContinue
if (-not $awsCmd) {
  Write-Error "AWS CLI not found. Install AWS CLI v2: https://aws.amazon.com/cli/"
  exit 1
}

Write-Host "=========================================="
Write-Host "=== SkyCMS Editor Stack Deletion ==="
Write-Host "=========================================="
Write-Host ""
Write-Host "This will DELETE the following resources:"
Write-Host "  • Stack: $StackName"
Write-Host "  • ECS Fargate cluster and tasks"
Write-Host "  • RDS MySQL database (including all data)"
Write-Host "  • Application Load Balancer"
Write-Host "  • CloudFront distribution"
Write-Host "  • VPC, subnets, security groups"
Write-Host "  • Lambda initialization function"
Write-Host "  • CloudWatch log groups"
Write-Host ""

if ($DeleteIAMUser) {
  Write-Host "  • IAM User: $IAMUserName (and access keys)"
  Write-Host ""
}

# Check if stack exists
$stackExists = $false
try {
  aws cloudformation describe-stacks --stack-name $StackName --region $Region 2>$null | Out-Null
  if ($LASTEXITCODE -eq 0) { $stackExists = $true }
} catch {
  $stackExists = $false
}

if (-not $stackExists) {
  Write-Host "⚠️  Stack '$StackName' does not exist in region $Region"
  if ($DeleteIAMUser) {
    Write-Host "Proceeding with IAM user deletion only..."
  } else {
    Write-Host "Nothing to delete."
    exit 0
  }
}

if (-not $Force -and $stackExists) {
  Write-Host "⚠️  WARNING: This action CANNOT be undone!"
  Write-Host "⚠️  All data in the RDS database will be permanently lost!"
  Write-Host ""
  $confirm = Read-Host "Type 'DELETE' (all caps) to confirm deletion"
  if ($confirm -ne "DELETE") {
    Write-Host "Deletion cancelled."
    exit 0
  }
}

if ($stackExists) {
  Write-Host ""
  Write-Host "Initiating stack deletion..."
  Write-Host "This may take 10-15 minutes (CloudFront deletion is slow)."
  Write-Host ""

  # Initiate deletion
  aws cloudformation delete-stack --stack-name $StackName --region $Region
  if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to initiate stack deletion."
    exit 1
  }

  Write-Host "✅ Deletion initiated"
  Write-Host ""
  Write-Host "Waiting for stack deletion to complete..."
  Write-Host "(Press Ctrl+C to stop waiting, but deletion will continue in background)"
  Write-Host ""

  # Wait for deletion
  aws cloudformation wait stack-delete-complete --stack-name $StackName --region $Region
  
  if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✅ Stack '$StackName' deleted successfully"
  } elseif ($LASTEXITCODE -eq 255) {
    # Stack doesn't exist (already deleted)
    Write-Host ""
    Write-Host "✅ Stack '$StackName' deleted successfully"
  } else {
    Write-Host ""
    Write-Host "⚠️  Stack deletion may have failed or is still in progress."
    Write-Host "Check the AWS Console or run:"
    Write-Host "  aws cloudformation describe-stacks --stack-name $StackName --region $Region"
  }
}

# Delete IAM user if requested
if ($DeleteIAMUser) {
  Write-Host ""
  Write-Host "Deleting IAM user '$IAMUserName'..."
  
  # Check if user exists
  aws iam get-user --user-name $IAMUserName 2>$null | Out-Null
  if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️  IAM user '$IAMUserName' does not exist"
  } else {
    # Delete all access keys first
    Write-Host "  Removing access keys..."
    $keys = aws iam list-access-keys --user-name $IAMUserName --query 'AccessKeyMetadata[*].AccessKeyId' --output json | ConvertFrom-Json
    if ($keys) {
      foreach ($key in $keys) {
        aws iam delete-access-key --user-name $IAMUserName --access-key-id $key 2>$null | Out-Null
        Write-Host "    ✅ Deleted key: $key"
      }
    }
    
    # Delete inline policies
    Write-Host "  Removing inline policies..."
    $policies = aws iam list-user-policies --user-name $IAMUserName --query 'PolicyNames' --output json | ConvertFrom-Json
    if ($policies) {
      foreach ($policy in $policies) {
        aws iam delete-user-policy --user-name $IAMUserName --policy-name $policy 2>$null | Out-Null
        Write-Host "    ✅ Deleted policy: $policy"
      }
    }
    
    # Delete attached managed policies
    $attachedPolicies = aws iam list-attached-user-policies --user-name $IAMUserName --query 'AttachedPolicies[*].PolicyArn' --output json | ConvertFrom-Json
    if ($attachedPolicies) {
      foreach ($policyArn in $attachedPolicies) {
        aws iam detach-user-policy --user-name $IAMUserName --policy-arn $policyArn 2>$null | Out-Null
        Write-Host "    ✅ Detached policy: $policyArn"
      }
    }
    
    # Delete user
    aws iam delete-user --user-name $IAMUserName 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) {
      Write-Host "  ✅ IAM user '$IAMUserName' deleted"
    } else {
      Write-Host "  ⚠️  Failed to delete IAM user (may have additional resources attached)"
    }
  }
}

Write-Host ""
Write-Host "=========================================="
Write-Host "Cleanup Complete! 🗑️"
Write-Host "=========================================="
Write-Host ""
Write-Host "Resources removed:"
if ($stackExists) {
  Write-Host "  ✅ CloudFormation stack: $StackName"
  Write-Host "  ✅ All associated AWS resources (ECS, RDS, ALB, CloudFront, VPC)"
}
if ($DeleteIAMUser) {
  Write-Host "  ✅ IAM user: $IAMUserName"
}
Write-Host ""
Write-Host "💡 Tip: Static site stack (S3 + CloudFront) is separate."
Write-Host "   To delete it, run: .\destroy-static-site.ps1"
Write-Host ""

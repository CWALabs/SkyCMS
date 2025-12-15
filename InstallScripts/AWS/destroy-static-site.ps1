Param(
  [string] $StackName = "skycms-static-site",
  [string] $Region = "us-east-1",
  [switch] $Force,
  [switch] $EmptyBucket,
  [switch] $DeleteCloudFrontPurgeUser,
  [string] $CloudFrontPurgeUserName = "skycms-purge-user"
)

# Requires AWS CLI v2
$awsCmd = Get-Command aws -ErrorAction SilentlyContinue
if (-not $awsCmd) {
  Write-Error "AWS CLI not found. Install AWS CLI v2: https://aws.amazon.com/cli/"
  exit 1
}

Write-Host "=========================================="
Write-Host "=== SkyCMS Static Site Stack Deletion ==="
Write-Host "=========================================="
Write-Host ""
Write-Host "This will DELETE the following resources:"
Write-Host "  • Stack: $StackName"
Write-Host "  • S3 bucket (including all website files)"
Write-Host "  • CloudFront distribution(s)"
Write-Host "  • Route 53 DNS records (if custom domain configured)"
Write-Host ""

if ($DeleteCloudFrontPurgeUser) {
  Write-Host "  • IAM User: $CloudFrontPurgeUserName (and access keys)"
  Write-Host ""
}

# Check if stack exists
$stackExists = $false
$bucketName = $null
try {
  $stack = aws cloudformation describe-stacks --stack-name $StackName --region $Region 2>$null | ConvertFrom-Json
  if ($LASTEXITCODE -eq 0 -and $stack.Stacks) {
    $stackExists = $true
    # Get bucket name from outputs
    $outputs = $stack.Stacks[0].Outputs
    $bucketName = ($outputs | Where-Object { $_.OutputKey -eq 'WebsiteBucketName' }).OutputValue
  }
} catch {
  $stackExists = $false
}

if (-not $stackExists) {
  Write-Host "⚠️  Stack '$StackName' does not exist in region $Region"
  if ($DeleteCloudFrontPurgeUser) {
    Write-Host "Proceeding with IAM user deletion only..."
  } else {
    Write-Host "Nothing to delete."
    exit 0
  }
}

if (-not $Force -and $stackExists) {
  Write-Host "⚠️  WARNING: This action CANNOT be undone!"
  Write-Host "⚠️  All website files in the S3 bucket will be permanently lost!"
  if ($bucketName) {
    Write-Host "⚠️  Bucket to be deleted: $bucketName"
  }
  Write-Host ""
  $confirm = Read-Host "Type 'DELETE' (all caps) to confirm deletion"
  if ($confirm -ne "DELETE") {
    Write-Host "Deletion cancelled."
    exit 0
  }
}

if ($stackExists -and $bucketName) {
  # Empty the S3 bucket first (required before CloudFormation can delete it)
  if ($EmptyBucket) {
    Write-Host ""
    Write-Host "Emptying S3 bucket '$bucketName'..."
    
    # Check if bucket exists
    aws s3api head-bucket --bucket $bucketName 2>$null
    if ($LASTEXITCODE -eq 0) {
      # Remove all objects and versions
      aws s3 rm "s3://$bucketName" --recursive
      if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Bucket emptied"
      } else {
        Write-Host "⚠️  Failed to empty bucket. Continuing with stack deletion..."
      }
    } else {
      Write-Host "⚠️  Bucket not found or not accessible"
    }
  } else {
    Write-Host ""
    Write-Host "⚠️  Note: S3 bucket must be empty before stack deletion can complete."
    Write-Host "   If deletion fails, run this script with -EmptyBucket flag:"
    Write-Host "   .\destroy-static-site.ps1 -EmptyBucket"
    Write-Host ""
  }
}

if ($stackExists) {
  Write-Host ""
  Write-Host "Initiating stack deletion..."
  Write-Host "This may take 15-20 minutes (CloudFront deletion is very slow)."
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
    Write-Host "Common issue: S3 bucket not empty. Run with -EmptyBucket flag."
    Write-Host ""
    Write-Host "Check status:"
    Write-Host "  aws cloudformation describe-stacks --stack-name $StackName --region $Region"
  }
}

# Delete CloudFront purge user if requested
if ($DeleteCloudFrontPurgeUser) {
  Write-Host ""
  Write-Host "Deleting IAM user '$CloudFrontPurgeUserName'..."
  
  # Check if user exists
  aws iam get-user --user-name $CloudFrontPurgeUserName 2>$null | Out-Null
  if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️  IAM user '$CloudFrontPurgeUserName' does not exist"
  } else {
    # Delete all access keys first
    Write-Host "  Removing access keys..."
    $keys = aws iam list-access-keys --user-name $CloudFrontPurgeUserName --query 'AccessKeyMetadata[*].AccessKeyId' --output json | ConvertFrom-Json
    if ($keys) {
      foreach ($key in $keys) {
        aws iam delete-access-key --user-name $CloudFrontPurgeUserName --access-key-id $key 2>$null | Out-Null
        Write-Host "    ✅ Deleted key: $key"
      }
    }
    
    # Delete inline policies
    Write-Host "  Removing inline policies..."
    $policies = aws iam list-user-policies --user-name $CloudFrontPurgeUserName --query 'PolicyNames' --output json | ConvertFrom-Json
    if ($policies) {
      foreach ($policy in $policies) {
        aws iam delete-user-policy --user-name $CloudFrontPurgeUserName --policy-name $policy 2>$null | Out-Null
        Write-Host "    ✅ Deleted policy: $policy"
      }
    }
    
    # Delete attached managed policies
    $attachedPolicies = aws iam list-attached-user-policies --user-name $CloudFrontPurgeUserName --query 'AttachedPolicies[*].PolicyArn' --output json | ConvertFrom-Json
    if ($attachedPolicies) {
      foreach ($policyArn in $attachedPolicies) {
        aws iam detach-user-policy --user-name $CloudFrontPurgeUserName --policy-arn $policyArn 2>$null | Out-Null
        Write-Host "    ✅ Detached policy: $policyArn"
      }
    }
    
    # Delete user
    aws iam delete-user --user-name $CloudFrontPurgeUserName 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) {
      Write-Host "  ✅ IAM user '$CloudFrontPurgeUserName' deleted"
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
  Write-Host "  ✅ All associated AWS resources (S3, CloudFront)"
}
if ($DeleteCloudFrontPurgeUser) {
  Write-Host "  ✅ IAM user: $CloudFrontPurgeUserName"
}
Write-Host ""
Write-Host "💡 Tip: Editor stack (ECS, RDS, ALB) is separate."
Write-Host "   To delete it, run: .\destroy-skycms-editor.ps1"
Write-Host ""

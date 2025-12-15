
# AWS S3 access keys for SkyCMS (quick guide)

Use this guide to create an AWS access key pair (Access Key ID and Secret Access Key) with least‑privilege permissions for a single S3 bucket used by SkyCMS.

## Prerequisites

- Your S3 bucket name (e.g., `my-site-content`)
- Bucket region (e.g., `us-west-2`)
- Permission to manage IAM users/policies in your AWS account

## 1) Create an IAM user (programmatic access)

Console path: AWS Console → IAM → Users → Create user

1. Click "Create user".
1. Enter a user name (e.g., `skycms-s3-user`).
1. Leave console sign-in disabled (we only need programmatic access via access keys).
1. Click "Next" to skip permissions (we'll attach a policy in a later step).
1. Click "Create user".

## 2) Create a policy for your bucket

**Note:** S3 permissions are scoped to either the bucket itself (bucket-level) or to objects within the bucket (object-level).  

- Bucket-level actions (like `s3:ListBucket`) apply to the bucket resource (`arn:aws:s3:::your-bucket-name`).  
- Object-level actions (like `s3:GetObject`, `s3:PutObject`, `s3:DeleteObject`) apply to objects within the bucket (`arn:aws:s3:::your-bucket-name/*`).

Console path: IAM → Policies → Create policy → JSON tab

1. Click "Create policy" and switch to the JSON tab.
1. Paste this policy (replace `your-bucket-name`):

```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": ["s3:ListBucket"],
            "Resource": ["arn:aws:s3:::your-bucket-name"]
        },
        {
            "Effect": "Allow",
            "Action": [
                "s3:GetObject",
                "s3:PutObject",
                "s3:DeleteObject",
                "s3:DeleteObjectVersion"
            ],
            "Resource": ["arn:aws:s3:::your-bucket-name/*"]
        }
    ]
}
```

The above grants read (GetObject), write (PutObject), and delete (DeleteObject/DeleteObjectVersion) permissions on objects within the specified bucket, plus the ability to list the bucket contents.

1. Choose "Next", add a name (e.g., `skycms-s3-bucket-access`) and an optional description, then "Create policy".

**Note:** S3 permissions are scoped to either the bucket itself (bucket-level) or to objects within the bucket (object-level).  

- Bucket-level actions (like `s3:ListBucket`) apply to the bucket resource (`arn:aws:s3:::your-bucket-name`).  
- Object-level actions (like `s3:GetObject`, `s3:PutObject`, `s3:DeleteObject`) apply to objects within the bucket (`arn:aws:s3:::your-bucket-name/*`).


Console path: IAM → Policies → Create policy → JSON tab

1. Click "Create policy" and switch to the JSON tab.
1. Paste this policy (replace `your-bucket-name`):

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": ["s3:ListBucket"],
      "Resource": ["arn:aws:s3:::your-bucket-name"]
    },
    {
      "Effect": "Allow",
      "Action": [
        "s3:GetObject",
        "s3:PutObject",
        "s3:DeleteObject",
        "s3:DeleteObjectVersion"
      ],
      "Resource": ["arn:aws:s3:::your-bucket-name/*"]
    }
  ]
}
```

The above grants read (GetObject), write (PutObject), and delete (DeleteObject/ DeleteObjectVersion) on objects within the specified bucket, plus the ability to list the bucket.

1. Choose "Next", add a name (e.g., `skycms-s3-bucket-access`) and an optional description, then "Create policy".

## 3) Attach the policy to the user

Console path: IAM → Users → your user → Permissions

1. Click "Add permissions".
1. Choose "Attach policies directly".
1. Search for your policy name (e.g., `skycms-s3-bucket-access`) and select it.
1. Click "Next" (if prompted), then "Add permissions".

## 4) Create access keys

Console path: IAM → Users → your user → Security credentials → Access keys

1. Click "Create access key".
1. For use case, select "Application running outside AWS" and acknowledge the notice.
1. Click "Create access key".
1. Copy the Access Key ID and Secret Access Key, or download the CSV.

Important: You will not be able to view the Secret Access Key again after closing this dialog. Store it securely (password manager or secrets store).

## 5) Configure SkyCMS

Add this connection string to `appsettings.json` (or supply it as an environment variable/secret):

```json
{
  "ConnectionStrings": {
    "StorageConnectionString": "Bucket={your-bucket-name};Region={aws-region};KeyId={your-access-key-id};Key={your-secret-access-key};"
  }
}
```

Example:

```json
{
  "ConnectionStrings": {
    "StorageConnectionString": "Bucket=my-site-content;Region=us-west-2;KeyId=AKIA...;Key=abcd..."
  }
}
```

On Azure App Service, you can set this under Configuration:

- App Settings: key `ConnectionStrings:StorageConnectionString` with the full value above, or
- Connection strings: add a new connection string named `StorageConnectionString` (type: Custom).

## Tips

- Grant only needed actions; avoid wildcard `*`
- Don’t commit keys; use environment variables or a secrets store
- Rotate keys regularly; remove unused keys
- If running on AWS compute, prefer IAM roles over long‑lived keys

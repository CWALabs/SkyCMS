#!/usr/bin/env node
import * as cdk from 'aws-cdk-lib';
import * as cloudfront from 'aws-cdk-lib/aws-cloudfront';
import * as s3 from 'aws-cdk-lib/aws-s3';
import * as iam from 'aws-cdk-lib/aws-iam';
import { Construct } from 'constructs';

// Stack Definition
class SkyCmsCloudFrontTestStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    // S3 bucket for CloudFront origin
    const bucket = new s3.Bucket(this, 'TestBucket', {
      bucketName: `skycms-test-${this.account}`,
      removalPolicy: cdk.RemovalPolicy.DESTROY,
      autoDeleteObjects: true,
      publicReadAccess: false,
      blockPublicAccess: s3.BlockPublicAccess.BLOCK_ALL,
    });

    // CloudFront Origin Access Identity
    const oai = new cloudfront.OriginAccessIdentity(this, 'OAI', {
      comment: 'SkyCMS Test OAI',
    });

    bucket.grantRead(oai);

    // CloudFront Distribution
    const distribution = new cloudfront.CloudFrontWebDistribution(this, 'Distribution', {
      comment: 'SkyCMS Test Distribution',
      originConfigs: [{
        s3OriginSource: {
          s3BucketSource: bucket,
          originAccessIdentity: oai,
        },
        behaviors: [{
          isDefaultBehavior: true,
          compress: true,
          viewerProtocolPolicy: cloudfront.ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
        }],
      }],
      priceClass: cloudfront.PriceClass.PRICE_CLASS_100,
    });

    // IAM User for testing
    const user = new iam.User(this, 'TestUser', {
      userName: 'skycms-cloudfront-tester',
    });

    user.addToPolicy(new iam.PolicyStatement({
      actions: ['cloudfront:CreateInvalidation', 'cloudfront:GetInvalidation', 'cloudfront:ListInvalidations'],
      resources: [`arn:aws:cloudfront::${this.account}:distribution/${distribution.distributionId}`],
    }));

    const accessKey = new iam.CfnAccessKey(this, 'AccessKey', {
      userName: user.userName,
    });

    // Outputs
    new cdk.CfnOutput(this, 'DistributionId', {
      value: distribution.distributionId,
      description: 'Use this in your user secrets',
    });

    new cdk.CfnOutput(this, 'AccessKeyId', {
      value: accessKey.ref,
      description: 'Access Key ID',
    });

    new cdk.CfnOutput(this, 'SecretAccessKey', {
      value: accessKey.attrSecretAccessKey,
      description: 'Secret Access Key (save securely!)',
    });

    new cdk.CfnOutput(this, 'SetupCommands', {
      value: [
        'cd Tests',
        `dotnet user-secrets set "AWS:CloudFront:DistributionId" "${distribution.distributionId}"`,
        `dotnet user-secrets set "AWS:CloudFront:AccessKeyId" "${accessKey.ref}"`,
        `dotnet user-secrets set "AWS:CloudFront:SecretAccessKey" "${accessKey.attrSecretAccessKey}"`,
        `dotnet user-secrets set "AWS:CloudFront:Region" "${this.region}"`,
      ].join(' && '),
      description: 'Run these commands to configure your tests',
    });
  }
}

// App
const app = new cdk.App();
new SkyCmsCloudFrontTestStack(app, 'SkyCmsCloudFrontTest', {
  env: {
    account: process.env.CDK_DEFAULT_ACCOUNT,
    region: process.env.CDK_DEFAULT_REGION || 'us-east-1',
  },
});
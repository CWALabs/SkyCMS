import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';
import * as iam from 'aws-cdk-lib/aws-iam';
import * as lambda from 'aws-cdk-lib/aws-lambda';
import * as cr from 'aws-cdk-lib/custom-resources';
import * as logs from 'aws-cdk-lib/aws-logs';

export interface AccessKeyGeneratorProps {
  userName: string;
}

/**
 * Custom resource that generates IAM access keys for a user.
 * Since CDK/CloudFormation doesn't natively create access keys,
 * we use a Lambda-backed custom resource.
 */
export class AccessKeyGenerator extends Construct {
  public readonly accessKeyId: string;
  public readonly secretAccessKey: string;

  constructor(scope: Construct, id: string, props: AccessKeyGeneratorProps) {
    super(scope, id);

    // Lambda function that creates/deletes access keys
    const onEventHandler = new lambda.Function(this, 'OnEventHandler', {
      runtime: lambda.Runtime.PYTHON_3_12,
      handler: 'index.on_event',
      code: lambda.Code.fromInline(`
import boto3
import json

iam_client = boto3.client('iam')

def on_event(event, context):
    request_type = event['RequestType']
    user_name = event['ResourceProperties']['UserName']
    
    if request_type == 'Create':
        # Create new access key
        response = iam_client.create_access_key(UserName=user_name)
        access_key = response['AccessKey']
        return {
            'PhysicalResourceId': access_key['AccessKeyId'],
            'Data': {
                'AccessKeyId': access_key['AccessKeyId'],
                'SecretAccessKey': access_key['SecretAccessKey']
            }
        }
    
    elif request_type == 'Update':
        # For updates, we'll create a new key and delete the old one
        old_key_id = event['PhysicalResourceId']
        
        # Create new key
        response = iam_client.create_access_key(UserName=user_name)
        access_key = response['AccessKey']
        
        # Delete old key if it's different
        if old_key_id != access_key['AccessKeyId']:
            try:
                iam_client.delete_access_key(
                    UserName=user_name,
                    AccessKeyId=old_key_id
                )
            except:
                pass  # Old key might not exist
        
        return {
            'PhysicalResourceId': access_key['AccessKeyId'],
            'Data': {
                'AccessKeyId': access_key['AccessKeyId'],
                'SecretAccessKey': access_key['SecretAccessKey']
            }
        }
    
    elif request_type == 'Delete':
        # Delete the access key
        access_key_id = event['PhysicalResourceId']
        try:
            iam_client.delete_access_key(
                UserName=user_name,
                AccessKeyId=access_key_id
            )
        except:
            pass  # Key might already be deleted
        
        return {'PhysicalResourceId': access_key_id}
`),
      timeout: cdk.Duration.minutes(2),
      logRetention: logs.RetentionDays.ONE_DAY,
    });

    // Grant permissions to create/delete access keys
    onEventHandler.addToRolePolicy(
      new iam.PolicyStatement({
        actions: ['iam:CreateAccessKey', 'iam:DeleteAccessKey'],
        resources: [`arn:aws:iam::${cdk.Stack.of(this).account}:user/${props.userName}`],
      })
    );

    // Custom resource provider
    const provider = new cr.Provider(this, 'Provider', {
      onEventHandler,
      logRetention: logs.RetentionDays.ONE_DAY,
    });

    // Custom resource
    const resource = new cdk.CustomResource(this, 'Resource', {
      serviceToken: provider.serviceToken,
      properties: {
        UserName: props.userName,
      },
    });

    this.accessKeyId = resource.getAttString('AccessKeyId');
    this.secretAccessKey = resource.getAttString('SecretAccessKey');
  }
}

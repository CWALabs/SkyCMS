"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
exports.AccessKeyGenerator = void 0;
const cdk = __importStar(require("aws-cdk-lib"));
const constructs_1 = require("constructs");
const iam = __importStar(require("aws-cdk-lib/aws-iam"));
const lambda = __importStar(require("aws-cdk-lib/aws-lambda"));
const cr = __importStar(require("aws-cdk-lib/custom-resources"));
const logs = __importStar(require("aws-cdk-lib/aws-logs"));
/**
 * Custom resource that generates IAM access keys for a user.
 * Since CDK/CloudFormation doesn't natively create access keys,
 * we use a Lambda-backed custom resource.
 */
class AccessKeyGenerator extends constructs_1.Construct {
    constructor(scope, id, props) {
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
        onEventHandler.addToRolePolicy(new iam.PolicyStatement({
            actions: ['iam:CreateAccessKey', 'iam:DeleteAccessKey'],
            resources: [`arn:aws:iam::${cdk.Stack.of(this).account}:user/${props.userName}`],
        }));
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
exports.AccessKeyGenerator = AccessKeyGenerator;

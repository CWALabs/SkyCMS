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
        // Explicit log group to control retention (replaces deprecated logRetention)
        const onEventLogGroup = new logs.LogGroup(this, 'OnEventHandlerLogGroup', {
            retention: logs.RetentionDays.ONE_DAY,
        });
        const onEventHandler = new lambda.Function(this, 'OnEventHandler', {
            runtime: lambda.Runtime.PYTHON_3_12,
            handler: 'index.on_event',
            code: lambda.Code.fromInline(`
import boto3
import json
import hmac
import hashlib
import base64

iam_client = boto3.client('iam')


def _sig(key, msg):
    return hmac.new(key, msg.encode('utf-8'), hashlib.sha256).digest()


def _smtp_password(secret_key, region):
    # SES SMTP password derivation per AWS docs (SigV4-based)
    date_key = _sig(('AWS4' + secret_key).encode('utf-8'), '11111111')
    region_key = _sig(date_key, region)
    service_key = _sig(region_key, 'ses')
    signing_key = _sig(service_key, 'aws4_request')
    signature = _sig(signing_key, 'SendRawEmail')
    return base64.b64encode(b'AWS4' + signature).decode('utf-8')


def on_event(event, context):
    request_type = event['RequestType']
    user_name = event['ResourceProperties']['UserName']
    region = event['ResourceProperties'].get('Region', 'us-east-1')

    if request_type == 'Create':
        response = iam_client.create_access_key(UserName=user_name)
        access_key = response['AccessKey']
        return {
            'PhysicalResourceId': access_key['AccessKeyId'],
            'Data': {
                'AccessKeyId': access_key['AccessKeyId'],
                'SecretAccessKey': access_key['SecretAccessKey'],
                'SmtpPassword': _smtp_password(access_key['SecretAccessKey'], region)
            }
        }

    elif request_type == 'Update':
        old_key_id = event['PhysicalResourceId']
        response = iam_client.create_access_key(UserName=user_name)
        access_key = response['AccessKey']
        if old_key_id != access_key['AccessKeyId']:
            try:
                iam_client.delete_access_key(UserName=user_name, AccessKeyId=old_key_id)
            except Exception:
                pass
        return {
            'PhysicalResourceId': access_key['AccessKeyId'],
            'Data': {
                'AccessKeyId': access_key['AccessKeyId'],
                'SecretAccessKey': access_key['SecretAccessKey'],
                'SmtpPassword': _smtp_password(access_key['SecretAccessKey'], region)
            }
        }

    elif request_type == 'Delete':
        access_key_id = event['PhysicalResourceId']
        try:
            iam_client.delete_access_key(UserName=user_name, AccessKeyId=access_key_id)
        except Exception:
            pass
        return {'PhysicalResourceId': access_key_id}
`),
            timeout: cdk.Duration.minutes(2),
            logGroup: onEventLogGroup,
        });
        // Grant permissions to create/delete access keys
        onEventHandler.addToRolePolicy(new iam.PolicyStatement({
            actions: ['iam:CreateAccessKey', 'iam:DeleteAccessKey'],
            resources: [`arn:aws:iam::${cdk.Stack.of(this).account}:user/${props.userName}`],
        }));
        // Custom resource provider
        const provider = new cr.Provider(this, 'Provider', {
            onEventHandler,
        });
        // Custom resource
        const resource = new cdk.CustomResource(this, 'Resource', {
            serviceToken: provider.serviceToken,
            properties: {
                UserName: props.userName,
                Region: props.region ?? cdk.Stack.of(this).region,
            },
        });
        this.accessKeyId = resource.getAttString('AccessKeyId');
        this.secretAccessKey = resource.getAttString('SecretAccessKey');
        this.smtpPassword = resource.getAttString('SmtpPassword');
    }
}
exports.AccessKeyGenerator = AccessKeyGenerator;

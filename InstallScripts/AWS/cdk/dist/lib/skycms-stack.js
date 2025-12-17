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
exports.SkyCmsEditorStack = void 0;
const cdk = __importStar(require("aws-cdk-lib"));
const ec2 = __importStar(require("aws-cdk-lib/aws-ec2"));
const ecs = __importStar(require("aws-cdk-lib/aws-ecs"));
const ecs_patterns = __importStar(require("aws-cdk-lib/aws-ecs-patterns"));
const rds = __importStar(require("aws-cdk-lib/aws-rds"));
const cloudfront = __importStar(require("aws-cdk-lib/aws-cloudfront"));
const origins = __importStar(require("aws-cdk-lib/aws-cloudfront-origins"));
class SkyCmsEditorStack extends cdk.Stack {
    constructor(scope, id, props) {
        super(scope, id, props);
        // VPC: public subnets for ALB/ECS, isolated subnets for RDS. No NAT to reduce cost.
        const vpc = new ec2.Vpc(this, 'Vpc', {
            maxAzs: 2,
            natGateways: 0,
            subnetConfiguration: [
                { name: 'Public', subnetType: ec2.SubnetType.PUBLIC },
                { name: 'Isolated', subnetType: ec2.SubnetType.PRIVATE_ISOLATED },
            ],
        });
        // ECS Cluster
        const cluster = new ecs.Cluster(this, 'Cluster', { vpc });
        // RDS Security Group (will wire permissions after Fargate is created)
        const dbSg = new ec2.SecurityGroup(this, 'DbSg', { vpc });
        // RDS Parameter Group with TLS enforcement
        const parameterGroup = new rds.ParameterGroup(this, 'MySqlParams', {
            engine: rds.DatabaseInstanceEngine.mysql({ version: rds.MysqlEngineVersion.VER_8_0 }),
            parameters: {
                require_secure_transport: '1',
            },
        });
        // Generate a fixed password for RDS (dev-only)
        const dbPassword = cdk.SecretValue.unsafePlainText('SkyCMS2025!Temp');
        const db = new rds.DatabaseInstance(this, 'MySql', {
            engine: rds.DatabaseInstanceEngine.mysql({ version: rds.MysqlEngineVersion.VER_8_0 }),
            vpc,
            vpcSubnets: { subnetType: ec2.SubnetType.PRIVATE_ISOLATED },
            instanceType: ec2.InstanceType.of(ec2.InstanceClass.BURSTABLE4_GRAVITON, ec2.InstanceSize.MICRO),
            allocatedStorage: 20,
            storageType: rds.StorageType.GP3,
            securityGroups: [dbSg],
            credentials: rds.Credentials.fromPassword('skycms_admin', dbPassword),
            databaseName: props.dbName,
            deletionProtection: false,
            removalPolicy: cdk.RemovalPolicy.DESTROY, // dev convenience
            backupRetention: cdk.Duration.days(7),
            publiclyAccessible: false,
            parameterGroup: parameterGroup,
        });
        // Fargate Service with ALB
        const fargate = new ecs_patterns.ApplicationLoadBalancedFargateService(this, 'EditorService', {
            cluster,
            assignPublicIp: true, // allow internet egress without NAT
            cpu: 512,
            memoryLimitMiB: 1024,
            desiredCount: props.desiredCount ?? 1,
            taskImageOptions: {
                image: ecs.ContainerImage.fromRegistry(props.image),
                containerPort: 80,
                environment: {
                    CosmosAllowSetup: 'true',
                    MultiTenantEditor: 'false',
                    ASPNETCORE_ENVIRONMENT: 'Production',
                    BlobServiceProvider: 'Amazon',
                    AmazonS3BucketName: props.bucketName || '',
                    AmazonS3Region: cdk.Aws.REGION,
                    SKYCMS_DB_HOST: db.instanceEndpoint.hostname,
                    SKYCMS_DB_USER: 'skycms_admin',
                    SKYCMS_DB_NAME: props.dbName,
                    SKYCMS_DB_PASSWORD: dbPassword.unsafeUnwrap(),
                    SKYCMS_DB_SSL: 'true',
                    SKYCMS_DB_SSL_MODE: 'Required',
                },
            },
            publicLoadBalancer: true,
        });
        // Allow access from ECS tasks to RDS
        dbSg.addIngressRule(ec2.Peer.securityGroupId(fargate.service.connections.securityGroups[0].securityGroupId), ec2.Port.tcp(3306), 'ECS tasks to MySQL');
        // Temporarily allow access from all IPs (0.0.0.0/0) for dev - MySQL Workbench access
        dbSg.addIngressRule(ec2.Peer.anyIpv4(), ec2.Port.tcp(3306), 'Temporary: Allow all IPv4 for MySQL Workbench access');
        // Health check tuning
        fargate.targetGroup.configureHealthCheck({
            path: '/',
            healthyHttpCodes: '200-399',
            timeout: cdk.Duration.seconds(5),
            interval: cdk.Duration.seconds(15),
            healthyThresholdCount: 2,
            unhealthyThresholdCount: 2,
        });
        fargate.service.node.tryGetContext('healthCheckGracePeriod');
        // Grant ECS task role access to S3 bucket
        if (props.bucketName) {
            const s3Arn = `arn:aws:s3:::${props.bucketName}`;
            fargate.taskDefinition.taskRole?.addToPrincipalPolicy(new cdk.aws_iam.PolicyStatement({
                effect: cdk.aws_iam.Effect.ALLOW,
                actions: ['s3:GetObject', 's3:PutObject', 's3:DeleteObject', 's3:ListBucket'],
                resources: [s3Arn, `${s3Arn}/*`],
            }));
        }
        // CloudFront in front of ALB with no caching
        const dist = new cloudfront.Distribution(this, 'EditorCdn', {
            defaultBehavior: {
                origin: new origins.LoadBalancerV2Origin(fargate.loadBalancer, {
                    protocolPolicy: cloudfront.OriginProtocolPolicy.HTTP_ONLY,
                }),
                cachePolicy: cloudfront.CachePolicy.CACHING_DISABLED,
                originRequestPolicy: cloudfront.OriginRequestPolicy.ALL_VIEWER,
                viewerProtocolPolicy: cloudfront.ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
            },
            comment: 'SkyCMS Editor CDN',
            defaultRootObject: 'index.html',
        });
        new cdk.CfnOutput(this, 'EditorURL', {
            value: `https://${dist.domainName}`,
            description: 'CloudFront URL for the SkyCMS Editor',
        });
        new cdk.CfnOutput(this, 'DatabaseEndpoint', {
            value: db.instanceEndpoint.hostname,
            description: 'RDS MySQL endpoint hostname',
        });
        new cdk.CfnOutput(this, 'DatabasePort', {
            value: '3306',
            description: 'RDS MySQL port',
        });
        new cdk.CfnOutput(this, 'DatabaseUsername', {
            value: 'skycms_admin',
            description: 'RDS MySQL admin username',
        });
        new cdk.CfnOutput(this, 'DatabasePassword', {
            value: dbPassword.unsafeUnwrap(),
            description: '⚠️  DATABASE PASSWORD (dev-only; store securely). Use with MySQL Workbench or other tools.',
        });
        new cdk.CfnOutput(this, 'DatabaseName', {
            value: props.dbName,
            description: 'Default database created in RDS',
        });
        const mysqlConnectionString = `Server=${db.instanceEndpoint.hostname};Port=3306;Database=${props.dbName};Uid=skycms_admin;Pwd=${dbPassword.unsafeUnwrap()};SslMode=Required;`;
        new cdk.CfnOutput(this, 'MySqlConnectionString', {
            value: mysqlConnectionString,
            description: '✅ MySQL Connection String for MySQL Workbench or other tools (dev-only, includes TLS)',
        });
        new cdk.CfnOutput(this, 'S3BucketName', {
            value: props.bucketName || '(not provided)',
            description: 'S3 bucket for media storage - fill this in the setup wizard',
        });
        new cdk.CfnOutput(this, 'S3Region', {
            value: cdk.Aws.REGION,
            description: 'AWS region for S3 bucket - fill this in the setup wizard',
        });
    }
}
exports.SkyCmsEditorStack = SkyCmsEditorStack;

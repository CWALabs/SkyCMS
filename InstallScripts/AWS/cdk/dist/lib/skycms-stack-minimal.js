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
const logs = __importStar(require("aws-cdk-lib/aws-logs"));
const rds = __importStar(require("aws-cdk-lib/aws-rds"));
const secretsmanager = __importStar(require("aws-cdk-lib/aws-secretsmanager"));
const elbv2 = __importStar(require("aws-cdk-lib/aws-elasticloadbalancingv2"));
const cloudfront = __importStar(require("aws-cdk-lib/aws-cloudfront"));
const origins = __importStar(require("aws-cdk-lib/aws-cloudfront-origins"));
const acm = __importStar(require("aws-cdk-lib/aws-certificatemanager"));
const route53 = __importStar(require("aws-cdk-lib/aws-route53"));
const s3 = __importStar(require("aws-cdk-lib/aws-s3"));
const iam = __importStar(require("aws-cdk-lib/aws-iam"));
const access_key_generator_1 = require("./access-key-generator");
class SkyCmsEditorStack extends cdk.Stack {
    constructor(scope, id, props) {
        super(scope, id, props);
        const desiredCount = props.desiredCount || 1;
        const dbName = props.dbName || 'skycms';
        const deployPublisher = props.deployPublisher !== undefined ? props.deployPublisher : false;
        const certificateArn = this.node.tryGetContext('certificateArn');
        const domainName = this.node.tryGetContext('domainName');
        const hostedZoneId = this.node.tryGetContext('hostedZoneId');
        const hostedZoneName = this.node.tryGetContext('hostedZoneName');
        const publisherDomainName = this.node.tryGetContext('publisherDomainName');
        const publisherCertificateArn = this.node.tryGetContext('publisherCertificateArn');
        // ============================================
        // PUBLISHER RESOURCES (S3 + CloudFront)
        // ============================================
        let storageSecret;
        let publisherBucket;
        let publisherDistribution;
        if (deployPublisher) {
            // S3 Bucket for static website
            publisherBucket = new s3.Bucket(this, 'PublisherBucket', {
                websiteIndexDocument: 'index.html',
                websiteErrorDocument: 'error.html',
                publicReadAccess: false,
                blockPublicAccess: s3.BlockPublicAccess.BLOCK_ALL,
                removalPolicy: cdk.RemovalPolicy.DESTROY,
                autoDeleteObjects: true,
            });
            // Origin Access Identity for CloudFront
            const oai = new cloudfront.OriginAccessIdentity(this, 'PublisherOAI', {
                comment: `OAI for ${this.stackName} Publisher`,
            });
            // Grant CloudFront read access to bucket
            publisherBucket.grantRead(oai);
            // CloudFront distribution for Publisher
            publisherDistribution = new cloudfront.Distribution(this, 'PublisherDistribution', {
                defaultBehavior: {
                    origin: new origins.S3Origin(publisherBucket, {
                        originAccessIdentity: oai,
                    }),
                    viewerProtocolPolicy: cloudfront.ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
                    allowedMethods: cloudfront.AllowedMethods.ALLOW_GET_HEAD,
                    cachedMethods: cloudfront.CachedMethods.CACHE_GET_HEAD,
                    compress: true,
                    cachePolicy: cloudfront.CachePolicy.CACHING_OPTIMIZED,
                },
                defaultRootObject: 'index.html',
                priceClass: cloudfront.PriceClass.PRICE_CLASS_100,
                domainNames: publisherDomainName ? [publisherDomainName] : undefined,
                certificate: publisherCertificateArn
                    ? acm.Certificate.fromCertificateArn(this, 'PublisherCert', publisherCertificateArn)
                    : undefined,
                minimumProtocolVersion: cloudfront.SecurityPolicyProtocol.TLS_V1_2_2021,
            });
            // IAM User for S3 access (unique per stack to avoid collisions)
            const iamUserName = `skycms-s3-publisher-user-${this.stackName}`;
            const s3User = new iam.User(this, 'S3PublisherUser', {
                userName: iamUserName,
            });
            // Grant S3 permissions to user
            s3User.addToPolicy(new iam.PolicyStatement({
                actions: ['s3:GetObject', 's3:PutObject', 's3:DeleteObject', 's3:ListBucket'],
                resources: [publisherBucket.bucketArn, `${publisherBucket.bucketArn}/*`],
            }));
            // Generate access keys using custom resource
            const accessKeys = new access_key_generator_1.AccessKeyGenerator(this, 'AccessKeys', {
                userName: s3User.userName,
            });
            // Create storage connection string and store in Secrets Manager
            const connectionString = `Bucket=${publisherBucket.bucketName};Region=${this.region};KeyId=${accessKeys.accessKeyId};Key=${accessKeys.secretAccessKey};`;
            storageSecret = new secretsmanager.Secret(this, 'StorageSecret', {
                secretName: `SkyCms-StorageConnectionString-${this.stackName}`,
                description: 'S3 storage connection string for SkyCMS',
                secretStringValue: cdk.SecretValue.unsafePlainText(connectionString),
            });
            // Outputs for Publisher
            new cdk.CfnOutput(this, 'PublisherBucketName', {
                value: publisherBucket.bucketName,
                description: 'S3 bucket for Publisher static files',
            });
            new cdk.CfnOutput(this, 'PublisherCloudFrontURL', {
                value: `https://${publisherDistribution.distributionDomainName}`,
                description: 'Publisher CloudFront distribution URL',
            });
            new cdk.CfnOutput(this, 'StorageSecretArn', {
                value: storageSecret.secretArn,
                description: 'ARN of storage connection string secret',
            });
        }
        // ============================================
        // EDITOR RESOURCES (ECS + RDS + CloudFront)
        // ============================================
        // VPC with 2 AZs, public subnets for ECS, isolated subnets for RDS
        const vpc = new ec2.Vpc(this, 'Vpc', {
            maxAzs: 2,
            natGateways: 0,
            subnetConfiguration: [
                {
                    cidrMask: 24,
                    name: 'Public',
                    subnetType: ec2.SubnetType.PUBLIC,
                },
                {
                    cidrMask: 24,
                    name: 'Isolated',
                    subnetType: ec2.SubnetType.PRIVATE_ISOLATED,
                },
            ],
        });
        // ECS Cluster
        const cluster = new ecs.Cluster(this, 'Cluster', { vpc });
        // RDS MySQL Parameter Group with TLS enforcement
        const parameterGroup = new rds.ParameterGroup(this, 'DbParameterGroup', {
            engine: rds.DatabaseInstanceEngine.mysql({
                version: rds.MysqlEngineVersion.VER_8_0,
            }),
            parameters: {
                require_secure_transport: '1',
            },
        });
        // Database credentials in Secrets Manager
        const dbCredentials = new secretsmanager.Secret(this, 'DbCredentials', {
            generateSecretString: {
                secretStringTemplate: JSON.stringify({ username: 'admin' }),
                generateStringKey: 'password',
                excludePunctuation: true,
                includeSpace: false,
                passwordLength: 16,
            },
        });
        // RDS Security Group
        const dbSecurityGroup = new ec2.SecurityGroup(this, 'DbSg', {
            vpc,
            description: 'RDS MySQL Security Group',
            allowAllOutbound: false,
        });
        // RDS MySQL Instance
        const database = new rds.DatabaseInstance(this, 'Database', {
            engine: rds.DatabaseInstanceEngine.mysql({
                version: rds.MysqlEngineVersion.VER_8_0,
            }),
            instanceType: ec2.InstanceType.of(ec2.InstanceClass.T4G, ec2.InstanceSize.MICRO),
            vpc,
            vpcSubnets: { subnetType: ec2.SubnetType.PRIVATE_ISOLATED },
            securityGroups: [dbSecurityGroup],
            credentials: rds.Credentials.fromSecret(dbCredentials),
            databaseName: dbName,
            parameterGroup,
            allocatedStorage: 20,
            maxAllocatedStorage: 100,
            multiAz: false,
            publiclyAccessible: false,
            removalPolicy: cdk.RemovalPolicy.DESTROY,
            deletionProtection: false,
        });
        // Construct connection string from RDS database endpoint and credentials
        const dbUser = dbCredentials.secretValueFromJson('username').toString();
        const dbPassword = dbCredentials.secretValueFromJson('password').toString();
        const connectionString = `Server=${database.dbInstanceEndpointAddress};Port=3306;Uid=${dbUser};Pwd=${dbPassword};Database=${dbName};`;
        const connectionStringSecret = new secretsmanager.Secret(this, 'DbConnectionStringSecret', {
            secretStringValue: cdk.SecretValue.unsafePlainText(connectionString),
            description: 'MySQL connection string (dynamically constructed from RDS) ending with semicolon',
        });
        // Task Definition
        const taskDefinition = new ecs.FargateTaskDefinition(this, 'TaskDef', {
            cpu: 512,
            memoryLimitMiB: 1024,
        });
        // CloudWatch Log Group
        const logGroup = new logs.LogGroup(this, 'LogGroup', {
            retention: logs.RetentionDays.ONE_WEEK,
            removalPolicy: cdk.RemovalPolicy.DESTROY,
        });
        // Container
        // MySQL format: Server=<server>;Port=3306;Uid=<user>;Pwd=<password>;Database=<database>;
        const containerSecrets = {
            DB_PASSWORD: ecs.Secret.fromSecretsManager(dbCredentials, 'password'),
            ConnectionStrings__ApplicationDbContextConnection: ecs.Secret.fromSecretsManager(connectionStringSecret),
        };
        // Add StorageConnectionString if Publisher is deployed
        if (storageSecret) {
            containerSecrets.ConnectionStrings__StorageConnectionString =
                ecs.Secret.fromSecretsManager(storageSecret);
        }
        // Build environment variables for container
        const containerEnvironment = {
            CosmosAllowSetup: 'true',
            MultiTenantEditor: 'false',
            ASPNETCORE_ENVIRONMENT: 'Development',
            AdminEmail: 'admin@example.com',
            DB_HOST: database.dbInstanceEndpointAddress,
            DB_PORT: '3306',
            DB_NAME: dbName,
            DB_USER: 'admin',
        };
        // Add CosmosPublisherUrl if Publisher is deployed
        if (deployPublisher && publisherDistribution) {
            const publisherUrl = publisherDomainName
                ? `https://${publisherDomainName}`
                : `https://${publisherDistribution.distributionDomainName}`;
            containerEnvironment.CosmosPublisherUrl = publisherUrl;
        }
        const container = taskDefinition.addContainer('web', {
            image: ecs.ContainerImage.fromRegistry(props.image),
            portMappings: [{ containerPort: 8080 }],
            logging: ecs.LogDrivers.awsLogs({
                streamPrefix: 'SkyCMS',
                logGroup,
            }),
            environment: containerEnvironment,
            secrets: containerSecrets,
        });
        // Security Group
        const securityGroup = new ec2.SecurityGroup(this, 'ServiceSg', {
            vpc,
            description: 'ECS Service Security Group',
            allowAllOutbound: true,
        });
        securityGroup.addIngressRule(ec2.Peer.anyIpv4(), ec2.Port.tcp(80), 'Allow HTTP from anywhere');
        // Application Load Balancer
        const alb = new elbv2.ApplicationLoadBalancer(this, 'ALB', {
            vpc,
            internetFacing: true,
        });
        const albSg = alb.connections.securityGroups[0];
        const listener = alb.addListener('HttpListener', {
            port: 80,
            protocol: elbv2.ApplicationProtocol.HTTP,
        });
        // ECS Service
        const service = new ecs.FargateService(this, 'Service', {
            cluster,
            taskDefinition,
            desiredCount,
            assignPublicIp: true, // Required to access Secrets Manager without NAT Gateway
            securityGroups: [securityGroup],
            vpcSubnets: { subnetType: ec2.SubnetType.PUBLIC },
            healthCheckGracePeriod: cdk.Duration.seconds(120),
        });
        // Add service as target (HTTP listener)
        const targetGroup = listener.addTargets('EcsTarget', {
            port: 8080,
            targets: [service],
            healthCheck: {
                path: '/healthz',
                healthyHttpCodes: '200-399',
                timeout: cdk.Duration.seconds(5),
                interval: cdk.Duration.seconds(15),
                healthyThresholdCount: 2,
                unhealthyThresholdCount: 2,
            },
        });
        // Optional HTTPS listener: use provided certificateArn or auto-provision via Route 53 if domain/zone specified
        let httpsCertificate = undefined;
        if (certificateArn) {
            httpsCertificate = acm.Certificate.fromCertificateArn(this, 'AlbCert', certificateArn);
        }
        else if (domainName && (hostedZoneId && hostedZoneName)) {
            const zone = route53.HostedZone.fromHostedZoneAttributes(this, 'HostedZone', {
                hostedZoneId,
                zoneName: hostedZoneName,
            });
            httpsCertificate = new acm.DnsValidatedCertificate(this, 'AlbCertAuto', {
                domainName,
                hostedZone: zone,
                region: cdk.Aws.REGION,
            });
        }
        if (httpsCertificate) {
            alb.addListener('HttpsListener', {
                port: 443,
                protocol: elbv2.ApplicationProtocol.HTTPS,
                certificates: [httpsCertificate],
                defaultAction: elbv2.ListenerAction.forward([targetGroup]),
            });
            albSg.addIngressRule(ec2.Peer.anyIpv4(), ec2.Port.tcp(443), 'Allow HTTPS from anywhere');
        }
        // Allow ECS to connect to RDS
        dbSecurityGroup.addIngressRule(securityGroup, ec2.Port.tcp(3306), 'Allow MySQL from ECS');
        // Allow temporary access from anywhere for MySQL Workbench (dev only)
        dbSecurityGroup.addIngressRule(ec2.Peer.anyIpv4(), ec2.Port.tcp(3306), 'Temporary: Allow MySQL from anywhere for development');
        // Allow ALB to reach ECS tasks on port 8080
        securityGroup.addIngressRule(ec2.Peer.securityGroupId(albSg.securityGroupId), ec2.Port.tcp(8080), 'Allow HTTP from ALB to container port 8080');
        // CloudFront Distribution with HTTPS (auto-generated SSL certificate)
        // Create custom origin request policy to forward X-Forwarded-Proto and other headers for proxy awareness
        const originRequestPolicy = new cloudfront.OriginRequestPolicy(this, 'OriginRequestPolicy', {
            headerBehavior: cloudfront.OriginRequestHeaderBehavior.allowList('Host', 'CloudFront-Forwarded-Proto', 'User-Agent'),
            queryStringBehavior: cloudfront.OriginRequestQueryStringBehavior.all(),
            cookieBehavior: cloudfront.OriginRequestCookieBehavior.all(),
        });
        const distribution = new cloudfront.Distribution(this, 'CloudFrontDist', {
            defaultBehavior: {
                origin: new origins.LoadBalancerV2Origin(alb, {
                    protocolPolicy: (httpsCertificate || certificateArn)
                        ? cloudfront.OriginProtocolPolicy.HTTPS_ONLY
                        : cloudfront.OriginProtocolPolicy.HTTP_ONLY,
                    httpsPort: 443,
                    httpPort: 80,
                    originSslProtocols: [cloudfront.OriginSslPolicy.TLS_V1_2],
                }),
                allowedMethods: cloudfront.AllowedMethods.ALLOW_ALL,
                cachePolicy: cloudfront.CachePolicy.CACHING_DISABLED,
                originRequestPolicy: originRequestPolicy,
                viewerProtocolPolicy: cloudfront.ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
                compress: true,
            },
            enabled: true,
            comment: 'SkyCMS Editor Distribution',
        });
        // Outputs
        new cdk.CfnOutput(this, 'ClusterName', {
            value: cluster.clusterName,
            description: 'ECS Cluster Name',
        });
        new cdk.CfnOutput(this, 'ServiceName', {
            value: service.serviceName,
            description: 'ECS Service Name',
        });
        new cdk.CfnOutput(this, 'LogGroupName', {
            value: logGroup.logGroupName,
            description: 'CloudWatch Log Group',
        });
        new cdk.CfnOutput(this, 'Region', {
            value: cdk.Aws.REGION,
            description: 'AWS Region',
        });
        new cdk.CfnOutput(this, 'DatabaseEndpoint', {
            value: database.dbInstanceEndpointAddress,
            description: 'RDS MySQL Endpoint',
        });
        new cdk.CfnOutput(this, 'DatabaseName', {
            value: dbName,
            description: 'Database Name',
        });
        new cdk.CfnOutput(this, 'DatabaseCredentialsSecret', {
            value: dbCredentials.secretArn,
            description: 'Database Credentials Secret ARN',
        });
        new cdk.CfnOutput(this, 'ConnectionStringSecret', {
            value: connectionStringSecret.secretArn,
            description: 'MySQL Connection String Secret ARN (provided)',
        });
        const mysqlConnectionString = `Server=${database.dbInstanceEndpointAddress};Port=3306;Uid=admin;Pwd=${dbCredentials.secretValueFromJson('password').unsafeUnwrap()};Database=${dbName};`;
        new cdk.CfnOutput(this, 'MySqlConnectionString', {
            value: mysqlConnectionString,
            description: '✅ MySQL Connection String for validation and MySQL Workbench (dev-only)',
        });
        new cdk.CfnOutput(this, 'DbSecurityGroupId', {
            value: dbSecurityGroup.securityGroupId,
            description: 'RDS MySQL Security Group ID (for IP allow-listing)',
        });
        new cdk.CfnOutput(this, 'CloudFrontURL', {
            value: `https://${distribution.domainName}`,
            description: 'SkyCMS Editor URL (CloudFront with TLS)',
        });
        new cdk.CfnOutput(this, 'ALBDomainName', {
            value: alb.loadBalancerDnsName,
            description: 'ALB DNS Name (for debugging)',
        });
    }
}
exports.SkyCmsEditorStack = SkyCmsEditorStack;

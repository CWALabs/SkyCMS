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
class SkyCmsEditorStack extends cdk.Stack {
    constructor(scope, id, props) {
        super(scope, id, props);
        const desiredCount = props.desiredCount || 1;
        const dbName = props.dbName || 'skycms';
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
        // Container - connection string as environment variable with password from secret
        // MySQL format: Server=<host>;Database=<db>;Uid=<user>;Pwd=<password>;SslMode=Required
        const container = taskDefinition.addContainer('web', {
            image: ecs.ContainerImage.fromRegistry(props.image),
            portMappings: [{ containerPort: 80 }],
            logging: ecs.LogDrivers.awsLogs({
                streamPrefix: 'SkyCMS',
                logGroup,
            }),
            environment: {
                CosmosAllowSetup: 'true',
                MultiTenantEditor: 'false',
                ASPNETCORE_ENVIRONMENT: 'Production',
                DB_HOST: database.dbInstanceEndpointAddress,
                DB_PORT: '3306',
                DB_NAME: dbName,
                DB_USER: 'admin',
            },
            secrets: {
                DB_PASSWORD: ecs.Secret.fromSecretsManager(dbCredentials, 'password'),
            },
        });
        // Manually set the connection string environment variable that combines the parts
        // ECS will inject DB_PASSWORD secret before this runs
        container.addEnvironment('ConnectionStrings__ApplicationDbContextConnection', `Server=${database.dbInstanceEndpointAddress};Port=3306;Database=${dbName};Uid=admin;Pwd=${dbCredentials.secretValueFromJson('password').unsafeUnwrap()};SslMode=Required`);
        // Security Group
        const securityGroup = new ec2.SecurityGroup(this, 'ServiceSg', {
            vpc,
            description: 'ECS Service Security Group',
            allowAllOutbound: true,
        });
        securityGroup.addIngressRule(ec2.Peer.anyIpv4(), ec2.Port.tcp(80), 'Allow HTTP from anywhere');
        // ECS Service
        const service = new ecs.FargateService(this, 'Service', {
            cluster,
            taskDefinition,
            desiredCount,
            assignPublicIp: true,
            securityGroups: [securityGroup],
            vpcSubnets: { subnetType: ec2.SubnetType.PUBLIC },
        });
        // Allow ECS to connect to RDS
        dbSecurityGroup.addIngressRule(securityGroup, ec2.Port.tcp(3306), 'Allow MySQL from ECS');
        // Allow temporary access from anywhere for MySQL Workbench (dev only)
        dbSecurityGroup.addIngressRule(ec2.Peer.anyIpv4(), ec2.Port.tcp(3306), 'Temporary: Allow MySQL from anywhere for development');
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
    }
}
exports.SkyCmsEditorStack = SkyCmsEditorStack;

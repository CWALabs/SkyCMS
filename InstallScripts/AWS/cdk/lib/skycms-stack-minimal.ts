import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as ecs from 'aws-cdk-lib/aws-ecs';
import * as logs from 'aws-cdk-lib/aws-logs';
import * as rds from 'aws-cdk-lib/aws-rds';
import * as secretsmanager from 'aws-cdk-lib/aws-secretsmanager';
import * as elbv2 from 'aws-cdk-lib/aws-elasticloadbalancingv2';
import * as cloudfront from 'aws-cdk-lib/aws-cloudfront';
import * as origins from 'aws-cdk-lib/aws-cloudfront-origins';
import * as acm from 'aws-cdk-lib/aws-certificatemanager';

export interface SkyCmsProps extends cdk.StackProps {
  image: string;
  desiredCount?: number;
  dbName?: string;
}

export class SkyCmsEditorStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props: SkyCmsProps) {
    super(scope, id, props);

    const desiredCount = props.desiredCount || 1;
    const dbName = props.dbName || 'skycms';
    const certificateArn = this.node.tryGetContext('certificateArn') as string | undefined;

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
      instanceType: ec2.InstanceType.of(
        ec2.InstanceClass.T4G,
        ec2.InstanceSize.MICRO
      ),
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

    const connectionStringSecret = new secretsmanager.Secret(
      this,
      'DbConnectionStringSecret',
      {
        secretStringValue: cdk.SecretValue.unsafePlainText(
          'Server=cosmos-cms-mysql-dev.mysql.database.azure.com;Port=3306;Uid=toiyabe;Pwd=ga5H#7g7hQ@!vzCnq4Pb;Database=cosmoscms;'
        ),
        description: 'MySQL connection string (provided) ending with semicolon',
      }
    );

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
    const container = taskDefinition.addContainer('web', {
      image: ecs.ContainerImage.fromRegistry(props.image),
      portMappings: [{ containerPort: 8080 }],
      logging: ecs.LogDrivers.awsLogs({
        streamPrefix: 'SkyCMS',
        logGroup,
      }),
      environment: {
        CosmosAllowSetup: 'true',
        MultiTenantEditor: 'false',
        ASPNETCORE_ENVIRONMENT: 'Production',
        AdminEmail: 'admin@example.com',
        DB_HOST: database.dbInstanceEndpointAddress,
        DB_PORT: '3306',
        DB_NAME: dbName,
        DB_USER: 'admin',
      },
      secrets: {
        DB_PASSWORD: ecs.Secret.fromSecretsManager(dbCredentials, 'password'),
        ConnectionStrings__ApplicationDbContextConnection:
          ecs.Secret.fromSecretsManager(connectionStringSecret),
      },
    });

    // Security Group
    const securityGroup = new ec2.SecurityGroup(this, 'ServiceSg', {
      vpc,
      description: 'ECS Service Security Group',
      allowAllOutbound: true,
    });

    securityGroup.addIngressRule(
      ec2.Peer.anyIpv4(),
      ec2.Port.tcp(80),
      'Allow HTTP from anywhere'
    );

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
      assignPublicIp: true,  // Required to access Secrets Manager without NAT Gateway
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

    // Optional HTTPS listener if a certificate ARN is provided via context: certificateArn
    if (certificateArn) {
      const certificate = acm.Certificate.fromCertificateArn(this, 'AlbCert', certificateArn);
      alb.addListener('HttpsListener', {
        port: 443,
        protocol: elbv2.ApplicationProtocol.HTTPS,
        certificates: [certificate],
        defaultAction: elbv2.ListenerAction.forward([targetGroup]),
      });

      albSg.addIngressRule(ec2.Peer.anyIpv4(), ec2.Port.tcp(443), 'Allow HTTPS from anywhere');
    }

    // Allow ECS to connect to RDS
    dbSecurityGroup.addIngressRule(
      securityGroup,
      ec2.Port.tcp(3306),
      'Allow MySQL from ECS'
    );

    // Allow temporary access from anywhere for MySQL Workbench (dev only)
    dbSecurityGroup.addIngressRule(
      ec2.Peer.anyIpv4(),
      ec2.Port.tcp(3306),
      'Temporary: Allow MySQL from anywhere for development'
    );

    // Allow ALB to reach ECS tasks on port 8080
    securityGroup.addIngressRule(
      ec2.Peer.securityGroupId(albSg.securityGroupId),
      ec2.Port.tcp(8080),
      'Allow HTTP from ALB to container port 8080'
    );

    // CloudFront Distribution with HTTPS (auto-generated SSL certificate)
    const distribution = new cloudfront.Distribution(this, 'CloudFrontDist', {
      defaultBehavior: {
        origin: new origins.LoadBalancerV2Origin(alb, {
          protocolPolicy: certificateArn
            ? cloudfront.OriginProtocolPolicy.HTTPS_ONLY
            : cloudfront.OriginProtocolPolicy.HTTP_ONLY,
          httpsPort: 443,
          httpPort: 80,
          originSslProtocols: [cloudfront.OriginSslPolicy.TLS_V1_2],
        }),
        allowedMethods: cloudfront.AllowedMethods.ALLOW_ALL,
        cachePolicy: cloudfront.CachePolicy.CACHING_DISABLED,
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

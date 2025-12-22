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
import * as route53 from 'aws-cdk-lib/aws-route53';
import * as s3 from 'aws-cdk-lib/aws-s3';
import * as iam from 'aws-cdk-lib/aws-iam';
import { AccessKeyGenerator } from './access-key-generator';

export interface SkyCmsProps extends cdk.StackProps {
  image: string;
  desiredCount?: number;
  dbName?: string;
  deployPublisher?: boolean;
}

export class SkyCmsEditorStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props: SkyCmsProps) {
    super(scope, id, props);

    const desiredCount = props.desiredCount || 1;
    const dbName = props.dbName || 'skycms';
    const deployPublisher = props.deployPublisher !== undefined ? props.deployPublisher : false;
    const certificateArn = this.node.tryGetContext('certificateArn') as string | undefined;
    const domainName = this.node.tryGetContext('domainName') as string | undefined;
    const hostedZoneId = this.node.tryGetContext('hostedZoneId') as string | undefined;
    const hostedZoneName = this.node.tryGetContext('hostedZoneName') as string | undefined;
    const publisherDomainName = this.node.tryGetContext('publisherDomainName') as string | undefined;
    const publisherCertificateArn = this.node.tryGetContext('publisherCertificateArn') as string | undefined;
    // Optional CDN caching controls
    const editorCacheEnabled = String(this.node.tryGetContext('editorCacheEnabled') ?? 'false') === 'true';
    const publisherCacheEnabled = String(this.node.tryGetContext('publisherCacheEnabled') ?? 'true') === 'true';

    // ============================================
    // PUBLISHER RESOURCES (S3 + CloudFront)
    // ============================================

    let storageSecret: secretsmanager.ISecret | undefined;
    let publisherBucket: s3.Bucket | undefined;
    let publisherDistribution: cloudfront.Distribution | undefined;
    let cloudFrontSecret: secretsmanager.ISecret | undefined;

    if (deployPublisher) {
      // S3 Bucket for static website
      publisherBucket = new s3.Bucket(this, 'PublisherBucket', {
        // Use private bucket with CloudFront OAI/OAC; no website hosting
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
      // Use S3BucketOrigin with Origin Access Identity (OAI)
      publisherDistribution = new cloudfront.Distribution(this, 'PublisherDistribution', {
        defaultBehavior: {
          origin: origins.S3BucketOrigin.withOriginAccessIdentity(publisherBucket, {
            originAccessIdentity: oai,
          }),
          viewerProtocolPolicy: cloudfront.ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
          allowedMethods: cloudfront.AllowedMethods.ALLOW_GET_HEAD,
          cachedMethods: cloudfront.CachedMethods.CACHE_GET_HEAD,
          compress: true,
          cachePolicy: publisherCacheEnabled
            ? cloudfront.CachePolicy.CACHING_OPTIMIZED
            : cloudfront.CachePolicy.CACHING_DISABLED,
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
      s3User.addToPolicy(
        new iam.PolicyStatement({
          actions: ['s3:GetObject', 's3:PutObject', 's3:DeleteObject', 's3:ListBucket'],
          resources: [publisherBucket.bucketArn, `${publisherBucket.bucketArn}/*`],
        })
      );

      // Grant CloudFront invalidation permission (will be set after distribution is created)
      // Note: Permission is added after distribution creation below

      // Generate access keys using custom resource
      const accessKeys = new AccessKeyGenerator(this, 'AccessKeys', {
        userName: s3User.userName,
      });

      // Create storage connection string and store in Secrets Manager
      const connectionString = `Bucket=${publisherBucket.bucketName};Region=${this.region};KeyId=${accessKeys.accessKeyId};Key=${accessKeys.secretAccessKey};`;
      
      storageSecret = new secretsmanager.Secret(this, 'StorageSecret', {
        secretName: `SkyCms-StorageConnectionString-${this.stackName}`,
        description: 'S3 storage connection string for SkyCMS',
        secretStringValue: cdk.SecretValue.unsafePlainText(connectionString),
      });

      // Create CloudFront CDN configuration secret for Editor
      const cloudFrontConfig = {
        CdnProvider: 'CloudFront',
        AccessKeyId: accessKeys.accessKeyId,
        SecretAccessKey: accessKeys.secretAccessKey,
        DistributionId: publisherDistribution.distributionId,
        Region: this.region,
      };

      cloudFrontSecret = new secretsmanager.Secret(this, 'CloudFrontSecret', {
        secretName: `SkyCms-CloudFrontConfig-${this.stackName}`,
        description: 'CloudFront CDN configuration for SkyCMS Publisher',
        secretStringValue: cdk.SecretValue.unsafePlainText(JSON.stringify(cloudFrontConfig)),
      });

      // Grant CloudFront invalidation permission to S3 user
      s3User.addToPolicy(
        new iam.PolicyStatement({
          effect: iam.Effect.ALLOW,
          actions: ['cloudfront:CreateInvalidation', 'cloudfront:GetInvalidation'],
          resources: [`arn:aws:cloudfront::${this.account}:distribution/${publisherDistribution.distributionId}`],
        })
      );

      // Outputs for Publisher
      new cdk.CfnOutput(this, 'PublisherBucketName', {
        value: publisherBucket.bucketName,
        description: 'S3 bucket for Publisher static files',
      });

      new cdk.CfnOutput(this, 'PublisherCloudFrontURL', {
        value: `https://${publisherDistribution.distributionDomainName}`,
        description: 'Publisher CloudFront distribution URL',
      });

      new cdk.CfnOutput(this, 'CloudFrontConfigSecret', {
        value: cloudFrontSecret.secretArn,
        description: 'CloudFront CDN configuration secret ARN',
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

    // Construct connection string from RDS database endpoint and credentials
    const dbUser = dbCredentials.secretValueFromJson('username').toString();
    const dbPassword = dbCredentials.secretValueFromJson('password').toString();
    const connectionString = `Server=${database.dbInstanceEndpointAddress};Port=3306;Uid=${dbUser};Pwd=${dbPassword};Database=${dbName};`;

    const connectionStringSecret = new secretsmanager.Secret(
      this,
      'DbConnectionStringSecret',
      {
        secretStringValue: cdk.SecretValue.unsafePlainText(connectionString),
        description: 'MySQL connection string (dynamically constructed from RDS) ending with semicolon',
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
    const containerSecrets: { [key: string]: ecs.Secret } = {
      DB_PASSWORD: ecs.Secret.fromSecretsManager(dbCredentials, 'password'),
      ConnectionStrings__ApplicationDbContextConnection:
        ecs.Secret.fromSecretsManager(connectionStringSecret),
    };

    // Add StorageConnectionString if Publisher is deployed
    if (storageSecret) {
      containerSecrets.ConnectionStrings__StorageConnectionString = 
        ecs.Secret.fromSecretsManager(storageSecret);
    }

    // Build environment variables for container
    const containerEnvironment: { [key: string]: string } = {
      CosmosAllowSetup: 'true',
      MultiTenantEditor: 'false',
      ASPNETCORE_ENVIRONMENT: 'Development',
      AdminEmail: 'admin@example.com',
      DB_HOST: database.dbInstanceEndpointAddress,
      DB_PORT: '3306',
      DB_NAME: dbName,
      DB_USER: 'admin',
    };

    // Add CosmosPublisherUrl and CloudFront config if Publisher is deployed
    if (deployPublisher && publisherDistribution) {
      const publisherUrl = publisherDomainName 
        ? `https://${publisherDomainName}`
        : `https://${publisherDistribution.distributionDomainName}`;
      containerEnvironment.CosmosPublisherUrl = publisherUrl;
      
      // Store the CloudFront secret ARN as an environment variable for the startup service to read
      if (cloudFrontSecret) {
        containerEnvironment.CloudFrontConfigSecretArn = cloudFrontSecret.secretArn;
        // Grant task role permission to read CloudFront secret
        cloudFrontSecret.grantRead(taskDefinition.taskRole);
      }
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
      minHealthyPercent: 100,
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

    // Optional HTTPS listener: use provided certificateArn or auto-provision via Route 53 if domain/zone specified
    let httpsCertificate: acm.ICertificate | undefined = undefined;
    if (certificateArn) {
      httpsCertificate = acm.Certificate.fromCertificateArn(this, 'AlbCert', certificateArn);
    } else if (domainName && (hostedZoneId && hostedZoneName)) {
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
    // Create custom origin request policy to forward X-Forwarded-Proto and other headers for proxy awareness
    const originRequestPolicy = new cloudfront.OriginRequestPolicy(this, 'OriginRequestPolicy', {
      headerBehavior: cloudfront.OriginRequestHeaderBehavior.allowList(
        'Host',
        'CloudFront-Forwarded-Proto',
        'User-Agent'
      ),
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
        cachePolicy: editorCacheEnabled
          ? cloudfront.CachePolicy.CACHING_OPTIMIZED
          : cloudfront.CachePolicy.CACHING_DISABLED,
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

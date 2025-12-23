import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as ecs from 'aws-cdk-lib/aws-ecs';
import * as ecs_patterns from 'aws-cdk-lib/aws-ecs-patterns';
import * as rds from 'aws-cdk-lib/aws-rds';
import * as secretsmanager from 'aws-cdk-lib/aws-secretsmanager';
import * as cloudfront from 'aws-cdk-lib/aws-cloudfront';
import * as origins from 'aws-cdk-lib/aws-cloudfront-origins';
import * as acm from 'aws-cdk-lib/aws-certificatemanager';
import * as route53 from 'aws-cdk-lib/aws-route53';
import { ApplicationProtocol } from 'aws-cdk-lib/aws-elasticloadbalancingv2';

export interface SkyCmsProps extends cdk.StackProps {
  image: string;
  bucketName: string;
  dbName: string;
  desiredCount: number;
}

export class SkyCmsEditorStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props: SkyCmsProps) {
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

    // Generate a managed secret for the DB admin credentials (avoid fixed passwords)
    const dbCredentialsSecret = new secretsmanager.Secret(this, 'DbAdminCredentials', {
      generateSecretString: {
        secretStringTemplate: JSON.stringify({ username: 'skycms_admin' }),
        generateStringKey: 'password',
        excludePunctuation: true,
        passwordLength: 20,
      },
    });

    const certificateArn = this.node.tryGetContext('certificateArn') as string | undefined;
    const domainName = this.node.tryGetContext('domainName') as string | undefined;
    const hostedZoneId = this.node.tryGetContext('hostedZoneId') as string | undefined;
    const hostedZoneName = this.node.tryGetContext('hostedZoneName') as string | undefined;
    let certificate: acm.ICertificate | undefined = undefined;
    if (certificateArn) {
      certificate = acm.Certificate.fromCertificateArn(this, 'AlbCert', certificateArn);
    } else if (domainName && (hostedZoneId && hostedZoneName)) {
      const zone = route53.HostedZone.fromHostedZoneAttributes(this, 'HostedZone', {
        hostedZoneId,
        zoneName: hostedZoneName,
      });
      certificate = new acm.DnsValidatedCertificate(this, 'AlbCertAuto', {
        domainName,
        hostedZone: zone,
        region: cdk.Aws.REGION,
      });
    }
    
    const db = new rds.DatabaseInstance(this, 'MySql', {
      engine: rds.DatabaseInstanceEngine.mysql({ version: rds.MysqlEngineVersion.VER_8_0 }),
      vpc,
      vpcSubnets: { subnetType: ec2.SubnetType.PRIVATE_ISOLATED },
      instanceType: ec2.InstanceType.of(ec2.InstanceClass.BURSTABLE4_GRAVITON, ec2.InstanceSize.MICRO),
      allocatedStorage: 20,
      storageType: rds.StorageType.GP3,
      securityGroups: [dbSg],
      credentials: rds.Credentials.fromSecret(dbCredentialsSecret),
      databaseName: props.dbName,
      deletionProtection: false,
      removalPolicy: cdk.RemovalPolicy.DESTROY, // dev convenience
      backupRetention: cdk.Duration.days(7),
      publiclyAccessible: false,
      parameterGroup: parameterGroup,
    });

    // Construct connection string using the generated DB password (resolved at deploy/runtime)
    const connectionString = cdk.Fn.sub(
      'Server=${DbHost};Port=3306;Uid=skycms_admin;Pwd=${DbPassword};Database=${DbName};',
      {
        DbHost: db.dbInstanceEndpointAddress,
        DbPassword: cdk.Token.asString(dbCredentialsSecret.secretValueFromJson('password')),
        DbName: props.dbName,
      }
    );

    const connectionStringSecret = new secretsmanager.Secret(
      this,
      'DbConnectionStringSecret',
      {
        secretStringValue: cdk.SecretValue.unsafePlainText(connectionString),
        description: 'MySQL connection string (dynamically constructed from RDS, password resolved from secret) ending with semicolon',
      }
    );

    // Base environment and secrets for the editor task definition
    const editorEnv: Record<string, string> = {
      CosmosAllowSetup: 'true',
      MultiTenantEditor: 'false',
      ASPNETCORE_ENVIRONMENT: 'Development',
      BlobServiceProvider: 'Amazon',
      AmazonS3BucketName: props.bucketName || '',
      AmazonS3Region: cdk.Aws.REGION,
      SKYCMS_DB_HOST: db.instanceEndpoint.hostname,
      SKYCMS_DB_USER: 'skycms_admin',
      SKYCMS_DB_NAME: props.dbName,
      SKYCMS_DB_SSL: 'true',
      SKYCMS_DB_SSL_MODE: 'Required',
    };

    const editorSecrets: Record<string, ecs.Secret> = {
      ConnectionStrings__ApplicationDbContextConnection:
        ecs.Secret.fromSecretsManager(connectionStringSecret),
    };

    // Optional: Amazon SES SMTP configuration (password supplied via Secrets Manager)
    const sesEnabled = this.node.tryGetContext('sesEnabled') === 'true';
    const sesSmtpHost =
      (this.node.tryGetContext('sesSmtpHost') as string | undefined) ??
      `email-smtp.${cdk.Aws.REGION}.amazonaws.com`;
    const sesSmtpPort =
      (this.node.tryGetContext('sesSmtpPort') as string | undefined) ?? '587';
    const sesSmtpUsername = this.node.tryGetContext('sesSmtpUsername') as string | undefined;
    const sesSmtpPasswordSecretArn = this.node.tryGetContext('sesSmtpPasswordSecretArn') as string | undefined;
    const sesSenderEmail = this.node.tryGetContext('sesSenderEmail') as string | undefined;

    if (sesEnabled && sesSmtpUsername && sesSmtpPasswordSecretArn) {
      const sesPasswordSecret = secretsmanager.Secret.fromSecretCompleteArn(
        this,
        'SesSmtpPasswordSecret',
        sesSmtpPasswordSecretArn
      );

      editorEnv.SmtpEmailProviderOptions__Host = sesSmtpHost;
      editorEnv.SmtpEmailProviderOptions__Port = sesSmtpPort;
      editorEnv.SmtpEmailProviderOptions__UserName = sesSmtpUsername;
      editorEnv.SmtpEmailProviderOptions__UsesSsl = 'false'; // SES uses STARTTLS on 587
      if (sesSenderEmail) {
        editorEnv.AdminEmail = sesSenderEmail;
      }

      editorSecrets.SmtpEmailProviderOptions__Password = ecs.Secret.fromSecretsManager(
        sesPasswordSecret
      );
    }

    // Fargate Service with ALB
    const fargate = new ecs_patterns.ApplicationLoadBalancedFargateService(this, 'EditorService', {
      cluster,
      assignPublicIp: true, // allow internet egress without NAT
      cpu: 512,
      memoryLimitMiB: 1024,
      desiredCount: props.desiredCount ?? 1,
      protocol: certificate ? ApplicationProtocol.HTTPS : ApplicationProtocol.HTTP,
      certificate,
      redirectHTTP: certificate ? true : undefined,
      taskImageOptions: {
        image: ecs.ContainerImage.fromRegistry(props.image),
        containerPort: 80,
        environment: editorEnv,
        secrets: editorSecrets,
      },
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
      fargate.taskDefinition.taskRole?.addToPrincipalPolicy(
        new cdk.aws_iam.PolicyStatement({
          effect: cdk.aws_iam.Effect.ALLOW,
          actions: ['s3:GetObject', 's3:PutObject', 's3:DeleteObject', 's3:ListBucket'],
          resources: [s3Arn, `${s3Arn}/*`],
        })
      );
    }

    // CloudFront in front of ALB with no caching
    const dist = new cloudfront.Distribution(this, 'EditorCdn', {
      defaultBehavior: {
        origin: new origins.LoadBalancerV2Origin(fargate.loadBalancer, {
          protocolPolicy: certificate
            ? cloudfront.OriginProtocolPolicy.HTTPS_ONLY
            : cloudfront.OriginProtocolPolicy.HTTP_ONLY,
          httpsPort: 443,
          httpPort: 80,
          originSslProtocols: [cloudfront.OriginSslPolicy.TLS_V1_2],
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

    new cdk.CfnOutput(this, 'DatabaseName', {
      value: props.dbName,
      description: 'Default database created in RDS',
    });

    new cdk.CfnOutput(this, 'ConnectionStringSecret', {
      value: connectionStringSecret.secretArn,
      description: '✅ MySQL Connection String Secret ARN (provided)',
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

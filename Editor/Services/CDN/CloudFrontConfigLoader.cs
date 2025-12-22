// <copyright file="CloudFrontConfigLoader.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.CDN
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.SecretsManager;
    using Amazon.SecretsManager.Model;
    using Cosmos.Common.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Loads CloudFront CDN configuration from AWS Secrets Manager on startup.
    /// </summary>
    public class CloudFrontConfigLoader
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<CloudFrontConfigLoader> logger;
        private readonly ApplicationDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudFrontConfigLoader"/> class.
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="logger">Logger instance.</param>
        /// <param name="dbContext">Application database context.</param>
        public CloudFrontConfigLoader(
            IConfiguration configuration,
            ILogger<CloudFrontConfigLoader> logger,
            ApplicationDbContext dbContext)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// Loads CloudFront configuration from AWS Secrets Manager and pre-populates CDN settings in the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task LoadConfigurationAsync()
        {
            try
            {
                // Get the CloudFront config secret ARN from environment variable (set by CDK)
                var secretArn = configuration.GetValue<string>("CloudFrontConfigSecretArn");

                if (string.IsNullOrWhiteSpace(secretArn))
                {
                    logger.LogInformation("CloudFrontConfigSecretArn not configured. Skipping CloudFront auto-configuration.");
                    return;
                }

                // Ensure database is accessible before proceeding
                if (!await dbContext.Database.CanConnectAsync())
                {
                    logger.LogWarning("Database not accessible. Skipping CloudFront auto-configuration.");
                    return;
                }

                // Check if CloudFront CDN is already configured in the database
                // Look for a CloudFront-specific setting, not just any CDN
                var existingConfig = await dbContext.Settings
                    .Where(s => s.Group == CdnService.CDNGROUPNAME && s.Name == "CloudFront")
                    .FirstOrDefaultAsync();

                if (existingConfig != null)
                {
                    logger.LogInformation("CloudFront CDN already configured in database. Skipping auto-configuration.");
                    return;
                }

                // Extract region from ARN (format: arn:aws:secretsmanager:region:account:secret:name)
                var arnParts = secretArn.Split(':');
                if (arnParts.Length < 6)
                {
                    logger.LogWarning("Invalid CloudFront secret ARN format: {SecretArn}", secretArn);
                    return;
                }

                var region = arnParts[3]; // Region is at index 3

                // Retrieve the secret from AWS Secrets Manager
                using var client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));
                var request = new GetSecretValueRequest
                {
                    SecretId = secretArn,
                };

                var response = await client.GetSecretValueAsync(request);
                var secretString = response.SecretString;

                if (string.IsNullOrWhiteSpace(secretString))
                {
                    logger.LogWarning("CloudFront secret is empty.");
                    return;
                }

                // Deserialize the CloudFront config
                var cloudFrontConfig = JsonConvert.DeserializeObject<CloudFrontSecretConfig>(secretString);

                if (cloudFrontConfig == null)
                {
                    logger.LogWarning("Failed to deserialize CloudFront config.");
                    return;
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(cloudFrontConfig.AccessKeyId) ||
                    string.IsNullOrWhiteSpace(cloudFrontConfig.SecretAccessKey) ||
                    string.IsNullOrWhiteSpace(cloudFrontConfig.DistributionId) ||
                    string.IsNullOrWhiteSpace(cloudFrontConfig.Region))
                {
                    logger.LogWarning("CloudFront config is missing required fields (AccessKeyId, SecretAccessKey, DistributionId, or Region).");
                    return;
                }

                // Create CdnSetting object with CloudFrontCdnConfig serialized to Value
                var cloudFrontCdnConfig = new CloudFrontCdnConfig
                {
                    AccessKeyId = cloudFrontConfig.AccessKeyId,
                    SecretAccessKey = cloudFrontConfig.SecretAccessKey,
                    DistributionId = cloudFrontConfig.DistributionId,
                    Region = cloudFrontConfig.Region,
                };

                // Serialize CloudFrontCdnConfig to JSON for storage in Value field
                var configJson = JsonConvert.SerializeObject(cloudFrontCdnConfig);

                var cdnSetting = new CdnSetting
                {
                    CdnProvider = CdnProviderEnum.CloudFront,
                    Value = configJson,
                };

                // Serialize CdnSetting and store in database
                var settingJson = JsonConvert.SerializeObject(cdnSetting);
                var setting = new Setting
                {
                    Group = CdnService.CDNGROUPNAME,
                    Name = "CloudFront",
                    Value = settingJson,
                    IsRequired = false, // Not required - can be removed/reconfigured
                    Description = "CloudFront CDN configuration (auto-configured from AWS deployment)",
                };

                dbContext.Settings.Add(setting);
                await dbContext.SaveChangesAsync();

                logger.LogInformation("CloudFront CDN configuration loaded and saved to database successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading CloudFront configuration from Secrets Manager.");
            }
        }

        /// <summary>
        /// Secret format from AWS Secrets Manager.
        /// </summary>
        private class CloudFrontSecretConfig
        {
            /// <summary>
            /// Gets or sets the CDN provider name.
            /// </summary>
            public string CdnProvider { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the AWS access key ID.
            /// </summary>
            public string AccessKeyId { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the AWS secret access key.
            /// </summary>
            public string SecretAccessKey { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the CloudFront distribution ID.
            /// </summary>
            public string DistributionId { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the AWS region.
            /// </summary>
            public string Region { get; set; } = string.Empty;
        }
    }
}

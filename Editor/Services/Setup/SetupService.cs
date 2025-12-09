// <copyright file="SetupService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Setup
{
    using Cosmos.BlobService;
    using Cosmos.Cms.Data;
    using Cosmos.Common.Data;
    using Cosmos.Common.Data.Logic;
    using Cosmos.Editor.Services;
    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Sky.Editor.Data;
    using Sky.Editor.Data.Logic;
    using Sky.Editor.Features.Articles.Save;
    using Sky.Editor.Features.Shared;
    using Sky.Editor.Services.Layouts;
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Mail;
    using System.Threading.Tasks;

    /// <summary>
    /// Service for setup wizard operations.
    /// </summary>
    public class SetupService : ISetupService
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<SetupService> logger;
        private readonly IMemoryCache memoryCache;
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly string setupDbPath;
        private readonly ILayoutImportService layoutImportService;
        private readonly IMediator mediator;
        private readonly ArticleEditLogic articleLogic;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupService"/> class.
        /// </summary>
        /// <param name="configuration">Configuration.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="memoryCache">Memory cache.</param>
        /// <param name="userManager">User manager.</param>
        /// <param name="roleManager">Role manager.</param>
        /// <param name="layoutImportService">Layout import service.</param>
        /// <param name="articleLogic">Article edit logic.</param>
        /// <param name="mediator">Mediator service.</param>
        public SetupService(
    IConfiguration configuration,
    ILogger<SetupService> logger,
    IMemoryCache memoryCache,
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ILayoutImportService layoutImportService,
    ArticleEditLogic articleLogic,
    IMediator mediator)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.memoryCache = memoryCache;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.layoutImportService = layoutImportService;
            this.articleLogic = articleLogic;
            this.mediator = mediator;

            // ✅ SUPPORT CUSTOM PATH FOR TESTING
            setupDbPath = configuration["SetupDatabasePath"]
        ?? Path.Combine(Path.GetTempPath(), "skycms-setup.db");
        }

        /// <inheritdoc/>
        public async Task<SetupConfiguration> InitializeSetupAsync()
        {
            try
            {
                using var context = CreateSetupContext();
                await context.Database.EnsureCreatedAsync();

                // Check if setup already in progress
                var existing = await context.SetupConfigurations
                    .Where(s => s.IsComplete == false)
                    .OrderByDescending(s => s.CreatedAt) // ✅ FIXED
                    .FirstOrDefaultAsync();

                if (existing != null)
                {
                    logger.LogInformation("Resuming existing setup session {SetupId}", existing.Id);
                    return existing;
                }

                // Create new setup session
                var config = new SetupConfiguration
                {
                    Id = Guid.NewGuid(),
                    TenantMode = "SingleTenant",
                    CreatedAt = DateTime.UtcNow,
                    CurrentStep = 1
                };

                context.SetupConfigurations.Add(config);
                await context.SaveChangesAsync();

                logger.LogInformation("Created new setup session {SetupId}", config.Id);
                return config;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize setup");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<SetupConfiguration> GetCurrentSetupAsync()
        {
            try
            {
                using var context = CreateSetupContext();

                var config = await context.SetupConfigurations
                    .Where(s => !s.IsComplete)
                    .OrderByDescending(s => s.CreatedAt) // ✅ FIXED
                    .FirstOrDefaultAsync();

                return config;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get current setup");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task UpdateTenantModeAsync(Guid setupId, string tenantMode)
        {
            try
            {
                using var context = CreateSetupContext();
                var config = await context.SetupConfigurations.FindAsync(setupId);

                if (config == null)
                {
                    throw new InvalidOperationException($"Setup configuration {setupId} not found");
                }

                config.TenantMode = tenantMode;
                await context.SaveChangesAsync();

                logger.LogInformation("Updated tenant mode to {TenantMode} for setup {SetupId}", tenantMode, setupId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update tenant mode");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<TestResult> TestDatabaseConnectionAsync(string connectionString)
        {
            try
            {
                // Test connection by creating a temporary context
                using var context = new ApplicationDbContext(connectionString);
                var canConnect = await context.Database.CanConnectAsync();

                if (!canConnect)
                {
                    return new TestResult
                    {
                        Success = false,
                        Message = "Unable to connect to database"
                    };
                }

                // For Cosmos DB, check if database exists
                if (context.Database.IsCosmos())
                {
                    var dbStatus = ApplicationDbContext.EnsureDatabaseExists(connectionString);

                    return new TestResult
                    {
                        Success = true,
                        Message = $"Database connection successful. Status: {dbStatus}"
                    };
                }

                // For relational databases
                return new TestResult
                {
                    Success = true,
                    Message = "Database connection successful"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database connection test failed");
                return new TestResult
                {
                    Success = false,
                    Message = $"Connection failed: {ex.Message}"
                };
            }
        }

        /// <inheritdoc/>
        public async Task UpdateDatabaseConfigAsync(Guid setupId, string connectionString)
        {
            try
            {
                using var context = CreateSetupContext();
                var config = await context.SetupConfigurations.FindAsync(setupId);

                if (config == null)
                {
                    throw new InvalidOperationException($"Setup configuration {setupId} not found");
                }

                config.DatabaseConnectionString = connectionString;
                await context.SaveChangesAsync();

                logger.LogInformation("Updated database configuration for setup {SetupId}", setupId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update database configuration");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<TestResult> TestStorageConnectionAsync(string connectionString)
        {
            try
            {
                var storageContext = new StorageContext(connectionString, memoryCache);

                // Test by listing root directory
                var result = await storageContext.GetFilesAndDirectories("/");

                if (result == null)
                {
                    return new TestResult
                    {
                        Success = false,
                        Message = "Unable to connect to storage"
                    };
                }

                return new TestResult
                {
                    Success = true,
                    Message = $"Storage connection successful. Found {result.Count} items in root."
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Storage connection test failed");
                return new TestResult
                {
                    Success = false,
                    Message = $"Connection failed: {ex.Message}"
                };
            }
        }

        /// <inheritdoc/>
        public async Task UpdateStorageConfigAsync(Guid setupId, string storageConnectionString, string blobPublicUrl)
        {
            try
            {
                using var context = CreateSetupContext();
                var config = await context.SetupConfigurations.FindAsync(setupId);

                if (config == null)
                {
                    throw new InvalidOperationException($"Setup configuration {setupId} not found");
                }

                config.StorageConnectionString = storageConnectionString;
                config.BlobPublicUrl = blobPublicUrl;
                await context.SaveChangesAsync();

                logger.LogInformation("Updated storage configuration for setup {SetupId}", setupId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update storage configuration");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task UpdateAdminAccountAsync(Guid setupId, string email, string password)
        {
            try
            {
                using var context = CreateSetupContext();
                var config = await context.SetupConfigurations.FindAsync(setupId);

                if (config == null)
                {
                    throw new InvalidOperationException($"Setup configuration {setupId} not found");
                }

                config.AdminEmail = email;
                config.AdminPassword = password; // Will be hashed during completion
                await context.SaveChangesAsync();

                logger.LogInformation("Updated admin account for setup {SetupId}", setupId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update admin account");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task UpdatePublisherConfigAsync(
            Guid setupId,
            string publisherUrl,
            bool staticWebPages,
            bool requiresAuthentication,
            string allowedFileTypes,
            string microsoftAppId,
            string siteDesignId,
            string title)
        {
            try
            {
                using var context = CreateSetupContext();
                var config = await context.SetupConfigurations.FindAsync(setupId);

                if (config == null)
                {
                    throw new InvalidOperationException($"Setup configuration {setupId} not found");
                }

                config.PublisherUrl = publisherUrl;
                config.StaticWebPages = staticWebPages;
                config.CosmosRequiresAuthentication = requiresAuthentication;
                config.AllowedFileTypes = allowedFileTypes;
                config.MicrosoftAppId = microsoftAppId;
                config.SiteDesignId = siteDesignId;
                config.WebsiteTitle = title;

                // If static mode, force BlobPublicUrl to "/"
                if (staticWebPages)
                {
                    config.BlobPublicUrl = "/";
                }

                await context.SaveChangesAsync();

                logger.LogInformation("Updated publisher configuration for setup {SetupId}", setupId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update publisher configuration");
                throw;
            }
        }

        /// <summary>
        /// Creates a setup database context.
        /// </summary>
        /// <returns>Setup database context.</returns>
        private SetupDbContext CreateSetupContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<SetupDbContext>();
            optionsBuilder.UseSqlite($"Data Source={setupDbPath}");
            return new SetupDbContext(optionsBuilder.Options);
        }

        /// <inheritdoc/>
        public async Task UpdateStepAsync(Guid setupId, int step)
        {
            try
            {
                using var context = CreateSetupContext();
                var config = await context.SetupConfigurations.FindAsync(setupId);

                if (config == null)
                {
                    throw new InvalidOperationException($"Setup configuration {setupId} not found");
                }

                config.CurrentStep = step;
                await context.SaveChangesAsync();

                logger.LogInformation("Updated current step to {Step} for setup {SetupId}", step, setupId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update step");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<TestResult> TestEmailConfigAsync(
            string provider,
            string sendGridApiKey,
            string azureConnectionString,
            string smtpHost,
            int smtpPort,
            string smtpUsername,
            string smtpPassword,
            string senderEmail,
            string testRecipient)
        {
            try
            {
                switch (provider)
                {
                    case "SendGrid":
                        return await TestSendGridAsync(sendGridApiKey, senderEmail, testRecipient);

                    case "AzureCommunication":
                        return await TestAzureEmailAsync(azureConnectionString, senderEmail, testRecipient);

                    case "SMTP":
                        return await TestSmtpAsync(smtpHost, smtpPort, smtpUsername, smtpPassword, senderEmail, testRecipient);

                    default:
                        return new TestResult
                        {
                            Success = false,
                            Message = "Unknown email provider"
                        };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Email configuration test failed");
                return new TestResult
                {
                    Success = false,
                    Message = $"Test failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Tests SendGrid configuration.
        /// </summary>
        private async Task<TestResult> TestSendGridAsync(string apiKey, string senderEmail, string recipient)
        {
            try
            {
                var client = new SendGrid.SendGridClient(apiKey);
                var from = new SendGrid.Helpers.Mail.EmailAddress(senderEmail, "SkyCMS Setup");
                var to = new SendGrid.Helpers.Mail.EmailAddress(recipient);
                var msg = SendGrid.Helpers.Mail.MailHelper.CreateSingleEmail(
                    from,
                    to,
                    "SkyCMS Setup Test Email",
                    "This is a test email from SkyCMS setup wizard.",
                    "<p>This is a test email from SkyCMS setup wizard.</p>");

                var response = await client.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                {
                    return new TestResult
                    {
                        Success = true,
                        Message = $"Test email sent successfully to {recipient}"
                    };
                }

                var body = await response.Body.ReadAsStringAsync();
                return new TestResult
                {
                    Success = false,
                    Message = $"SendGrid returned status {response.StatusCode}: {body}"
                };
            }
            catch (Exception ex)
            {
                return new TestResult
                {
                    Success = false,
                    Message = $"SendGrid test failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Tests Azure Communication Services email.
        /// </summary>
        private async Task<TestResult> TestAzureEmailAsync(string connectionString, string senderEmail, string recipient)
        {
            try
            {
                // Azure Communication Services email testing
                // Note: This requires Azure.Communication.Email package
                return new TestResult
                {
                    Success = true,
                    Message = "Azure Communication Services configuration saved (test email not implemented)"
                };
            }
            catch (Exception ex)
            {
                return new TestResult
                {
                    Success = false,
                    Message = $"Azure email test failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Tests SMTP configuration.
        /// </summary>
        private async Task<TestResult> TestSmtpAsync(
            string host,
            int port,
            string username,
            string password,
            string senderEmail,
            string recipient)
        {
            try
            {
                using var client = new SmtpClient(host, port);
                client.EnableSsl = port == 587 || port == 465;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(username, password);

                var message = new MailMessage(senderEmail, recipient)
                {
                    Subject = "SkyCMS Setup Test Email",
                    Body = "This is a test email from SkyCMS setup wizard.",
                    IsBodyHtml = false
                };

                await client.SendMailAsync(message);

                return new TestResult
                {
                    Success = true,
                    Message = $"Test email sent successfully to {recipient}"
                };
            }
            catch (Exception ex)
            {
                return new TestResult
                {
                    Success = false,
                    Message = $"SMTP test failed: {ex.Message}"
                };
            }
        }

        /// <inheritdoc/>
        public async Task UpdateEmailConfigAsync(
            Guid setupId,
            string provider,
            string sendGridApiKey,
            string azureConnectionString,
            string smtpHost,
            int smtpPort,
            string smtpUsername,
            string smtpPassword)
        {
            try
            {
                using var context = CreateSetupContext();
                var config = await context.SetupConfigurations.FindAsync(setupId);

                if (config == null)
                {
                    throw new InvalidOperationException($"Setup configuration {setupId} not found");
                }

                config.SendGridApiKey = sendGridApiKey;
                config.AzureEmailConnectionString = azureConnectionString;
                config.SmtpHost = smtpHost;
                config.SmtpPort = smtpPort;
                config.SmtpUsername = smtpUsername;
                config.SmtpPassword = smtpPassword;

                await context.SaveChangesAsync();

                logger.LogInformation("Updated email configuration for setup {SetupId}", setupId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update email configuration");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<SetupCompletionResult> CompleteSetupAsync(Guid setupId)
        {
            try
            {
                logger.LogInformation("Starting setup completion for {SetupId}", setupId);

                using var setupContext = CreateSetupContext();

                // Ensure the setup database exists before querying
                try
                {
                    await setupContext.Database.EnsureCreatedAsync();
                }
                catch (Exception dbEx)
                {
                    logger.LogError(dbEx, "Failed to ensure setup database exists");
                    return new SetupCompletionResult
                    {
                        Success = false,
                        Message = "Setup database could not be initialized"
                    };
                }

                var config = await setupContext.SetupConfigurations.FindAsync(setupId);

                if (config == null)
                {
                    return new SetupCompletionResult
                    {
                        Success = false,
                        Message = "Setup configuration not found"
                    };
                }

                // Validate all required fields
                var validationResult = ValidateSetupConfiguration(config);
                if (!validationResult.Success)
                {
                    return validationResult;
                }

                // Get the main database connection string
                var mainDbConnectionString = configuration.GetConnectionString("ApplicationDbContextConnection");

                if (string.IsNullOrEmpty(mainDbConnectionString))
                {
                    return new SetupCompletionResult
                    {
                        Success = false,
                        Message = "Main database connection string not found in configuration"
                    };
                }

                // Create main database context
                using var mainDbContext = new ApplicationDbContext(mainDbConnectionString);

                // Step 1: Ensure database exists and is initialized
                logger.LogInformation("Ensuring database exists and is initialized");
                await EnsureDatabaseInitializedAsync(mainDbContext);

                // Step 2: Create administrator account--if one does not exist yet.
                var adminAccounts = await userManager.GetUsersInRoleAsync(RequiredIdentityRoles.Administrators);

                if (!adminAccounts.Any())
                {
                    logger.LogInformation("Creating administrator account");
                    var adminResult = await CreateAdminAccountAsync(config);
                    if (!adminResult.Success)
                    {
                        return adminResult;
                    }
                }
                else
                {
                    logger.LogInformation("Administrator account already exists, skipping creation");
                }

                // Step 3: Save settings to main database
                logger.LogInformation("Saving settings to database");
                await SaveSettingsToDatabaseAsync(mainDbContext, config);

                // Step 4: Create default layout if none exists
                logger.LogInformation("Ensuring default layout exists");
                await EnsureDefaultLayoutAndHomePageExistsAsync(mainDbContext, config);

                // Step 5: Mark setup as complete
                config.IsComplete = true;
                config.CompletedAt = DateTime.UtcNow;

                // Clear sensitive data
                config.AdminPassword = null;
                config.SendGridApiKey = null;
                config.AzureEmailConnectionString = null;
                config.SmtpPassword = null;
                config.SmtpUsername = null;

                await setupContext.SaveChangesAsync();

                logger.LogInformation("Setup completed successfully for {SetupId}", setupId);

                // Step 6: Clean up setup database (optional - delete after successful completion)
                try
                {
                    if (File.Exists(setupDbPath))
                    {
                        File.Delete(setupDbPath);
                        logger.LogInformation("Setup database deleted successfully");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to delete setup database, but setup was successful");
                }

                return new SetupCompletionResult
                {
                    Success = true,
                    Message = "Setup completed successfully. Please restart the application for changes to take effect."
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to complete setup");
                return new SetupCompletionResult
                {
                    Success = false,
                    Message = $"Setup failed: {ex.Message}"
                };
            }
        }

        /// <inheritdoc/>
        public async Task UpdateCdnConfigAsync(
            Guid setupId,
            string azureSubscriptionId,
            string azureResourceGroup,
            string azureProfileName,
            string azureEndpointName,
            bool azureIsFrontDoor,
            string cloudflareApiToken,
            string cloudflareZoneId,
            string sucuriApiKey,
            string sucuriApiSecret)
        {
            try
            {
                using var context = CreateSetupContext();
                var config = await context.SetupConfigurations.FindAsync(setupId);

                if (config == null)
                {
                    throw new InvalidOperationException($"Setup configuration {setupId} not found");
                }

                // Update Azure CDN/Front Door settings
                config.AzureCdnSubscriptionId = azureSubscriptionId ?? string.Empty;
                config.AzureCdnResourceGroup = azureResourceGroup ?? string.Empty;
                config.AzureCdnProfileName = azureProfileName ?? string.Empty;
                config.AzureCdnEndpointName = azureEndpointName ?? string.Empty;
                config.AzureCdnIsFrontDoor = azureIsFrontDoor;

                // Update Cloudflare settings
                config.CloudflareApiToken = cloudflareApiToken ?? string.Empty;
                config.CloudflareZoneId = cloudflareZoneId ?? string.Empty;

                // Update Sucuri settings
                config.SucuriApiKey = sucuriApiKey ?? string.Empty;
                config.SucuriApiSecret = sucuriApiSecret ?? string.Empty;

                await context.SaveChangesAsync();

                logger.LogInformation("Updated CDN configuration for setup {SetupId}", setupId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update CDN configuration");
                throw;
            }
        }

        /// <summary>
        /// Validates setup configuration.
        /// </summary>
        private SetupCompletionResult ValidateSetupConfiguration(SetupConfiguration config)
        {
            if (string.IsNullOrEmpty(config.StorageConnectionString))
            {
                return new SetupCompletionResult
                {
                    Success = false,
                    Message = "Storage connection string is required"
                };
            }

            if (string.IsNullOrEmpty(config.AdminEmail))
            {
                return new SetupCompletionResult
                {
                    Success = false,
                    Message = "Administrator email is required"
                };
            }

            if (string.IsNullOrEmpty(config.AdminPassword))
            {
                return new SetupCompletionResult
                {
                    Success = false,
                    Message = "Administrator password is required"
                };
            }

            if (string.IsNullOrEmpty(config.PublisherUrl))
            {
                return new SetupCompletionResult
                {
                    Success = false,
                    Message = "Publisher URL is required"
                };
            }

            return new SetupCompletionResult { Success = true };
        }

        /// <summary>
        /// Ensures database is initialized.
        /// </summary>
        private async Task EnsureDatabaseInitializedAsync(ApplicationDbContext context)
        {
            if (context.Database.IsCosmos())
            {
                // For Cosmos DB, use EnsureCreated
                await context.Database.EnsureCreatedAsync();
            }
            else
            {
                // For relational databases, apply migrations or ensure created
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    await context.Database.MigrateAsync();
                }
                else
                {
                    await context.Database.EnsureCreatedAsync();
                }
            }

            logger.LogInformation("Database initialized successfully");
        }

        /// <summary>
        /// Creates the administrator account.
        /// </summary>
        private async Task<SetupCompletionResult> CreateAdminAccountAsync(SetupConfiguration config)
        {
            try
            {
                // Create user
                var user = new IdentityUser
                {
                    UserName = config.AdminEmail,
                    Email = config.AdminEmail,
                    EmailConfirmed = true // Auto-confirm for first admin
                };

                var result = await userManager.CreateAsync(user, config.AdminPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new SetupCompletionResult
                    {
                        Success = false,
                        Message = $"Failed to create admin account: {errors}"
                    };
                }

                // Add to Administrators role
                var addToRoleResult = await SetupNewAdministrator.Ensure_RolesAndAdmin_Exists(roleManager, userManager, user);

                if (!addToRoleResult)
                {
                    return new SetupCompletionResult
                    {
                        Success = false,
                        Message = $"Failed to assign admin role."
                    };
                }
                else
                {
                    logger.LogInformation("Admin user {Email} assigned to Administrators role", config.AdminEmail);
                }

                // Add user to Administrators role
                var roleResult = await userManager.AddToRoleAsync(user, "Administrators");

                if (!roleResult.Succeeded)
                {
                    logger.LogWarning("Failed to add user to Administrators role");
                }

                logger.LogInformation("Admin user {Email} added to Administrators role", config.AdminEmail);

                return new SetupCompletionResult
                {
                    Success = true,
                    Message = "Admin account created successfully"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create admin account");
                return new SetupCompletionResult
                {
                    Success = false,
                    Message = $"Failed to create admin account: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Saves settings to the main database.
        /// </summary>
        private async Task SaveSettingsToDatabaseAsync(ApplicationDbContext context, SetupConfiguration config)
        {
            try
            {
                // Save storage connection string
                await SaveOrUpdateSettingAsync(
                    context,
                    "STORAGE",
                    "StorageConnectionString",
                    config.StorageConnectionString,
                    "Cloud storage connection string");

                // Save blob public URL
                await SaveOrUpdateSettingAsync(
                    context,
                    "STORAGE",
                    "BlobPublicUrl",
                    config.BlobPublicUrl,
                    "Public URL for static assets");

                // Save publisher URL
                await SaveOrUpdateSettingAsync(
                    context,
                    "PUBLISHER",
                    "PublisherUrl",
                    config.PublisherUrl,
                    "Publisher website URL");

                // Save static web pages flag
                await SaveOrUpdateSettingAsync(
                    context,
                    "PUBLISHER",
                    "StaticWebPages",
                    config.StaticWebPages.ToString(),
                    "Enable static website mode");

                // Save requires authentication flag
                await SaveOrUpdateSettingAsync(
                    context,
                    "PUBLISHER",
                    "CosmosRequiresAuthentication",
                    config.CosmosRequiresAuthentication.ToString(),
                    "Website requires authentication");

                // Save allowed file types
                await SaveOrUpdateSettingAsync(
                    context,
                    "PUBLISHER",
                    "AllowedFileTypes",
                    config.AllowedFileTypes,
                    "Allowed file types for upload");

                // Save Microsoft App ID if provided
                if (!string.IsNullOrEmpty(config.MicrosoftAppId))
                {
                    await SaveOrUpdateSettingAsync(
                        context,
                        "OAUTH",
                        "MicrosoftAppId",
                        config.MicrosoftAppId,
                        "Microsoft OAuth Application ID");
                }

                // Save email settings if configured
                if (!string.IsNullOrEmpty(config.SendGridApiKey))
                {
                    await SaveOrUpdateSettingAsync(
                        context,
                        "EMAIL",
                        "SendGridApiKey",
                        config.SendGridApiKey,
                        "SendGrid API Key");
                }

                if (!string.IsNullOrEmpty(config.AzureEmailConnectionString))
                {
                    await SaveOrUpdateSettingAsync(
                        context,
                        "EMAIL",
                        "AzureEmailConnectionString",
                        config.AzureEmailConnectionString,
                        "Azure Communication Services connection string");
                }

                if (!string.IsNullOrEmpty(config.SmtpHost))
                {
                    await SaveOrUpdateSettingAsync(
                        context,
                        "EMAIL",
                        "SmtpHost",
                        config.SmtpHost,
                        "SMTP server host");

                    await SaveOrUpdateSettingAsync(
                        context,
                        "EMAIL",
                        "SmtpPort",
                        config.SmtpPort.ToString(),
                        "SMTP server port");

                    await SaveOrUpdateSettingAsync(
                        context,
                        "EMAIL",
                        "SmtpUsername",
                        config.SmtpUsername,
                        "SMTP username");

                    await SaveOrUpdateSettingAsync(
                        context,
                        "EMAIL",
                        "SmtpPassword",
                        config.SmtpPassword,
                        "SMTP password");
                }

                // Save CDN settings if configured
                if (!string.IsNullOrEmpty(config.AzureCdnSubscriptionId) &&
                    !string.IsNullOrEmpty(config.AzureCdnResourceGroup) &&
                    !string.IsNullOrEmpty(config.AzureCdnProfileName) &&
                    !string.IsNullOrEmpty(config.AzureCdnEndpointName))
                {
                    var azureCdnConfig = new Sky.Editor.Services.CDN.AzureCdnConfig
                    {
                        IsFrontDoor = config.AzureCdnIsFrontDoor,
                        SubscriptionId = config.AzureCdnSubscriptionId,
                        ResourceGroup = config.AzureCdnResourceGroup,
                        ProfileName = config.AzureCdnProfileName,
                        EndpointName = config.AzureCdnEndpointName
                    };

                    var azureCdnSetting = new Sky.Editor.Services.CDN.CdnSetting
                    {
                        CdnProvider = config.AzureCdnIsFrontDoor
                            ? Sky.Editor.Services.CDN.CdnProviderEnum.AzureFrontdoor
                            : Sky.Editor.Services.CDN.CdnProviderEnum.AzureCDN,
                        Value = JsonConvert.SerializeObject(azureCdnConfig)
                    };

                    await SaveOrUpdateSettingAsync(
                        context,
                        "CDN",
                        config.AzureCdnIsFrontDoor ? "AzureFrontDoor" : "AzureCDN",
                        JsonConvert.SerializeObject(azureCdnSetting),
                        config.AzureCdnIsFrontDoor ? "Azure Front Door CDN" : "Azure CDN");
                }

                if (!string.IsNullOrEmpty(config.CloudflareApiToken) &&
                    !string.IsNullOrEmpty(config.CloudflareZoneId))
                {
                    var cloudflareCdnConfig = new Sky.Editor.Services.CDN.CloudflareCdnConfig
                    {
                        ApiToken = config.CloudflareApiToken,
                        ZoneId = config.CloudflareZoneId
                    };

                    var cloudflareCdnSetting = new Sky.Editor.Services.CDN.CdnSetting
                    {
                        CdnProvider = Sky.Editor.Services.CDN.CdnProviderEnum.Cloudflare,
                        Value = JsonConvert.SerializeObject(cloudflareCdnConfig)
                    };

                    await SaveOrUpdateSettingAsync(
                        context,
                        "CDN",
                        "Cloudflare",
                        JsonConvert.SerializeObject(cloudflareCdnSetting),
                        "Cloudflare CDN");
                }

                if (!string.IsNullOrEmpty(config.SucuriApiKey) &&
                    !string.IsNullOrEmpty(config.SucuriApiSecret))
                {
                    var sucuriCdnConfig = new Sky.Editor.Services.CDN.SucuriCdnConfig
                    {
                        ApiKey = config.SucuriApiKey,
                        ApiSecret = config.SucuriApiSecret
                    };

                    var sucuriCdnSetting = new Sky.Editor.Services.CDN.CdnSetting
                    {
                        CdnProvider = Sky.Editor.Services.CDN.CdnProviderEnum.Sucuri,
                        Value = JsonConvert.SerializeObject(sucuriCdnConfig)
                    };

                    await SaveOrUpdateSettingAsync(
                        context,
                        "CDN",
                        "Sucuri",
                        JsonConvert.SerializeObject(sucuriCdnSetting),
                        "Sucuri CDN/Firewall");
                }

                // IMPORTANT: Set AllowSetup to false
                await SaveOrUpdateSettingAsync(
                    context,
                    "SYSTEM",
                    "AllowSetup",
                    "false",
                    "Allow setup mode");

                await context.SaveChangesAsync();

                logger.LogInformation("Settings saved to database successfully");

                // Check for partial Azure CDN configuration
                var azureFields = new[] { config.AzureCdnSubscriptionId, config.AzureCdnResourceGroup, config.AzureCdnProfileName, config.AzureCdnEndpointName };
                var nonEmptyCount = azureFields.Count(f => !string.IsNullOrEmpty(f));
                if (nonEmptyCount > 0 && nonEmptyCount < 4)
                {
                    logger.LogWarning("Partial Azure CDN configuration detected ({Count}/4 fields populated) - Azure CDN will not be saved", nonEmptyCount);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save settings to database");
                throw;
            }
        }

        /// <summary>
        /// Saves or updates a setting in the database.
        /// </summary>
        private async Task SaveOrUpdateSettingAsync(
            ApplicationDbContext context,
            string group,
            string name,
            string value,
            string description)
        {
            var setting = await context.Settings
                .FirstOrDefaultAsync(s => s.Group == group && s.Name == name);

            if (setting == null)
            {
                setting = new Setting
                {
                    Id = Guid.NewGuid(),
                    Group = group,
                    Name = name,
                    Value = value,
                    Description = description,
                    IsRequired = false
                };
                context.Settings.Add(setting);
            }
            else
            {
                setting.Value = value;
                setting.Description = description;
            }
        }

        /// <summary>
        /// Ensures a default layout exists.
        /// </summary>
        private async Task EnsureDefaultLayoutAndHomePageExistsAsync(ApplicationDbContext dbContext, SetupConfiguration config)
        {
            try
            {
                if (config.SiteDesignId == null)
                {
                    var layout = new Layout
                    {
                        Id = Guid.NewGuid(),
                        LayoutName = "Default Layout",
                        IsDefault = true,
                        Notes = "Default layout created by setup wizard",
                        Head = "<!-- Add your HEAD content here -->",
                        HtmlHeader = "<header>\n  <h1>Welcome to SkyCMS</h1>\n</header>",
                        FooterHtmlContent = "<footer>\n  <p>&copy; 2024 Your Company</p>\n</footer>",
                        Version = 1,
                        Published = DateTimeOffset.UtcNow,
                        LastModified = DateTimeOffset.UtcNow
                    };

                    dbContext.Layouts.Add(layout);
                    await dbContext.SaveChangesAsync();

                    logger.LogInformation("Created default layout");
                }
                else
                {
                    var layoutId = config.SiteDesignId?.ToString();

                    var layout = await layoutImportService.GetCommunityLayoutAsync(layoutId, true);
                    var communityPages = await layoutImportService.GetCommunityTemplatePagesAsync(layoutId);
                    var userId = await dbContext.Users.Select(u => u.Id).FirstOrDefaultAsync();

                    if ((await dbContext.Layouts.FirstOrDefaultAsync(a => a.IsDefault)) == null)
                    {
                        layout.Version = 1;
                        layout.IsDefault = true;
                    }
                    else
                    {
                        layout.Version = (await dbContext.Layouts.CountAsync()) + 1;
                        layout.IsDefault = false;
                    }

                    dbContext.Layouts.Add(layout);
                    await dbContext.SaveChangesAsync();

                    foreach (var page in communityPages)
                    {
                        page.LayoutId = layout.Id;
                    }

                    dbContext.Templates.AddRange(communityPages);
                    await dbContext.SaveChangesAsync();

                    // Create initial home page
                    var template = await dbContext.Templates.FirstOrDefaultAsync(f => f.Title.ToLower() == "home page");
                    var model = await articleLogic.CreateArticle(config.WebsiteTitle, Guid.Parse(userId), template.Id);

                    model.Published = DateTimeOffset.UtcNow;
                    model.StatusCode = (int)StatusCodeEnum.Active;
                    model.Content = template.Content;
                    model.UrlPath = "root";
                    model.Title = config.WebsiteTitle;
                    model.ArticleNumber = 1;
                    model.VersionNumber = 1;
                    model.Updated = DateTimeOffset.UtcNow;
                    model.ArticleType = ArticleType.General;

                    // NEW: Use SaveArticle command
                    var command = new SaveArticleCommand
                    {
                        ArticleNumber = model.ArticleNumber,
                        Title = model.Title,
                        Content = model.Content,
                        HeadJavaScript = model.HeadJavaScript,
                        FooterJavaScript = model.FooterJavaScript,
                        BannerImage = model.BannerImage,
                        ArticleType = model.ArticleType,
                        Category = model.Category,
                        Introduction = model.Introduction,
                        UrlPath = model.UrlPath,
                        Published = model.Published,
                        UserId = Guid.Parse(userId)
                    };

                    var result = await mediator.SendAsync(command);

                    logger.LogInformation($"Using site design: '{layout.LayoutName}'");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create default layout");
                // Don't throw - this is not critical for setup completion
            }
        }
    }
}
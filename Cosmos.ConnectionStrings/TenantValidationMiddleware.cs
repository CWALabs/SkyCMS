using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Cosmos.DynamicConfig.Middleware
{
    /// <summary>
    /// Middleware to validate tenant domain before processing requests.
    /// </summary>
    public class TenantValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantValidationMiddleware> _logger;

        public TenantValidationMiddleware(RequestDelegate next, ILogger<TenantValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IDynamicConfigurationProvider configProvider)
        {
            // Skip validation for static files, health checks, and error pages
            if (context.Request.Path.StartsWithSegments("/health") ||
                context.Request.Path.StartsWithSegments("/favicon.ico") ||
                context.Request.Path.StartsWithSegments("/TenantError") ||
                context.Request.Path.Value?.Contains(".") == true)
            {
                await _next(context);
                return;
            }

            try
            {
                var domainName = configProvider.GetTenantDomainNameFromRequest();

                if (string.IsNullOrWhiteSpace(domainName))
                {
                    _logger.LogWarning("No domain name found in request");
                    context.Items["TenantErrorTitle"] = "Invalid Request";
                    context.Items["TenantErrorMessage"] = "No domain name could be determined from the request.";
                    context.Items["TenantDomainName"] = string.Empty;
                    context.Request.Path = "/TenantError";
                    await _next(context);
                    return;
                }

                var isValid = await configProvider.ValidateDomainName(domainName);

                if (!isValid)
                {
                    _logger.LogWarning("Invalid tenant domain: {Domain}", domainName);
                    context.Items["TenantErrorTitle"] = "Tenant Not Found";
                    context.Items["TenantErrorMessage"] = $"The domain '{domainName}' is not configured for this application.";
                    context.Items["TenantDomainName"] = domainName;
                    context.Request.Path = "/TenantError";
                    await _next(context);
                    return;
                }

                // Domain is valid, continue processing
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating tenant");
                context.Items["TenantErrorTitle"] = "Configuration Error";
                context.Items["TenantErrorMessage"] = "An error occurred while validating your request.";
                context.Items["TenantDomainName"] = string.Empty;
                context.Request.Path = "/TenantError";
                await _next(context);
            }
        }
    }
}

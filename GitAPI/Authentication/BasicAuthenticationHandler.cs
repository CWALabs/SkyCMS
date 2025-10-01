using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Sky.GitAPI.Models;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sky.GitAPI.Authentication
{
    /// <summary>
    /// Basic authentication handler for Git API
    /// </summary>
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly GitApiSettings _settings;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IOptions<GitApiSettings> settings)
            : base(options, logger, encoder)
        {
            _settings = settings.Value;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Check if Authorization header exists
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header"));
            }

            var authHeader = Request.Headers["Authorization"].ToString();
            
            if (!authHeader.StartsWith("Basic "))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization header"));
            }

            try
            {
                // Decode the Base64 encoded credentials
                var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
                var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
                var parts = credentials.Split(':', 2);

                if (parts.Length != 2)
                {
                    return Task.FromResult(AuthenticateResult.Fail("Invalid credentials format"));
                }

                var username = parts[0];
                var password = parts[1];

                // Validate credentials against settings
                if (username == _settings.Username && password == _settings.Password)
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.Name, username),
                        new Claim(ClaimTypes.Role, "GitUser")
                    };

                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);

                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }

                return Task.FromResult(AuthenticateResult.Fail("Invalid username or password"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(AuthenticateResult.Fail($"Authentication error: {ex.Message}"));
            }
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Headers["WWW-Authenticate"] = "Basic realm=\"Sky CMS Git API\"";
            return base.HandleChallengeAsync(properties);
        }
    }
}
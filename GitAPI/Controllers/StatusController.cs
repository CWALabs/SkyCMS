using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Extensions.Logging;

namespace Sky.GitAPI.Controllers
{
    /// <summary>
    /// Health check and status controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly ILogger<StatusController> _logger;

        public StatusController(ILogger<StatusController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTimeOffset.UtcNow,
                Service = "Sky CMS Git API",
                Version = "1.0.0"
            });
        }

        /// <summary>
        /// Get API information
        /// </summary>
        [HttpGet("info")]
        public IActionResult Info()
        {
            return Ok(new
            {
                Name = "Sky CMS Git API",
                Version = "1.0.0",
                Description = "Git-compatible API for editing Sky CMS articles",
                Endpoints = new
                {
                    Git = new[]
                    {
                        "GET /api/git/refs",
                        "GET /api/git/refs/{refPath}",
                        "GET /api/git/trees/{sha}",
                        "GET /api/git/blobs/{sha}",
                        "POST /api/git/blobs",
                        "GET /api/git/commits/{sha}",
                        "PATCH /api/git/refs/{refPath}"
                    },
                    Articles = new[]
                    {
                        "GET /api/articles",
                        "GET /api/articles/{id}",
                        "GET /api/articles/{id}/versions",
                        "PUT /api/articles/{id}",
                        "POST /api/articles"
                    }
                },
                Authentication = "HTTP Basic Authentication required"
            });
        }
    }
}
namespace Sky.Editor.Services.CDN
{

    /// <summary>
    /// Cloudflare settings.
    /// </summary>
    public class CloudflareCdnConfig
    {
        /// <summary>
        /// Gets or sets the API token.
        /// </summary>
        public string ApiToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the zone ID.
        /// </summary>
        public string ZoneId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets validation trigger to ensure all or none of the AzureCDN properties are set.
        /// </summary>
        [AllOrNoneRequired("ApiToken", "ZoneId", ErrorMessage = "Cloudflare settings are not complete.")]
        public string ValidationTrigger { get; set; } = string.Empty; // dummy property to attach the attribute
    }
}

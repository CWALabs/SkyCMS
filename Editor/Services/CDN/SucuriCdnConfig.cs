namespace Sky.Editor.Services.CDN
{

    /// <summary>
    /// Settings for Sucuri CDN/Firewall.
    /// </summary>
    public class SucuriCdnConfig
    {
        /// <summary>
        /// Gets or sets the API key.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the API secret used for authentication or secure communication with the API.
        /// </summary>
        public string ApiSecret { get; set; }

        /// <summary>
        /// Gets or sets validation trigger to ensure all or none of the AzureCDN properties are set.
        /// </summary>
        [AllOrNoneRequired("ApiKey", "ApiSecret", ErrorMessage = "Sucuri CDN/Firewall settings are not complete.")]
        public string ValidationTrigger { get; set; } = string.Empty; // dummy property to attach the attribute
    }
}

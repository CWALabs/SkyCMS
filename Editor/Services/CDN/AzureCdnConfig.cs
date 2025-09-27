namespace Sky.Editor.Services.CDN
{
    /// <summary>
    ///  Settings for Azure CDN/Front door.
    /// </summary>
    public class AzureCdnConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether to use Azure Front Door. If false, Azure CDN is used.
        /// </summary>
        public bool IsFrontDoor { get; set; } = false;

        /// <summary>
        ///  Gets or sets the endpoint name.
        /// </summary>
        public string EndpointName { get; set; } = string.Empty;

        /// <summary>
        ///  Gets or sets the profile name.
        /// </summary>
        public string ProfileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resource group name.
        /// </summary>
        public string ResourceGroup { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unique identifier for the subscription.
        /// </summary>
        public string SubscriptionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets validation trigger to ensure all or none of the AzureCDN properties are set.
        /// </summary>
        [AllOrNoneRequired("EndpointName", "ProfileName", "ResourceGroup", "SubscriptionId", ErrorMessage = "AzureCDN or Front door settings are not complete.")]
        public string ValidationTrigger { get; set; } = string.Empty; // dummy property to attach the attribute
    }
}

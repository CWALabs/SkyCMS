namespace Sky.Editor.Services.Scheduling
{
    using System.Threading.Tasks;
    using Sky.Editor.Data.Logic;

    /// <summary>
    /// Interface for a factory that creates ArticleEditLogic instances for specific tenants.
    /// </summary>
    public interface ITenantArticleLogicFactory
    {
        /// <summary>
        /// Creates an ArticleEditLogic instance for the specified tenant domain name.
        /// </summary>
        /// <param name="domainName">Domain name.</param>
        /// <returns>ArticleEditLogic.</returns>
        Task<ArticleEditLogic> CreateForTenantAsync(string domainName);
    }
}
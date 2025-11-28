namespace Sky.Editor.Services.Scheduling
{
    using System.Threading.Tasks;
    using Sky.Editor.Data.Logic;

    public interface ITenantArticleLogicFactory
    {
        Task<ArticleEditLogic> CreateForTenantAsync(string domainName);
    }
}
namespace Sky.Editor.Services.Scheduling
{
    using Sky.Editor.Data.Logic;
    using System.Threading.Tasks;

    public interface ITenantArticleLogicFactory
    {
        Task<ArticleEditLogic> CreateForTenantAsync(string domainName);
    }
}
using System.Threading;
using System.Threading.Tasks;

namespace Sky.Editor.Features.Shared
{
    /// <summary>
    /// Handler for executing queries.
    /// </summary>
    /// <typeparam name="TQuery">The query type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    public interface IQueryHandler<in TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        /// <summary>
        /// Handles the query execution.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the query.</returns>
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
    }
}
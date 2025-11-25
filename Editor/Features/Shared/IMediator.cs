using System.Threading;
using System.Threading.Tasks;

namespace Sky.Editor.Features.Shared
{
    /// <summary>
    /// Mediator for dispatching commands and queries to their handlers.
    /// </summary>
    public interface IMediator
    {
        /// <summary>
        /// Sends a command to its handler.
        /// </summary>
        Task<TResult> SendAsync<TResult>(
            ICommand<TResult> command,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a query to its handler.
        /// </summary>
        Task<TResult> QueryAsync<TResult>(
            IQuery<TResult> query,
            CancellationToken cancellationToken = default);
    }
}
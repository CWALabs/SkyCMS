using System.Threading;
using System.Threading.Tasks;

namespace Sky.Editor.Features.Shared
{
    /// <summary>
    /// Handler for executing commands.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    public interface ICommandHandler<in TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        /// <summary>
        /// Handles the command execution.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the command execution.</returns>
        Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
    }
}
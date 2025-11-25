namespace Sky.Editor.Features.Shared
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Simple mediator implementation using service provider for handler resolution.
    /// </summary>
    public class Mediator : IMediator
    {
        private readonly IServiceProvider serviceProvider;

        public Mediator(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task<TResult> SendAsync<TResult>(
            ICommand<TResult> command,
            CancellationToken cancellationToken = default)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var commandType = command.GetType();
            var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));

            var handler = serviceProvider.GetRequiredService(handlerType);
            var method = handlerType.GetMethod(nameof(ICommandHandler<ICommand<TResult>, TResult>.HandleAsync));

            if (method == null)
            {
                throw new InvalidOperationException($"Handler method not found for {commandType.Name}");
            }

            var result = method.Invoke(handler, new object[] { command, cancellationToken });

            if (result is Task<TResult> task)
            {
                return await task;
            }

            throw new InvalidOperationException($"Handler did not return expected type for {commandType.Name}");
        }

        public async Task<TResult> QueryAsync<TResult>(
            IQuery<TResult> query,
            CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var queryType = query.GetType();
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResult));

            var handler = serviceProvider.GetRequiredService(handlerType);
            var method = handlerType.GetMethod(nameof(IQueryHandler<IQuery<TResult>, TResult>.HandleAsync));

            if (method == null)
            {
                throw new InvalidOperationException($"Handler method not found for {queryType.Name}");
            }

            var result = method.Invoke(handler, new object[] { query, cancellationToken });

            if (result is Task<TResult> task)
            {
                return await task;
            }

            throw new InvalidOperationException($"Handler did not return expected type for {queryType.Name}");
        }
    }
}
namespace Sky.Editor.Features.Shared
{
    /// <summary>
    /// Marker interface for commands that modify state.
    /// </summary>
    /// <typeparam name="TResult">The result type of the command execution.</typeparam>
    public interface ICommand<TResult>
    {
    }
}

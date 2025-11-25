namespace Sky.Editor.Features.Shared
{
    /// <summary>
    /// Marker interface for queries that retrieve data without modifying state.
    /// </summary>
    /// <typeparam name="TResult">The result type of the query.</typeparam>
    public interface IQuery<TResult>
    {
    }
}
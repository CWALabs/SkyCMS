using System.Collections.Generic;

namespace Sky.Editor.Features.Shared
{
    /// <summary>
    /// Base result for command execution with success/failure status.
    /// </summary>
    public class CommandResult
    {
        public bool IsSuccess { get; init; }
        public string? ErrorMessage { get; init; }
        public Dictionary<string, string[]>? Errors { get; init; }

        public static CommandResult Success() => new() { IsSuccess = true };

        public static CommandResult Failure(string errorMessage) =>
            new() { IsSuccess = false, ErrorMessage = errorMessage };

        public static CommandResult Failure(Dictionary<string, string[]> errors) =>
            new() { IsSuccess = false, Errors = errors };
    }

    /// <summary>
    /// Result for command execution that returns data.
    /// </summary>
    /// <typeparam name="T">The data type.</typeparam>
    public class CommandResult<T> : CommandResult
    {
        public T? Data { get; init; }

        public static CommandResult<T> Success(T data) =>
            new() { IsSuccess = true, Data = data };

        public new static CommandResult<T> Failure(string errorMessage) =>
            new() { IsSuccess = false, ErrorMessage = errorMessage };

        public new static CommandResult<T> Failure(Dictionary<string, string[]> errors) =>
            new() { IsSuccess = false, Errors = errors };
    }
}
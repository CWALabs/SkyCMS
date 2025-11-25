namespace Sky.Editor.Features.Articles.Create
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Validates the CreateArticleCommand.
    /// </summary>
    public class CreateArticleValidator
    {
        public Dictionary<string, string[]> Validate(CreateArticleCommand command)
        {
            var errors = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(command.Title))
            {
                errors[nameof(command.Title)] = new[] { "Title is required." };
            }
            else if (command.Title.Length > 254)
            {
                errors[nameof(command.Title)] = new[] { "Title must not exceed 254 characters." };
            }

            if (command.UserId == Guid.Empty)
            {
                errors[nameof(command.UserId)] = new[] { "UserId is required." };
            }

            if (command.BlogKey.Length > 128)
            {
                errors[nameof(command.BlogKey)] = new[] { "BlogKey must not exceed 128 characters." };
            }

            return errors;
        }
    }
}
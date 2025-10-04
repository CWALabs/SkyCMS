namespace Cosmos.Common.Models.Blog
{
    using System.Collections.Generic;

    /// <summary>
    /// Blog index view model.
    /// </summary>
    public class BlogIndexViewModel
    {
        public IEnumerable<BlogListItem> Posts { get; set; } = new List<BlogListItem>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public string Category { get; set; } = string.Empty;
    }
}

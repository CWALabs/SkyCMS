namespace Cosmos.Common.Models.Blog
{
    using System;

    /// <summary>
    /// Lightweight blog list item for listings.
    /// </summary>
    public class BlogListItem
    {
        public Guid Id { get; set; }
        public int ArticleNumber { get; set; }
        public string Title { get; set; }
        public string UrlPath { get; set; }
        public DateTimeOffset? Published { get; set; }
        public string BannerImage { get; set; }
        public string Introduction { get; set; }
        public string Category { get; set; }
    }
}

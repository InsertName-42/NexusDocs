namespace NexusDocs.Models
{
    public class PublicPageViewModel
    {
        public string SiteTitle { get; set; } = string.Empty;
        public string PageTitle { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? GlobalTheme { get; set; }
        public string? CustomStyles { get; set; }

        //For the navigation bar
        public List<PageNavEntry> Navigation { get; set; } = new();

        //Functional tags and interactions
        public List<string> ScriptPaths { get; set; } = new();
        public List<PageInteraction> Interactions { get; set; } = new();
        public DateTime? EventDate { get; set; }
        public int PageId { get; set; }
    }

    public class PageNavEntry
    {
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
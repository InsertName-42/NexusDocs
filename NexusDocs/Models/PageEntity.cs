using System.ComponentModel.DataAnnotations.Schema;

namespace NexusDocs.Models
{
    public class PageEntity
    {
        public int PageEntityId { get; set; }
        public int SiteId { get; set; }
        [ForeignKey("SiteId")]
        public virtual SiteEntity? Site { get; set; }
        public int? TemplateId { get; set; }
        [ForeignKey("TemplateId")]
        public virtual TemplateEntity? Template { get; set; }
        public virtual ICollection<TagEntity> Tags { get; set; } = new List<TagEntity>();
        public required string PageTitle { get; set; }
        //The URL path for the page, e.g., "/gifts" or "/recipes"
        public required string Slug { get; set; }
        public string? LastETag { get; set; }
        public string? CachedContent { get; set; }
        public string? GoogleDocId { get; set; }
        public int? SortOrder { get; set; }
        public DateTime? LastSynced { get; set; }

        public DateTime? EventDate { get; set; }
        public virtual ICollection<PageInteraction> Interactions { get; set; } = new List<PageInteraction>();
    }
}

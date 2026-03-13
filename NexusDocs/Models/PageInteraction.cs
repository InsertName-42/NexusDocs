using System.ComponentModel.DataAnnotations.Schema;

namespace NexusDocs.Models
{
    public class PageInteraction
    {
    public int Id { get; set; }
        public int PageId { get; set; }
        [ForeignKey("PageId")]
        public virtual PageEntity? Page { get; set; }
        public string ElementKey { get; set; } = string.Empty;
        public InteractionType InteractionType { get; set; }
        public string Value { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum InteractionType
    {
        Toggle,
        Rating,
        Location,
        Comment
    }
}

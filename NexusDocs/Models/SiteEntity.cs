using System.ComponentModel.DataAnnotations.Schema;

namespace NexusDocs.Models
{
    public class SiteEntity
    {
        public int SiteEntityId { get; set; }
        public required string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUser? User { get; set; }
        public required string SiteTitle { get; set; }
        public string? GlobalTheme { get; set; }
        public string? WebIconURL { get; set; }
        public bool IsPublic { get; set; } = true;
        public virtual ICollection<PageEntity> Pages { get; set; } = new List<PageEntity>();



    }
}

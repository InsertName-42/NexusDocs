namespace NexusDocs.Models
{
    public class TagEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string ScriptPath { get; set; }
        public bool IsEnabled { get; set; }
        public virtual ICollection<PageEntity> Pages { get; set; } = new List<PageEntity>();
    }
}

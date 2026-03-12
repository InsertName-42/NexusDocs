namespace NexusDocs.Models
{
    public class TemplateEntity
    {
        public int TemplateEntityId { get; set; }
        public required string Name { get; set; }
        public required string ViewPath { get; set; }
        public string? DefaultStyles { get; set; }
    }
}

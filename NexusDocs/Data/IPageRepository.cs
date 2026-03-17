using NexusDocs.Models;

namespace NexusDocs.Repositories
{
    public interface IPageRepository
    {
        Task<PageEntity?> GetPageBySlugAsync(string userKey, string slug);
        Task<List<PageNavEntry>> GetNavigationEntriesAsync(int siteId, string currentSlug);
        Task<PageInteraction?> GetInteractionAsync(int pageId, string elementKey);
        Task UpdateInteractionAsync(PageInteraction interaction);
        Task<bool> SlugExistsAsync(int siteId, string slug);
        Task AddPageAsync(PageEntity page);
        Task UpdatePageAsync(PageEntity page);
    }
}
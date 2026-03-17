using NexusDocs.Models;
using NexusDocs.Repositories;

namespace NexusDocs.Tests.Fakes
{
    public class FakePageRepository : IPageRepository
    {
        public List<PageEntity> Pages { get; set; } = new();
        public List<PageInteraction> Interactions { get; set; } = new();

        public async Task<PageEntity?> GetPageBySlugAsync(string userKey, string slug)
        {
            return Pages.FirstOrDefault(p => p.Slug == slug && p.Site?.User?.UserKey == userKey);
        }

        public async Task<List<PageNavEntry>> GetNavigationEntriesAsync(int siteId, string currentSlug)
        {
            return Pages.Where(p => p.SiteId == siteId)
                        .Select(p => new PageNavEntry { Title = p.PageTitle, Slug = p.Slug, IsActive = p.Slug == currentSlug })
                        .ToList();
        }

        public async Task UpdatePageAsync(PageEntity page)
        {
            var existing = Pages.FirstOrDefault(p => p.PageEntityId == page.PageEntityId);
            if (existing != null) Pages[Pages.IndexOf(existing)] = page;
            await Task.CompletedTask;
        }

        public async Task UpdateInteractionAsync(PageInteraction interaction)
        {
            Interactions.Add(interaction);
            await Task.CompletedTask;
        }

        public Task<PageInteraction?> GetInteractionAsync(int pageId, string elementKey) =>
            Task.FromResult(Interactions.FirstOrDefault(i => i.PageId == pageId && i.ElementKey == elementKey));
        public Task<bool> SlugExistsAsync(int siteId, string slug) => Task.FromResult(Pages.Any(p => p.SiteId == siteId && p.Slug == slug));
        public Task AddPageAsync(PageEntity page) { Pages.Add(page); return Task.CompletedTask; }
    }
}
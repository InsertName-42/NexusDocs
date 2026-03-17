using Microsoft.EntityFrameworkCore;
using NexusDocs.Data;
using NexusDocs.Models;

namespace NexusDocs.Repositories
{
    public class PageRepository : IPageRepository
    {
        private readonly ApplicationDbContext _context;

        public PageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PageEntity?> GetPageBySlugAsync(string userKey, string slug)
        {
            return await _context.Pages
                .Include(p => p.Site).ThenInclude(s => s.User)
                .Include(p => p.Template)
                .Include(p => p.Tags)
                .Include(p => p.Interactions)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.Site.User.UserKey == userKey);
        }

        public async Task<List<PageNavEntry>> GetNavigationEntriesAsync(int siteId, string currentSlug)
        {
            return await _context.Pages
                .Where(p => p.SiteId == siteId)
                .OrderBy(p => p.SortOrder)
                .Select(p => new PageNavEntry
                {
                    Title = p.PageTitle,
                    Slug = p.Slug,
                    IsActive = p.Slug == currentSlug
                })
                .ToListAsync();
        }

        public async Task<PageInteraction?> GetInteractionAsync(int pageId, string elementKey)
        {
            return await _context.PageInteractions
                .FirstOrDefaultAsync(i => i.PageId == pageId && i.ElementKey == elementKey);
        }

        public async Task UpdateInteractionAsync(PageInteraction interaction)
        {
            var existing = await GetInteractionAsync(interaction.PageId, interaction.ElementKey);
            if (existing == null)
            {
                _context.PageInteractions.Add(interaction);
            }
            else
            {
                existing.Value = interaction.Value;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePageAsync(PageEntity page)
        {
            _context.Pages.Update(page);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> SlugExistsAsync(int siteId, string slug)
        {
            return await _context.Pages.AnyAsync(p => p.SiteId == siteId && p.Slug == slug);
        }

        public async Task AddPageAsync(PageEntity page)
        {
            await _context.Pages.AddAsync(page);
            await _context.SaveChangesAsync();
        }
    }
}
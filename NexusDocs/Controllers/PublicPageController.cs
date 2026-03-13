using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusDocs.Data;
using NexusDocs.Models;
using NexusDocs.Services;

namespace NexusDocs.Controllers
{
    public class PublicPageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GoogleSyncService _syncService;

        public PublicPageController(ApplicationDbContext context, GoogleSyncService syncService)
        {
            _context = context;
            _syncService = syncService;
        }

        public async Task<IActionResult> Display(string userKey, string slug)
        {
            //1. Fetch the page
            var page = await _context.Pages
                .Include(p => p.Site).ThenInclude(s => s.User)
                .Include(p => p.Template)
                .Include(p => p.Tags)
                .Include(p => p.Interactions)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.Site.User.UserKey == userKey);

            if (page == null || page.Site == null) return NotFound();

            //2. Perform the Google Sync Check
            if (!string.IsNullOrEmpty(page.GoogleDocId))
            {
                bool isMarkdown = page.Tags.Any(t => t.Name == "Markdown" && t.IsEnabled);

                try
                {
                    // page.LastETag now holds our version string
                    var (newContent, newVersion) = await _syncService.SyncPageAsync(page.GoogleDocId, page.LastETag, isMarkdown);

                    if (newContent != null)
                    {
                        page.CachedContent = newContent;
                        page.LastETag = newVersion;
                        page.LastSynced = DateTime.UtcNow;

                        _context.Pages.Update(page);
                        await _context.SaveChangesAsync();

                        System.Diagnostics.Debug.WriteLine($"SUCCESS: Saved version {newVersion} to DB.");
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.SyncError = ex.Message;
                }
            }

            //3. Navigation mapping
            var navPages = await _context.Pages
                .Where(p => p.SiteId == page.SiteId)
                .OrderBy(p => p.SortOrder)
                .Select(p => new PageNavEntry
                {
                    Title = p.PageTitle,
                    Slug = p.Slug,
                    IsActive = p.Slug == slug
                })
                .ToListAsync();

            //4. Build View Model
            var viewModel = new PublicPageViewModel
            {
                SiteTitle = page.Site.SiteTitle,
                PageTitle = page.PageTitle,
                Content = page.CachedContent,
                GlobalTheme = page.Site.GlobalTheme,
                CustomStyles = page.Template?.DefaultStyles,
                Navigation = navPages,
                ScriptPaths = page.Tags.Where(t => t.IsEnabled).Select(t => t.ScriptPath).ToList(),
                Interactions = page.Interactions.ToList()
            };

            string viewName = page.Template?.ViewPath ?? "Default";
            return View(viewName, viewModel);
        }
    }
}
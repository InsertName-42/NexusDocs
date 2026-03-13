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

        //Inject both the Database Context and the new Sync Service
        public PublicPageController(ApplicationDbContext context, GoogleSyncService syncService)
        {
            _context = context;
            _syncService = syncService;
        }

        public async Task<IActionResult> Display(string userKey, string slug)
        {
            //1. Fetch the page with all necessary related data
            var page = await _context.Pages
                .Include(p => p.Site)
                    .ThenInclude(s => s.User)
                .Include(p => p.Template)
                .Include(p => p.Tags)
                .Include(p => p.Interactions)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.Site.User.UserKey == userKey);

            if (page == null || page.Site == null)
            {
                return NotFound();
            }

            //2. Perform the Google Sync Check
            if (!string.IsNullOrEmpty(page.GoogleDocId))
            {
                //Check if the "Markdown" tag is active for this specific page
                bool isMarkdown = page.Tags.Any(t => t.Name == "Markdown" && t.IsEnabled);

                try
                {
                    //SyncPageAsync returns content only if the ETag has changed
                    var (newContent, newEtag) = await _syncService.SyncPageAsync(page.GoogleDocId, page.LastETag, isMarkdown);

                    if (newContent != null)
                    {
                        page.CachedContent = newContent;
                        page.LastETag = newEtag;
                        page.LastSynced = DateTime.UtcNow;

                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Google Sync Error: {ex.Message}");

                    ViewBag.SyncError = "Unable to sync with Google Docs. Please check permissions.";

                    if (ex.Message.Contains("403"))
                    {
                        ViewBag.SyncError = "Access Denied: Ensure the Google Doc is shared as 'Anyone with the link can view'.";
                    }
                }
            }

            //3. Build Navigation for the sidebar/header
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

            //4. Populate the ViewModel
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

            //5. Render using the assigned template or the default
            string viewName = page.Template?.ViewPath ?? "Default";
            return View(viewName, viewModel);
        }
    }
}
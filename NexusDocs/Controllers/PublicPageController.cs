using Microsoft.AspNetCore.Mvc;
using NexusDocs.Models;
using NexusDocs.Repositories;
using Microsoft.AspNetCore.Authorization;
using NexusDocs.Services;
using Microsoft.EntityFrameworkCore;


namespace NexusDocs.Controllers
{
    public class PublicPageController : Controller
    {
        private readonly IPageRepository _repo;
        private readonly GoogleSyncService _syncService;

        public PublicPageController(IPageRepository repo, GoogleSyncService syncService)
        {
            _repo = repo;
            _syncService = syncService;
        }
        public async Task<IActionResult> Display(string userKey, string slug)
        {
            var page = await _repo.GetPageBySlugAsync(userKey, slug);

            if (page == null || page.Site == null) return NotFound();

            if (!string.IsNullOrEmpty(page.GoogleDocId))
            {
                bool isMarkdown = page.Tags.Any(t => t.Name == "Markdown" && t.IsEnabled);
                try
                {
                    var (newContent, newVersion) = await _syncService.SyncPageAsync(page.GoogleDocId, page.LastETag, isMarkdown);

                    if (newContent != null)
                    {
                        page.CachedContent = newContent;
                        page.LastETag = newVersion;
                        page.LastSynced = DateTime.UtcNow;
                        await _repo.UpdatePageAsync(page);
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.SyncError = "Sync failed: " + ex.Message;
                }
            }


            var viewModel = new PublicPageViewModel
            {
                SiteTitle = page.Site.SiteTitle,
                PageTitle = page.PageTitle,
                Content = page.CachedContent,
                GlobalTheme = page.Site.GlobalTheme,
                CustomStyles = page.Template?.Name ?? "Default",
                Navigation = await _repo.GetNavigationEntriesAsync(page.SiteId, slug),
                PageId = page.PageEntityId,
                Interactions = page.Interactions.ToList(),
                EventDate = page.EventDate,
                Tags = page.Tags.Where(t => t.IsEnabled).ToList(),
                TagZones = page.Tags
                    .Where(t => t.IsEnabled)
                    .GroupBy(t => t.Zone ?? "Bottom")
                    .ToDictionary(g => g.Key, g => g.Select(t => t.Name).ToList())
            };

            return View(page.Template?.ViewPath ?? "Default", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleInteraction(int pageId, string elementKey, string value)
        {
            var interaction = new PageInteraction
            {
                PageId = pageId,
                ElementKey = elementKey,
                Value = value,
                InteractionType = InteractionType.Toggle
            };

            await _repo.UpdateInteractionAsync(interaction);
            return Json(new { success = true });
        }
    }
}
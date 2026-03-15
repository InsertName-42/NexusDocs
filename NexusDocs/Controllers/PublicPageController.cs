using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusDocs.Data;
using NexusDocs.Models;
using NexusDocs.Services;
using System.Text.RegularExpressions;

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
            var page = await _context.Pages
                .Include(p => p.Site).ThenInclude(s => s.User)
                .Include(p => p.Template)
                .Include(p => p.Tags)
                .Include(p => p.Interactions)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.Site.User.UserKey == userKey);

            if (page == null || page.Site == null) return NotFound();

            if (!string.IsNullOrEmpty(page.GoogleDocId))
            {
                bool isMarkdown = page.Tags.Any(t => t.Name == "Markdown" && t.IsEnabled);

                try
                {
                    var (newContent, newVersion) = await _syncService.SyncPageAsync(page.GoogleDocId, page.LastETag, isMarkdown);

                    if (newContent != null)
                    {
                        if (isMarkdown)
                        {
                            //Convert plain text markdown to HTML
                            page.CachedContent = Markdig.Markdown.ToHtml(newContent);
                        }
                        else
                        {
                            //1. Get inner body content
                            var bodyMatch = Regex.Match(newContent, @"<body[^>]*>(.*?)</body>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                            string cleaned = bodyMatch.Success ? bodyMatch.Groups[1].Value : newContent;

                            //2. Remove inline styles (fixes the 1/3rd screen width and padding issue)
                            page.CachedContent = Regex.Replace(cleaned, @"style\s*=\s*""[^""]*""", "", RegexOptions.IgnoreCase);
                        }

                        page.LastETag = newVersion;
                        page.LastSynced = DateTime.UtcNow;

                        _context.Pages.Update(page);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.SyncError = "Sync failed: " + ex.Message;
                }
            }

            //Navigation and View Model building
            var navPages = await _context.Pages
                .Where(p => p.SiteId == page.SiteId)
                .OrderBy(p => p.SortOrder)
                .Select(p => new PageNavEntry { Title = p.PageTitle, Slug = p.Slug, IsActive = p.Slug == slug })
                .ToListAsync();

            var viewModel = new PublicPageViewModel
            {
                SiteTitle = page.Site.SiteTitle,
                PageTitle = page.PageTitle,
                Content = page.CachedContent,
                GlobalTheme = page.Site.GlobalTheme,
                CustomStyles = page.Template?.Name ?? "Default",
                Navigation = navPages,
                PageId = page.PageEntityId,
                Interactions = page.Interactions.ToList(),
                EventDate = page.EventDate,
                ScriptPaths = page.Tags
                    .Where(t => t.IsEnabled)
                    .Select(t => t.Name)
                    .ToList(),
                TagZones = page.Tags
                    .Where(t => t.IsEnabled)
                    .GroupBy(t => t.Zone ?? "Bottom")
                    .ToDictionary(g => g.Key, g => g.Select(t => t.Name).ToList())
            };

            return View(page.Template?.ViewPath ?? "Default", viewModel);
        }
    }
}
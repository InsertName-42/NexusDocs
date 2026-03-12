using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusDocs.Data;
using NexusDocs.Models;

namespace NexusDocs.Controllers
{
    public class PublicPageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PublicPageController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Display(string userKey, string slug)
        {
            var page = _context.Pages
                .Include(p => p.Site)
                    .ThenInclude(s => s.User)
                .Include(p => p.Template)
                .Include(p => p.Tags)
                .Include(p => p.Interactions)
                .FirstOrDefault(p => p.Slug == slug && p.Site.User.UserKey == userKey);

            if (page == null || page.Site == null) return NotFound();

            var navPages = _context.Pages
                .Where(p => p.SiteId == page.SiteId)
                .OrderBy(p => p.SortOrder)
                .Select(p => new PageNavEntry
                {
                    Title = p.PageTitle,
                    Slug = p.Slug,
                    IsActive = p.Slug == slug
                })
                .ToList();

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

            if (page.Template == null) return View("Default", viewModel);

            return View(page.Template.ViewPath, viewModel);
        }
    }
}
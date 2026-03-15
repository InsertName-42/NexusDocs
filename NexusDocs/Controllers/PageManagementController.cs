using Google.Apis.Drive.v3.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusDocs.Data;
using NexusDocs.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class PageManagementController : Controller
{
    private readonly ApplicationDbContext _context;

    public PageManagementController(ApplicationDbContext context)
    {
        _context = context;
    }

    //List all pages for a specific site
    public async Task<IActionResult> Index(int siteId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var site = await _context.Sites
            .Include(s => s.User)
            .Include(s => s.Pages)
            .FirstOrDefaultAsync(s => s.SiteEntityId == siteId && s.UserId == userId);

        if (site == null) return NotFound("Site not found or access denied.");

        return View(site);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int siteId)
    {
        ViewBag.AvailableTags = await _context.Tags.ToListAsync();
        ViewBag.Templates = await _context.Templates.ToListAsync();
        return View(new PageEntity
        {
            SiteId = siteId,
            PageTitle = "",
            Slug = ""
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PageEntity page, int[] selectedTagIds)
    {
        ModelState.Remove("Site");
        ModelState.Remove("Template");
        ModelState.Remove("Interactions");
        ModelState.Remove("Tags");

        if (!string.IsNullOrEmpty(page.GoogleDocId))
        {
            page.GoogleDocId = ExtractDocId(page.GoogleDocId);
        }

        if (ModelState.IsValid)
        {
            if (selectedTagIds != null)
            {
                foreach (var id in selectedTagIds)
                {
                    var tag = await _context.Tags.FindAsync(id);
                    if (tag != null) page.Tags.Add(tag);
                }
            }
            _context.Add(page);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { siteId = page.SiteId });
        }
        ViewBag.AvailableTags = await _context.Tags.Where(t => t.IsEnabled).ToListAsync();
        ViewBag.Templates = await _context.Templates.ToListAsync();
        return View(page);
    }
    private string? ExtractDocId(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        var match = System.Text.RegularExpressions.Regex.Match(input, @"/d/(.+?)(/|$)");

        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return input.Trim();
    }
}


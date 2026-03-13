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
            .Include(s => s.Pages)
            .FirstOrDefaultAsync(s => s.SiteEntityId == siteId && s.UserId == userId);

        if (site == null) return NotFound("Site not found or access denied.");

        return View(site);
    }

    [HttpGet]
    public IActionResult Create(int siteId)
    {
        return View(new PageEntity
        {
            SiteId = siteId,
            PageTitle = "",
            Slug = ""
        });
}

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SiteEntity site)
    {
        //Manually assign the owner's ID
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        site.UserId = userId;

        //Remove validation for properties not in the form
        ModelState.Remove("User");
        ModelState.Remove("Pages");
        ModelState.Remove("UserId");

        if (ModelState.IsValid)
        {
            _context.Add(site);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //Debugging
        foreach (var modelState in ModelState.Values)
        {
            foreach (var error in modelState.Errors)
            {
                System.Diagnostics.Debug.WriteLine($"Validation Error: {error.ErrorMessage}");
            }
        }

        return View(site);
    }
}
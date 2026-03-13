using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusDocs.Data;
using NexusDocs.Models;
using System.Security.Claims;

namespace NexusDocs.Controllers
{
    [Authorize] //Only logged-in users can manage sites
    public class SiteManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SiteManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SiteEntity site)
        {
            // 1. Get the ID from the current login cookie
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(userId);

            if (user != null && string.IsNullOrEmpty(user.UserKey))
            {
                // Default the UserKey to the first part of their email
                user.UserKey = user.UserName.Split('@')[0];
                _context.Update(user);
            }

            // 2. Double check that this user actually exists in the DB
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);

            if (string.IsNullOrEmpty(userId) || !userExists)
            {
                // If the user doesn't exist (e.g. was deleted), force a logout/re-login
                return Challenge();
            }

            site.UserId = userId;

            // 3. Clear validation for things we aren't getting from the form
            ModelState.Remove("User");
            ModelState.Remove("Pages");
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                _context.Add(site);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(site);
        }
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var sites = await _context.Sites
                .Where(s => s.UserId == userId)
                .ToListAsync();

            return View(sites);
        }
    }
}
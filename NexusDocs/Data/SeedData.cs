using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexusDocs.Data;
using NexusDocs.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

public class SeedData
{
    public static async Task Seed(ApplicationDbContext context, IServiceProvider provider)
    {
        var userManager = provider.GetRequiredService<UserManager<AppUser>>();

        if(await context.Pages.AnyAsync(p => p.Slug == "prologue")) { return; }

        const string SECRET_PASSWORD = "Password123!";
        const string SEED_EMAIL = "ace@example.com";

        // 1. Get the tracked user instance
        var appUser = await userManager.FindByEmailAsync(SEED_EMAIL);
        if (appUser == null)
        {
            appUser = new AppUser
            {
                UserName = SEED_EMAIL,
                Email = SEED_EMAIL,
                EmailConfirmed = true,
                UserKey = "ace"
            };
            await userManager.CreateAsync(appUser, SECRET_PASSWORD);
        }

        // 2. Use the EXISTING tracked appUser instance
        var testSite = new SiteEntity
        {
            SiteTitle = "The Vale Chronicles",
            GlobalTheme = "Dark",
            UserId = appUser.Id,
            User = appUser
        };

        var defaultTemplate = new TemplateEntity
        {
            Name = "Standard Story",
            ViewPath = "Default",
            DefaultStyles = "body { font-family: 'serif'; background-color: #1a1a1a; color: #eee; }"
        };

        var page1 = new PageEntity
        {
            PageTitle = "Prologue",
            Slug = "prologue",
            CachedContent = "<h2>The Beginning</h2><p>Narrative energy hummed through the air...</p>",
            SortOrder = 1,
            Site = testSite,
            Template = defaultTemplate
        };

        var page2 = new PageEntity
        {
            PageTitle = "Chapter One",
            Slug = "chapter-1",
            CachedContent = "<h2>The First Step</h2><p>Corvus Vale stepped into the light.</p>",
            SortOrder = 2,
            Site = testSite,
            Template = defaultTemplate
        };

        context.Pages.AddRange(page1, page2);
        await context.SaveChangesAsync();
    }
}
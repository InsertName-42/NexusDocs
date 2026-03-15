using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexusDocs.Data;
using NexusDocs.Models;
using Microsoft.Extensions.DependencyInjection;

public class SeedData
{
    public static async Task Seed(ApplicationDbContext context, IServiceProvider provider)
    {
        var userManager = provider.GetRequiredService<UserManager<AppUser>>();

        //1. Seed Tags
        if (!context.Tags.Any())
        {
            context.Tags.AddRange(
                new TagEntity { Name = "Markdown", IsEnabled = true },

                new TagEntity { Name = "Downloadable", IsEnabled = true, ScriptPath = "Downloadable" }

            );
            await context.SaveChangesAsync();
        }

        //2. Seed Templates
        if (!context.Templates.Any())
        {
            //Template: Default
            var defaultTemplate = new TemplateEntity
            {
                Name = "Default",
                ViewPath = "Default",
                DefaultStyles = "Default" //Load Default.css
            };

            //Fill in
            var storyTemplate = new TemplateEntity
            {
                Name = "Standard Story",
                ViewPath = "Default",
                DefaultStyles = "StandardStory"
            };
            var giftTemplate = new TemplateEntity
            {
                Name = "Gifts",
                ViewPath = "Gifts", 
                DefaultStyles = "Gifts"
            };

            context.Templates.AddRange(defaultTemplate, storyTemplate, giftTemplate);
            await context.SaveChangesAsync();
        }

        //3. Prevent duplicate page seeding
        if (await context.Pages.AnyAsync(p => p.Slug == "prologue")) { return; }

        //4. Setup User and Site
        const string SEED_EMAIL = "ace@example.com";
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
            await userManager.CreateAsync(appUser, "Password123!");
        }

        var testSite = new SiteEntity
        {
            SiteTitle = "The Vale Chronicles",
            GlobalTheme = "Dark",
            UserId = appUser.Id,
            User = appUser
        };

        //5. Fetch the template
        var activeTemplate = await context.Templates.FirstOrDefaultAsync(t => t.Name == "Default");

        //6. Seed Pages
        var page1 = new PageEntity
        {
            PageTitle = "Prologue",
            Slug = "prologue",
            CachedContent = "<h2>The Beginning</h2><p>Narrative energy hummed through the air...</p>",
            SortOrder = 1,
            Site = testSite,
            Template = activeTemplate
        };

        var page2 = new PageEntity
        {
            PageTitle = "Chapter One",
            Slug = "chapter-1",
            CachedContent = "<h2>The First Step</h2><p>Corvus Vale stepped into the light.</p>",
            SortOrder = 2,
            Site = testSite,
            Template = activeTemplate
        };

        context.Pages.AddRange(page1, page2);
        await context.SaveChangesAsync();
    }
}
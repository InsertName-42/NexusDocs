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

        //Seed Tags
        var tags = new List<TagEntity>
        {
            new TagEntity { Name = "Markdown", IsEnabled = true },
            new TagEntity { Name = "Downloadable", IsEnabled = true, ScriptPath = "downloadable" },
            new TagEntity { Name = "ExternalLinks", IsEnabled = true, ScriptPath = "external-links" }
        };

        foreach (var tag in tags)
        {
            var existingTag = await context.Tags.FirstOrDefaultAsync(t => t.Name == tag.Name);
            if (existingTag == null)
            {
                context.Tags.Add(tag);
            }
            else
            {
                //Update properties if they changed in seed data
                existingTag.ScriptPath = tag.ScriptPath;
                existingTag.IsEnabled = tag.IsEnabled;
                existingTag.Zone = tag.Zone;
            }
        }
        await context.SaveChangesAsync();

        //Seed Templates
        var templates = new List<TemplateEntity>
        {
            new TemplateEntity { Name = "Default", ViewPath = "Default", DefaultStyles = "Default" },
            new TemplateEntity { Name = "Image", ViewPath = "Image", DefaultStyles = "Image" },
            new TemplateEntity { Name = "Gifts", ViewPath = "Gifts", DefaultStyles = "Gifts" },
            new TemplateEntity { Name = "Calendar", ViewPath = "Calendar", DefaultStyles = "Calendar" },
            new TemplateEntity { Name = "Gallery", ViewPath = "Gallery", DefaultStyles = "Gallery" }
        };

        foreach (var temp in templates)
        {
            var existingTemp = await context.Templates.FirstOrDefaultAsync(t => t.Name == temp.Name);
            if (existingTemp == null)
            {
                context.Templates.Add(temp);
            }
            else
            {
                existingTemp.ViewPath = temp.ViewPath;
                existingTemp.DefaultStyles = temp.DefaultStyles;
            }
        }
        await context.SaveChangesAsync();

        //User & Site Seeding
        const string SEED_EMAIL = "admin@nexusdocs.com";
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

        //Seed Site (Check by Title or UserId)
        var testSite = await context.Sites.FirstOrDefaultAsync(s => s.UserId == appUser.Id);
        if (testSite == null)
        {
            testSite = new SiteEntity
            {
                SiteTitle = "The Vale Chronicles",
                GlobalTheme = "Dark",
                UserId = appUser.Id
            };
            context.Sites.Add(testSite);
            await context.SaveChangesAsync();
        }

        //Seed Pages (Check by Slug within the Site)
        var activeTemplate = await context.Templates.FirstAsync(t => t.Name == "Default");

        var pagesToSeed = new List<PageEntity>
        {
            new PageEntity { PageTitle = "Prologue", Slug = "prologue", SortOrder = 1, SiteId = testSite.SiteEntityId, TemplateId = activeTemplate.TemplateEntityId },
            new PageEntity { PageTitle = "Chapter One", Slug = "chapter-1", SortOrder = 2, SiteId = testSite.SiteEntityId, TemplateId = activeTemplate.TemplateEntityId }
        };

        foreach (var p in pagesToSeed)
        {
            if (!await context.Pages.AnyAsync(page => page.Slug == p.Slug && page.SiteId == testSite.SiteEntityId))
            {
                context.Pages.Add(p);
            }
        }

        await context.SaveChangesAsync();
    }
}
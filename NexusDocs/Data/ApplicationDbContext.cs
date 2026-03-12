using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NexusDocs.Models;

namespace NexusDocs.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<AppUser>(options)
    {
        public DbSet<SiteEntity> Sites { get; set; }

        public DbSet<PageEntity> Pages { get; set; }

        public DbSet<TemplateEntity> Templates { get; set; }

        public DbSet<TagEntity> Tags { get; set; }

        public DbSet<PageInteraction> PageInteractions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //Ensuring the Slug is unique within a specific Site
            builder.Entity<PageEntity>()
                .HasIndex(p => new { p.SiteId, p.Slug })
                .IsUnique();
        }
    }
}
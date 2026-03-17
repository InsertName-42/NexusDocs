using Google.Apis.Drive.v3;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexusDocs.Data;
using NexusDocs.Models;
using NexusDocs.Repositories;
using NexusDocs.Services;

var builder = WebApplication.CreateBuilder(args);

//Database Configuration 
var baseConn = builder.Configuration.GetConnectionString("MySqlConnection")
    ?? throw new InvalidOperationException("Connection string 'MySqlConnection' not found.");

var user = builder.Configuration["ProdDbUser"] ?? builder.Configuration["DbUser"];
var pass = builder.Configuration["ProdDbPassword"] ?? builder.Configuration["DbPassword"];
var finalConn = $"{baseConn};userid={user};password={pass};";

var serverVersion = ServerVersion.AutoDetect(finalConn);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(finalConn, serverVersion));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//Identity Configuration
builder.Services.AddDefaultIdentity<AppUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

//Services
builder.Services.AddScoped<GoogleSyncService>();

builder.Services.AddScoped<IPageRepository, PageRepository>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

//HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

//Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "public_site_home",
    pattern: "{userKey}",
    defaults: new { controller = "PublicPage", action = "Display", slug = "" });

app.MapControllerRoute(
    name: "public_site",
    pattern: "{userKey}/{slug}",
    defaults: new { controller = "PublicPage", action = "Display" });

app.MapRazorPages();

//Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await SeedData.Seed(context, services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
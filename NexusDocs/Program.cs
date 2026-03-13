using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Drive.v3;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexusDocs.Data;
using NexusDocs.Models;
using NexusDocs.Services;

var builder = WebApplication.CreateBuilder(args);

//1. Database Configuration 
var baseConn = builder.Configuration.GetConnectionString("MySqlConnection")
    ?? throw new InvalidOperationException("Connection string 'MySqlConnection' not found.");

var user = builder.Configuration["ProdDbUser"] ?? builder.Configuration["DbUser"];
var pass = builder.Configuration["ProdDbPassword"] ?? builder.Configuration["DbPassword"];
var finalConn = $"{baseConn};userid={user};password={pass};";
var serverVersion = ServerVersion.AutoDetect(finalConn);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(finalConn, serverVersion));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//2. Identity & Authentication 
builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Google:ClientSecret"]!;

    //Scope required for the admin to interact with their Google Drive
    options.Scope.Add(DriveService.Scope.DriveReadonly);
    options.SaveTokens = true;

    options.Events.OnRedirectToAuthorizationEndpoint = context =>
    {
        //Forces the consent screen to ensure refresh tokens are provided
        context.Response.Redirect(context.RedirectUri + "&prompt=consent");
        return Task.CompletedTask;
    };
});

//3. Custom Services
builder.Services.AddScoped<GoogleSyncService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

//4. HTTP Request Pipeline
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
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

//5. Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//Routing for public pages based on userKey and slug
app.MapControllerRoute(
    name: "public_site_home",
    pattern: "{userKey}",
    defaults: new { controller = "PublicPage", action = "Display", slug = "" });

app.MapControllerRoute(
    name: "public_site",
    pattern: "{userKey}/{slug}",
    defaults: new { controller = "PublicPage", action = "Display" });

app.MapRazorPages();

//6. Seed database
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
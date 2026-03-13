using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Drive.v3;
using Microsoft.AspNetCore.Authentication;
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

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(finalConn));

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
    options.Scope.Add(DriveService.Scope.DriveFile);
    options.Scope.Add(DriveService.Scope.DriveReadonly);
    options.SaveTokens = true;
});

//Google API Custom Services
builder.Services.AddGoogleAuthProvider();

builder.Services.AddScoped<GoogleSyncService>();

builder.Services.AddScoped(sp => {
    var auth = sp.GetRequiredService<IGoogleAuthProvider>();
    var credential = auth.GetCredentialAsync().Result;

    return new DriveService(new Google.Apis.Services.BaseClientService.Initializer
    {
        HttpClientInitializer = credential,
        ApplicationName = "NexusDocs"
    });
});

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
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

//5. Routing
app.MapControllerRoute(
    name: "dashboard",
    pattern: "dashboard/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//Route for the user's home site (/ace)
app.MapControllerRoute(
    name: "public_site_home",
    pattern: "{userKey}",
    defaults: new { controller = "PublicPage", action = "Display", slug = "" });

//Route for specific pages (/ace/prologue)
app.MapControllerRoute(
    name: "public_site",
    pattern: "{userKey}/{slug}",
    defaults: new { controller = "PublicPage", action = "Display" });

app.MapRazorPages();

app.Run();
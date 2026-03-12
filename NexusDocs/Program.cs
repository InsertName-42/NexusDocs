using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexusDocs.Data;
using NexusDocs.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var baseConn = builder.Configuration.GetConnectionString("MySqlConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

var user = builder.Configuration["ProdDbUser"] ?? builder.Configuration["DbUser"];
var pass = builder.Configuration["ProdDbPassword"] ?? builder.Configuration["DbPassword"];

//Build the complete MySQL connection string
var finalConn = $"{baseConn};userid={user};password={pass};";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(finalConn));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();


app.MapControllerRoute(
    name: "dashboard",
    pattern: "dashboard/{controller=Home}/{action=Index}/{id?}");

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

app.MapRazorPages()
   .WithStaticAssets();

app.Run();

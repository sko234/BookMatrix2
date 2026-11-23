using BookMatrix.Data;
using BookMatrix.Models;
using BookMatrix.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    // Every POST (including the borrow/return calls) must carry the anti-forgery token.
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.Cookie.Name = "BookMatrix.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });

// Write Cyrillic text into the HTML as-is instead of as &#x...; entities (HTML-significant characters are still escaped).
builder.Services.AddWebEncoders(options => options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

// SQLite database file (created automatically on first run).
var configuredPath = builder.Configuration["Database:Path"] ?? "App_Data/bookmatrix.db";
var databasePath = Path.Combine(builder.Environment.ContentRootPath, configuredPath);
Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
builder.Services.AddDbContext<BookMatrixContext>(options => options.UseSqlite($"Data Source={databasePath}"));

builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAdminAccountService, AdminAccountService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IPasswordHasher<UserAccount>, PasswordHasher<UserAccount>>();
builder.Services.AddScoped<IPasswordHasher<AdminAccount>, PasswordHasher<AdminAccount>>();
builder.Services.AddScoped<UiText>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    DbInitializer.Initialize(
        scope.ServiceProvider.GetRequiredService<BookMatrixContext>(),
        app.Configuration,
        scope.ServiceProvider.GetRequiredService<IPasswordHasher<AdminAccount>>());
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

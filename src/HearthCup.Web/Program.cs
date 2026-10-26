using System.Globalization;
using System.Threading.RateLimiting;
using HearthCup.DataAccess.Data;
using HearthCup.DataAccess.DbInitializer;
using HearthCup.DataAccess.Repository;
using HearthCup.DataAccess.Security;
using HearthCup.Models;
using HearthCup.Utility.Constants;
using HearthCup.Utility.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// en-GB keeps the pound formatting identical on every machine.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-GB");
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-GB");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Shop"),
        sql => sql.EnableRetryOnFailure(3)));

// Composition root: the only place that knows which class fills which port.
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasherAdapter>();

builder.Services.AddScoped<ShopService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<OrderService>();

builder.Services.AddSingleton(new DbInitializerOptions
{
    ContentRootPath = builder.Environment.ContentRootPath,
    AdminEmail = builder.Configuration["Admin:Email"] ?? "",
    AdminPassword = builder.Configuration["Admin:Password"] ?? "",
    IsDevelopment = builder.Environment.IsDevelopment()
});
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.Name = "HearthCup.Account";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy(SD.LoginRateLimitPolicy, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "local",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<IDbInitializer>().InitializeAsync();
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Database initialization failed. Check SQL Server Express and ConnectionStrings:Shop.");
}

if (args.Contains("--import-amazon"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<IDbInitializer>().ImportCatalogueAsync();
    app.Logger.LogInformation("Imported 18 reviewed Amazon product references; original catalogue backed up locally.");
    return;
}

app.UseExceptionHandler("/Home/Error");

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Security headers, including a CSP that forbids inline script and inline style.
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; img-src 'self' data:; style-src 'self'; script-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
    await next();
});

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// Readiness probe: 503 when SQL Server is unreachable.
app.MapGet("/health", async (ApplicationDbContext db) =>
{
    try
    {
        return await db.Database.CanConnectAsync()
            ? Results.Text("Healthy")
            : Results.Text("Database unavailable", statusCode: 503);
    }
    catch
    {
        return Results.Text("Database unavailable", statusCode: 503);
    }
});

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();

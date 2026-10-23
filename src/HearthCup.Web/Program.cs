using System.Globalization;
using System.Threading.RateLimiting;
using HearthCup.Application;
using HearthCup.Infrastructure;
using HearthCup.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
CultureInfo.DefaultThreadCurrentCulture=CultureInfo.GetCultureInfo("en-GB");
CultureInfo.DefaultThreadCurrentUICulture=CultureInfo.GetCultureInfo("en-GB");
var builder=WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(o=>o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddDbContext<ShopDbContext>(o=>o.UseSqlServer(builder.Configuration.GetConnectionString("Shop"),s=>s.EnableRetryOnFailure(3)));
builder.Services.AddScoped<IProductRepository,ProductRepository>();builder.Services.AddScoped<ShopService>();
builder.Services.AddDistributedMemoryCache();builder.Services.AddSession(o=>{o.IdleTimeout=TimeSpan.FromHours(4);o.Cookie.Name="HearthCup.Basket";o.Cookie.HttpOnly=true;o.Cookie.IsEssential=true;o.Cookie.SameSite=SameSiteMode.Lax;});
builder.Services.AddHttpContextAccessor();builder.Services.AddScoped<BasketStore>();
builder.Services.AddSingleton<AdminCredentials>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o=>{o.LoginPath="/Account/Login";o.AccessDeniedPath="/Account/Login";o.Cookie.Name="HearthCup.Manager";o.Cookie.HttpOnly=true;o.Cookie.SameSite=SameSiteMode.Lax;o.ExpireTimeSpan=TimeSpan.FromMinutes(30);o.SlidingExpiration=true;});
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(o=>{o.RejectionStatusCode=429;o.AddPolicy("login",ctx=>RateLimitPartition.GetFixedWindowLimiter(ctx.Connection.RemoteIpAddress?.ToString()??"local",_=>new FixedWindowRateLimiterOptions{PermitLimit=5,Window=TimeSpan.FromMinutes(1),QueueLimit=0}));});
var app=builder.Build();
// Resolve before serving. Development creates a local-only manager credential; production requires configuration.
_ = app.Services.GetRequiredService<AdminCredentials>();
try{using var scope=app.Services.CreateScope();await SeedData.InitializeAsync(scope.ServiceProvider.GetRequiredService<ShopDbContext>(),Path.Combine(app.Environment.ContentRootPath,"Data","catalogue.json"));}
catch(Exception ex){app.Logger.LogError(ex,"Database initialization failed. Check SQL Server Express and ConnectionStrings:Shop.");}
if(args.Contains("--import-amazon"))
{
 using var scope=app.Services.CreateScope();
 await CatalogueImporter.ImportAsync(scope.ServiceProvider.GetRequiredService<ShopDbContext>(),Path.Combine(app.Environment.ContentRootPath,"Data","catalogue.json"),Path.Combine(app.Environment.ContentRootPath,".local","catalogue-backups"));
 app.Logger.LogInformation("Imported 18 reviewed Amazon product references; original catalogue backed up locally.");
 return;
}
app.UseExceptionHandler("/Home/Error");
if(!app.Environment.IsDevelopment()){app.UseHsts();app.UseHttpsRedirection();}
app.Use(async(ctx,next)=>{ctx.Response.Headers["X-Content-Type-Options"]="nosniff";ctx.Response.Headers["Referrer-Policy"]="strict-origin-when-cross-origin";ctx.Response.Headers["Content-Security-Policy"]="default-src 'self'; img-src 'self' data:; style-src 'self'; script-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";await next();});
app.UseStaticFiles();app.UseRouting();app.UseSession();app.UseAuthentication();app.UseAuthorization();app.UseRateLimiter();
app.MapGet("/health",async(ShopDbContext db)=>{try{return await db.Database.CanConnectAsync()?Results.Text("Healthy"):Results.Text("Database unavailable",statusCode:503);}catch{return Results.Text("Database unavailable",statusCode:503);}});
app.MapControllerRoute("default","{controller=Home}/{action=Index}/{id?}");
app.Run();

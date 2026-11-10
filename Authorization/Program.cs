using System.Diagnostics;
using Authorization.Features.Authentication;
using Authorization.Features.Favorites;
using Authorization.Features.PasswordReset;
using Authorization.Features.Products;
using Authorization.Features.Rating;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("AuthorizationDatabase")
    ?? throw new InvalidOperationException("Connection string 'AuthorizationDatabase' is missing.");
string smtpEmail = builder.Configuration["Smtp:Email"]
    ?? throw new InvalidOperationException("SMTP email is missing. Configure the Smtp:Email user secret.");
string smtpAppPassword = builder.Configuration["Smtp:AppPassword"]
    ?? throw new InvalidOperationException("SMTP password is missing. Configure the Smtp:AppPassword user secret.");

builder.Services.AddSingleton(new AuthorizationService(connectionString));
builder.Services.AddSingleton(new ProductSearchService(connectionString));
builder.Services.AddSingleton(new ProductPurchase.ProductPurchase(connectionString));
builder.Services.AddSingleton(new Feedback.RatingService(connectionString));
builder.Services.AddSingleton(new Feedback.MessageService(connectionString));
builder.Services.AddSingleton(new FavoritesService(connectionString));
builder.Services.AddSingleton(new PasswordResetService(connectionString, smtpEmail, smtpAppPassword));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string? googleClientId = builder.Configuration["Authentication:Google:ClientId"];
string? googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
bool googleConfigured = !string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret);

AuthenticationBuilder authentication = builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login.html";
        options.Cookie.Name = "Nexora.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

if (googleConfigured)
{
    authentication.AddGoogle(options =>
    {
        options.ClientId = googleClientId!;
        options.ClientSecret = googleClientSecret!;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    });
}

builder.Services.AddAuthorization();
WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
string? productPhotosDirectory = builder.Configuration["ProductPhotos:Directory"];
if (!string.IsNullOrWhiteSpace(productPhotosDirectory))
{
    string photosPath = Path.GetFullPath(productPhotosDirectory, app.Environment.ContentRootPath);
    if (!Directory.Exists(photosPath))
        throw new DirectoryNotFoundException($"Product photo directory does not exist: {photosPath}");

    var photosProvider = new PhysicalFileProvider(photosPath);
    app.Lifetime.ApplicationStopped.Register(photosProvider.Dispose);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = photosProvider,
        RequestPath = "/uploads/products",
        OnPrepareResponse = context => context.Context.Response.Headers.CacheControl = "no-cache"
    });
}
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        if (!app.Environment.IsDevelopment()) return;
        context.Context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        context.Context.Response.Headers.Pragma = "no-cache";
        context.Context.Response.Headers.Expires = "0";
    }
});
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/login.html"));
app.MapAuthenticationEndpoints(googleConfigured);
app.MapPasswordResetEndpoints();
app.MapProductEndpoints();
app.MapScoringEndpoints();

if (app.Environment.IsDevelopment())
{
    app.Lifetime.ApplicationStarted.Register(() => Process.Start(new ProcessStartInfo
    {
        FileName = "http://localhost:5187/login.html",
        UseShellExecute = true
    }));
}

app.Run();

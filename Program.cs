using Authorization.Models;
using Authorization.Services;
using SearchSystem.Services;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("AuthorizationDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'AuthorizationDatabase' is missing. Configure it in appsettings.json or the ConnectionStrings__AuthorizationDatabase environment variable.");

builder.Services.AddSingleton(new AuthorizationService(connectionString));
builder.Services.AddSingleton(new SearchSystem.Services.SearchSystem(connectionString));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

// Always send the application root to the login page.
app.MapGet("/", () => Results.Redirect("/login.html"));

app.MapPost("/api/login", (LoginRequest request, AuthorizationService authorizationService) =>
{
    Customer? customer = authorizationService.Login(request.Email, request.Password);

    if (customer == null)
        return Results.Unauthorized();

    return Results.Ok(new ApiResponse($"Welcome, {customer.Name}!"));
});
// The frontend sends registration data to this URL.
app.MapPost("/api/register", (RegisterRequest request, AuthorizationService authorizationService) =>
{
    string? errorMessage = authorizationService.Register(request);

    if (errorMessage != null)
        return Results.BadRequest(new ApiResponse(errorMessage));

    return Results.Ok(new ApiResponse("Registration successful! You can now log in."));
});

app.MapGet("/api/products/search", (string? prefix, SearchSystem.Services.SearchSystem searchSystem) =>
{
    string searchPrefix = prefix?.Trim() ?? "";
    return Results.Ok(searchSystem.Search(searchPrefix));
});

// `dotnet run` starts the web server but does not normally open a browser.
// Open the login page once the server is ready.
if (app.Environment.IsDevelopment())
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "http://localhost:5187/login.html",
            UseShellExecute = true
        });
    });
}

app.Run();

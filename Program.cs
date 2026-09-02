using Authorization.Models;
using Authorization.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using SearchSystem.Services;
using System.Diagnostics;
using RestoringaPassword;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("AuthorizationDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'AuthorizationDatabase' is missing. Configure it in appsettings.json or the ConnectionStrings__AuthorizationDatabase environment variable.");

string smtpEmail = builder.Configuration["Smtp:Email"]
    ?? throw new InvalidOperationException(
        "SMTP email is missing. Configure it with 'dotnet user-secrets set Smtp:Email you@gmail.com'.");
string smtpAppPassword = builder.Configuration["Smtp:AppPassword"]
    ?? throw new InvalidOperationException(
        "SMTP app password is missing. Configure it with 'dotnet user-secrets set Smtp:AppPassword <app-password>'.");

builder.Services.AddSingleton(new AuthorizationService(connectionString));
builder.Services.AddSingleton(new SearchSystem.Services.SearchSystem(connectionString));
builder.Services.AddSingleton(new CodeGenerator(connectionString, smtpEmail, smtpAppPassword));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string? googleClientId = builder.Configuration["Authentication:Google:ClientId"];
string? googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
bool googleSignInConfigured = !string.IsNullOrWhiteSpace(googleClientId)
    && !string.IsNullOrWhiteSpace(googleClientSecret);

AuthenticationBuilder authentication = builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login.html";
        options.Cookie.Name = "Nexora.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

if (googleSignInConfigured)
{
    authentication.AddGoogle(options =>
    {
        options.ClientId = googleClientId!;
        options.ClientSecret = googleClientSecret!;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    });
}

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        if (app.Environment.IsDevelopment())
        {
            context.Context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
            context.Context.Response.Headers.Pragma = "no-cache";
            context.Context.Response.Headers.Expires = "0";
        }
    }
});
app.UseAuthentication();
app.UseAuthorization();

// Always send the application root to the login page.
app.MapGet("/", () => Results.Redirect("/login.html"));

app.MapGet("/auth/google", () =>
{
    if (!googleSignInConfigured)
        return Results.Redirect("/login.html?google=not-configured");

    return Results.Challenge(
        new AuthenticationProperties { RedirectUri = "/product-search/index.html" },
        [GoogleDefaults.AuthenticationScheme]);
});

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

app.MapPost("/api/password-reset/code", (string? email, CodeGenerator codeGenerator, ILogger<Program> logger) =>
{
    if (string.IsNullOrWhiteSpace(email)
        || !System.Net.Mail.MailAddress.TryCreate(email.Trim(), out _))
    {
        return Results.BadRequest(new ApiResponse("Please enter a valid email address."));
    }

    try
    {
        codeGenerator.sendCode(email.Trim());
        return Results.Ok(new ApiResponse("Verification code sent. Check your inbox."));
    }
    catch (Exception exception)
    {
        logger.LogError(exception, "Failed to send a password-reset code to {Email}", email.Trim());
        return Results.Problem(
            title: "Unable to send verification code",
            detail: "The email could not be sent right now. Please try again.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapPost("/api/password-reset/verify", (VerifyResetCodeRequest request, CodeGenerator codeGenerator) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || request.Code is < 100000 or > 999999)
        return Results.BadRequest(new ApiResponse("Enter a valid email and six-digit code."));

    return codeGenerator.VerifyCode(request.Email, request.Code)
        ? Results.Ok(new ApiResponse("Code verified."))
        : Results.BadRequest(new ApiResponse("The verification code is invalid or has expired."));
});

app.MapPost("/api/password-reset", (ResetPasswordRequest request, CodeGenerator codeGenerator) =>
{
    if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
        return Results.BadRequest(new ApiResponse("Password must contain at least 8 characters."));

    string? error = codeGenerator.ResetPassword(request.Email, request.Code, request.NewPassword);
    return error is null
        ? Results.Ok(new ApiResponse("Password reset successfully."))
        : Results.BadRequest(new ApiResponse(error));
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

record VerifyResetCodeRequest(string Email, int Code);
record ResetPasswordRequest(string Email, int Code, string NewPassword);

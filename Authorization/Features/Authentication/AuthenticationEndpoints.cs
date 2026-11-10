using Authorization.Contracts;
using Authorization.Features.Authentication.Contracts;
using Authorization.Features.Authentication.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Authorization.Features.Authentication;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints, bool googleConfigured)
    {
        endpoints.MapGet("/auth/google", () => googleConfigured
            ? Results.Challenge(new AuthenticationProperties { RedirectUri = "/product-search/index.html" }, [GoogleDefaults.AuthenticationScheme])
            : Results.Redirect("/login.html?google=not-configured"));

        endpoints.MapPost("/api/login", async (LoginRequest request, AuthorizationService service, HttpContext context) =>
        {
            Customer? customer = service.Login(request.Email, request.Password);
            if (customer is null)
                return Results.Unauthorized();

            ClaimsIdentity identity = new(new[]
            {
                new Claim("CustomerId", customer.Id.ToString()),
                new Claim(ClaimTypes.Email, customer.Email),
                new Claim(ClaimTypes.Name, customer.Name)
            }, CookieAuthenticationDefaults.AuthenticationScheme);

            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            return Results.Ok(new ApiResponse($"Welcome, {customer.Name}!"));
        });

        endpoints.MapPost("/api/register", (RegisterRequest request, AuthorizationService service) =>
        {
            string? error = service.Register(request);
            return error is null
                ? Results.Ok(new ApiResponse("Registration successful! You can now log in."))
                : Results.BadRequest(new ApiResponse(error));
        });

        return endpoints;
    }
}

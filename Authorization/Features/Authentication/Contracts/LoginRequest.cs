namespace Authorization.Features.Authentication.Contracts;

public sealed record LoginRequest(string Email, string Password);

namespace Authorization.Features.Authentication.Contracts;

public sealed record RegisterRequest(
    string Name,
    string Email,
    string Phone,
    string Address,
    int Id,
    string Password);

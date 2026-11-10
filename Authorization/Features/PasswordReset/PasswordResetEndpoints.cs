using Authorization.Contracts;
using Authorization.Features.PasswordReset.Contracts;

namespace Authorization.Features.PasswordReset;

public static class PasswordResetEndpoints
{
    public static IEndpointRouteBuilder MapPasswordResetEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/password-reset/code", (string? email, PasswordResetService service, ILogger<PasswordResetService> logger) =>
        {
            if (string.IsNullOrWhiteSpace(email) || !System.Net.Mail.MailAddress.TryCreate(email.Trim(), out _))
                return Results.BadRequest(new ApiResponse("Please enter a valid email address."));
            try
            {
                service.SendCode(email.Trim());
                return Results.Ok(new ApiResponse("Verification code sent. Check your inbox."));
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to send a password-reset code to {Email}", email.Trim());
                return Results.Problem("The email could not be sent right now. Please try again.", statusCode: 503);
            }
        });

        endpoints.MapPost("/api/password-reset/verify", (VerifyResetCodeRequest request, PasswordResetService service) =>
            string.IsNullOrWhiteSpace(request.Email) || request.Code is < 100000 or > 999999
                ? Results.BadRequest(new ApiResponse("Enter a valid email and six-digit code."))
                : service.VerifyCode(request.Email, request.Code)
                    ? Results.Ok(new ApiResponse("Code verified."))
                    : Results.BadRequest(new ApiResponse("The verification code is invalid or has expired.")));

        endpoints.MapPost("/api/password-reset", (ResetPasswordRequest request, PasswordResetService service) =>
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
                return Results.BadRequest(new ApiResponse("Password must contain at least 8 characters."));
            string? error = service.ResetPassword(request.Email, request.Code, request.NewPassword);
            return error is null ? Results.Ok(new ApiResponse("Password reset successfully.")) : Results.BadRequest(new ApiResponse(error));
        });

        return endpoints;
    }
}

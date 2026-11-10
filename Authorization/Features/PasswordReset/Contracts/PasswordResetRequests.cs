namespace Authorization.Features.PasswordReset.Contracts;

public sealed record VerifyResetCodeRequest(string Email, int Code);

public sealed record ResetPasswordRequest(string Email, int Code, string NewPassword);

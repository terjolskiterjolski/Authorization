using System.Collections.Concurrent;
using System.Security.Cryptography;
using Authorization.Infrastructure.Persistence;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Npgsql;
using NpgsqlTypes;

namespace Authorization.Features.PasswordReset;

public sealed class PasswordResetService
{
    private readonly string connectionString;
    private readonly string smtpEmail;
    private readonly string smtpAppPassword;
    private readonly ConcurrentDictionary<string, VerificationCode> verificationCodes = new();

    private sealed record VerificationCode(int Code, DateTime ExpiresAtUtc);

    public PasswordResetService(string connectionString, string smtpEmail, string smtpAppPassword)
    {
        this.connectionString = connectionString;
        this.smtpEmail = smtpEmail;
        this.smtpAppPassword = smtpAppPassword;
    }

    public void SendCode(string email)
    {
        int code = RandomNumberGenerator.GetInt32(100_000, 1_000_000);
        SendMessage(email, "Your Nexora verification code", $"Your verification code is {code}");
        verificationCodes[NormalizeEmail(email)] =
            new VerificationCode(code, DateTime.UtcNow.AddMinutes(10));
    }

    public bool VerifyCode(string email, int code) =>
        verificationCodes.TryGetValue(NormalizeEmail(email), out VerificationCode? saved) &&
        saved.ExpiresAtUtc > DateTime.UtcNow &&
        saved.Code == code;

    public string? ResetPassword(string email, int code, string newPassword)
    {
        if (!VerifyCode(email, code))
            return "The verification code is invalid or has expired.";

        using NpgsqlConnection connection = new(connectionString);
        connection.Open();
        using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = SqlFileLoader.Load("restorePassword.sql");
        command.Parameters.AddWithValue("email", email.Trim());

        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            newPassword, salt, 100_000, HashAlgorithmName.SHA256, 32);
        command.Parameters.AddWithValue("passwordHash", NpgsqlDbType.Bytea, hash);
        command.Parameters.AddWithValue("passwordSalt", NpgsqlDbType.Bytea, salt);

        try
        {
            if (command.ExecuteNonQuery() == 0)
                return "No rows were updated. Please check the email and try again.";
        }
        catch (PostgresException exception)
        {
            return "An error occurred while updating the password: " + exception.Message;
        }

        verificationCodes.TryRemove(NormalizeEmail(email), out _);
        return null;
    }

    private void SendMessage(string email, string subject, string text)
    {
        MimeMessage message = new();
        message.From.Add(MailboxAddress.Parse(smtpEmail));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = text };

        using SmtpClient smtp = new();
        smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        smtp.Authenticate(smtpEmail, smtpAppPassword);
        smtp.Send(message);
        smtp.Disconnect(true);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}

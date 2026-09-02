using System.Security.Cryptography;
using System.Collections.Concurrent;
using Npgsql;
using Authorization.Services;
using NpgsqlTypes;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;


namespace RestoringaPassword{
    

    public class CodeGenerator
    {
    private readonly string smtpEmail;
    private readonly string smtpAppPassword;
    private readonly ConcurrentDictionary<string, VerificationCode> verificationCodes = new();
    public string connectionString { get; set; }

    private sealed record VerificationCode(int Code, DateTime ExpiresAtUtc);


    public CodeGenerator(string connectionString, string smtpEmail, string smtpAppPassword)
    {
        this.connectionString = connectionString;
        this.smtpEmail = smtpEmail;
        this.smtpAppPassword = smtpAppPassword;
    }
    public int[] GenerateCode(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        int code = RandomNumberGenerator.GetInt32(100000, 1000000);
        return code.ToString().Select(character => character - '0').ToArray();
    }
    private static string LoadSql(string fileName)
    {
            string sqlPath = Path.Combine(AppContext.BaseDirectory, "Database", fileName);
            return File.ReadAllText(sqlPath);
    }
    public bool VerifyCode(string email, int code)
    {
        string key = email.Trim().ToLowerInvariant();
        return verificationCodes.TryGetValue(key, out VerificationCode? saved)
            && saved.ExpiresAtUtc > DateTime.UtcNow
            && saved.Code == code;
    }

    public string? ResetPassword(string email, int code, string newPassword)
    {
        if (!VerifyCode(email, code))
        {
            return "The verification code is invalid or has expired.";
        }
        using NpgsqlConnection Connection = new NpgsqlConnection(connectionString);
        Connection.Open();
        using NpgsqlCommand command = Connection.CreateCommand();
        command.CommandText = LoadSql("restorePassword.sql");
        command.Parameters.AddWithValue("email", email.Trim());
        byte[] passwordSalt = RandomNumberGenerator.GetBytes(16);
        byte[] passwordHash = Rfc2898DeriveBytes.Pbkdf2(
            newPassword, passwordSalt, 100_000, HashAlgorithmName.SHA256, 32);
        command.Parameters.AddWithValue("passwordHash", NpgsqlDbType.Bytea, passwordHash);
        command.Parameters.AddWithValue("passwordSalt", NpgsqlDbType.Bytea, passwordSalt);

        int affectedRows = 0;
        try{
           affectedRows = command.ExecuteNonQuery();
        }
        catch (PostgresException exception)
        {
            return "An error occurred while updating the password: " + exception.Message;
        }
        if (affectedRows == 0)
        {
            return "No rows were updated. Please check the email and try again.";
        }
        verificationCodes.TryRemove(email.Trim().ToLowerInvariant(), out _);
        return null;        
    }
    public void sendMessage(string email,string subject, string text)
    {
          
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(smtpEmail));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new TextPart("plain")
            {
                Text = text
            };
        using (var smtp = new MailKit.Net.Smtp.SmtpClient())
        {
             smtp.Connect(
            "smtp.gmail.com",
            587,
            SecureSocketOptions.StartTls
        );

        smtp.Authenticate(smtpEmail, smtpAppPassword);

        smtp.Send(message);

        smtp.Disconnect(true);

       }
    
    }
    public void sendCode(String gmail)
    {
        string code = string.Concat(GenerateCode(gmail));
        sendMessage(gmail, "Your Nexora verification code", $"Your verification code is {code}");
        verificationCodes[gmail.Trim().ToLowerInvariant()] =
            new VerificationCode(int.Parse(code), DateTime.UtcNow.AddMinutes(10));
    }

    
    



           

}
}

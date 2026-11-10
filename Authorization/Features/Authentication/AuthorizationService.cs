using System.Security.Cryptography;
using Authorization.Features.Authentication.Contracts;
using Authorization.Features.Authentication.Models;
using Authorization.Infrastructure.Persistence;
using Npgsql;
using NpgsqlTypes;

namespace Authorization.Features.Authentication;

public sealed class AuthorizationService
{
    private readonly string connectionString;

    public AuthorizationService(string connectionString)
    {
        this.connectionString = connectionString;
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using NpgsqlConnection connection = new(connectionString);
        connection.Open();
        using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = SqlFileLoader.Load("CreateCustomersTable.sql");
        command.ExecuteNonQuery();
    }

    public string? Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return "Name, email, and password are required.";
        }

        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] passwordHash = HashPassword(request.Password, salt);

        using NpgsqlConnection connection = new(connectionString);
        connection.Open();
        using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = SqlFileLoader.Load("InsertCustomer.sql");
        command.Parameters.AddWithValue("customerId", request.Id); 
        command.Parameters.AddWithValue("name", request.Name.Trim());
        command.Parameters.AddWithValue("email", request.Email.Trim());
        command.Parameters.AddWithValue("phone", request.Phone.Trim());
        command.Parameters.AddWithValue("address", request.Address.Trim());
        command.Parameters.AddWithValue("passwordHash", NpgsqlDbType.Bytea, passwordHash);
        command.Parameters.AddWithValue("passwordSalt", NpgsqlDbType.Bytea, salt);
        command.Parameters.AddWithValue("createdAtUtc", DateTime.UtcNow);

        try
        {
            command.ExecuteNonQuery();
            return null;
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return "This email is already registered.";
        }
    }

    public Customer? Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
            return null;

        using NpgsqlConnection connection = new(connectionString);
        connection.Open();
        using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = SqlFileLoader.Load("FindCustomerByEmail.sql");
        command.Parameters.AddWithValue("email", email.Trim());

        using NpgsqlDataReader reader = command.ExecuteReader();
        if (!reader.Read())
            return null;

        byte[] suppliedHash = HashPassword(password, reader.GetFieldValue<byte[]>(6));
        if (!CryptographicOperations.FixedTimeEquals(reader.GetFieldValue<byte[]>(5), suppliedHash))
            return null;

        return new Customer
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Email = reader.GetString(2),
            Phone = reader.GetString(3),
            Address = reader.GetString(4)
        };
    }

    private static byte[] HashPassword(string password, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
}

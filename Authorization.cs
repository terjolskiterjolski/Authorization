namespace Authorization.Models
{
    public class Customer
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
        public int Id { get; set; }
    }

    // JSON expected from the registration form.
    public class RegisterRequest
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
        public int Id { get; set; }
        public string Password { get; set; } = "";
    }

    // JSON expected from the login form.
    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    // JSON returned to the frontend.
    public class ApiResponse
    {
        public string Message { get; set; }

        public ApiResponse(string message)
        {
            Message = message;
        }
    }
}

namespace Authorization.Services
{
    using Authorization.Models;
    using Npgsql;
    using NpgsqlTypes;
    using System.Security.Cryptography;

    public class AuthorizationService
    {
        private readonly string connectionString;

        private static string LoadSql(string fileName)
        {
            string sqlPath = Path.Combine(AppContext.BaseDirectory, "Database", fileName);
            return File.ReadAllText(sqlPath);
        }

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
            command.CommandText = LoadSql("CreateCustomersTable.sql");
            command.ExecuteNonQuery();
        }

        // Returns null when registration succeeds, or an error message when it fails.
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
            command.CommandText = LoadSql("InsertCustomer.sql");
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
            command.CommandText = LoadSql("FindCustomerByEmail.sql");
            command.Parameters.AddWithValue("email", email.Trim());

            using NpgsqlDataReader reader = command.ExecuteReader();
            if (!reader.Read())
                return null;

            byte[] storedHash = reader.GetFieldValue<byte[]>(5);
            byte[] salt = reader.GetFieldValue<byte[]>(6);
            byte[] suppliedHash = HashPassword(password, salt);

            if (!CryptographicOperations.FixedTimeEquals(storedHash, suppliedHash))
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

        private static byte[] HashPassword(string password, byte[] salt)
        {
            return Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                100_000,
                HashAlgorithmName.SHA256,
                32);
        }
    }
}

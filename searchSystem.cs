using Authorization.Models;
using Npgsql;

namespace SearchSystem.Services
{
    public class SearchSystem
    {
        private readonly string connectionString;

        public SearchSystem(string connectionString)
        {
            this.connectionString = connectionString;
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using NpgsqlConnection connection = new(connectionString);
            connection.Open();

            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = LoadSql("CreateProductsTable.sql");
            command.ExecuteNonQuery();

            command.CommandText = LoadSql("InsertProducts.sql");
            command.ExecuteNonQuery();
        }
        private static string LoadSql(string fileName)
        {
            string sqlPath = Path.Combine(AppContext.BaseDirectory, "Database", fileName);
            return File.ReadAllText(sqlPath);
        }
        public List<Product> Search(string value)
        {
            using NpgsqlConnection connection = new(connectionString);
            connection.Open();

            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = LoadSql("FindProductsByPrefix.sql");
            command.Parameters.AddWithValue("prefix", value);

            using NpgsqlDataReader reader = command.ExecuteReader();
            List<Product> products = new();

            while (reader.Read())
            {
                products.Add(new Product
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    Price = reader.GetDecimal(3),
                    StockQuantity = reader.GetInt32(4),
                    CreatedAtUtc = reader.GetDateTime(5)
                });
            }

            return products;
        }
    }   
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}

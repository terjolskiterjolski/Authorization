using System;
using System.Threading.Tasks;
using Npgsql;


namespace UploadingProducts
{
    public class ProductAdder
    {
        private readonly string _connectionString;

        public ProductAdder(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task AddProductAsync(
            int productId,
            string productName,
            decimal price,
            int stockQuantity,
            string description,
            string sex)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentException("Product name is required.");

            if (price < 0)
                throw new ArgumentException("Price cannot be negative.");

            if (stockQuantity < 0)
                throw new ArgumentException("Stock quantity cannot be negative.");

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description is required.");

            if (sex is not ("Men" or "Women" or "Unisex"))
                throw new ArgumentException("Sex must be Men, Women, or Unisex.");
            const string sql = @"
                INSERT INTO public.products
                    (product_id, name, description, price, stock_quantity, created_at_utc, sex)
                VALUES
                    (@id, @name, @description, @price, @stock, CURRENT_TIMESTAMP, @sex);";

            await using var connection =
                new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("id", productId);
            command.Parameters.AddWithValue("name", productName.Trim());
            command.Parameters.AddWithValue("description", description.Trim());
            command.Parameters.AddWithValue("price", price);
            command.Parameters.AddWithValue("stock", stockQuantity);
            command.Parameters.AddWithValue("sex", sex);

            await command.ExecuteNonQueryAsync();
        }
    };

}


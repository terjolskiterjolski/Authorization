namespace SelledPerfumes;


internal record PurchasedProduct(int ProductId, string ProductName, decimal Price, int StockQuantity, string Description, string Sex, int CustomerId);

internal class PurchasedProducts
{
    readonly string connectionString;
    public PurchasedProducts(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public async Task<IEnumerable<PurchasedProduct>> GetPurchasedProductsAsync(int customerId)
    {
        Npgsql.NpgsqlConnectionStringBuilder builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
        Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection(builder.ConnectionString);
        Npgsql.NpgsqlCommand command = new Npgsql.NpgsqlCommand("SELECT p.product_id, p.name, p.price, p.stock_quantity, p.description, p.sex, c.customer_id FROM products p JOIN customers c ON c.customer_id = @customerId", connection);
        command.Parameters.AddWithValue("@customerId", customerId);

        return await Task.Run(() =>
        {
            var purchasedProducts = new List<PurchasedProduct>();

            try
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int productId = reader.GetInt32(0);
                        string productName = reader.GetString(1);
                        decimal price = reader.GetDecimal(2);
                        int stockQuantity = reader.GetInt32(3);
                        string description = reader.GetString(4);
                        string sex = reader.GetString(5);
                        int customerIdFromDb = reader.GetInt32(6);

                        purchasedProducts.Add(new PurchasedProduct(productId, productName, price, stockQuantity, description, sex, customerIdFromDb));
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                Console.WriteLine($"Error fetching purchased products: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }

            return purchasedProducts.AsEnumerable();
        });
    }   

    public async Task<IEnumerable<PurchasedProduct>>? GetProductsSortedByTime()
    {
        Npgsql.NpgsqlConnectionStringBuilder builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
        Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection(builder.ConnectionString);
        Npgsql.NpgsqlCommand command = new Npgsql.NpgsqlCommand("SELECT p.product_id, p.name, p.price, p.stock_quantity, p.description, p.sex, c.customer_id FROM products p JOIN customers c ON c.customer_id = @customerId ORDER BY p.created_at DESC", connection);


        return await Task.Run(() =>
        {
            var purchasedProducts = new List<PurchasedProduct>();

            try
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int productId = reader.GetInt32(0);
                        string productName = reader.GetString(1);
                        decimal price = reader.GetDecimal(2);
                        int stockQuantity = reader.GetInt32(3);
                        string description = reader.GetString(4);
                        string sex = reader.GetString(5);
                        int customerIdFromDb = reader.GetInt32(6);

                        purchasedProducts.Add(new PurchasedProduct(productId, productName, price, stockQuantity, description, sex, customerIdFromDb));
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                Console.WriteLine($"Error fetching purchased products sorted by time: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }

            return purchasedProducts.AsEnumerable();
        });
    }   

}

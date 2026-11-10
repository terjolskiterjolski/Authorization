using Npgsql;

namespace ProductsEditing{
public class ProductEditor(string connectionString)
{

    public async Task<List<Product>> GetProductsAsync()
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "SELECT product_id, name, price, stock_quantity, description, sex, photo_path FROM public.products ORDER BY created_at_utc DESC, product_id DESC", connection);
        await using var reader = await command.ExecuteReaderAsync();
        var products = new List<Product>();
        while (await reader.ReadAsync())
            products.Add(new Product
            {
                Id = reader.GetInt32(0), Name = reader.GetString(1),
                Price = reader.GetDecimal(2), StockQuantity = reader.GetInt32(3),
                Description = reader.GetString(4), Sex = reader.GetString(5),
                PhotoPath = reader.IsDBNull(6) ? null : reader.GetString(6)
            });
        return products;
    }

    public async Task<bool> UpdateProductAsync(int originalId, Product input)
    {
        if (input.Sex is not ("Men" or "Women" or "Unisex"))
            throw new ArgumentException("Sex must be Men, Women, or Unisex.");
        var product = new Product { Id = originalId, Sex = input.Sex };
        var change = new Change(product);
        change.ChangeId(input.Id);
        change.ChangeName(product, input.Name);
        change.ChangePrice(product, input.Price);
        change.ChangeStockQuantity(product, input.StockQuantity);
        change.ChangeDescription(product, input.Description);

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("""
            UPDATE public.products
            SET product_id = @id, name = @name, price = @price,
                stock_quantity = @stock, description = @description, sex = @sex
            WHERE product_id = @originalId
            """, connection);
        command.Parameters.AddWithValue("id", product.Id);
        command.Parameters.AddWithValue("name", product.Name);
        command.Parameters.AddWithValue("price", product.Price);
        command.Parameters.AddWithValue("stock", product.StockQuantity);
        command.Parameters.AddWithValue("description", product.Description);
        command.Parameters.AddWithValue("sex", product.Sex);
        command.Parameters.AddWithValue("originalId", originalId);
        return await command.ExecuteNonQueryAsync() == 1;
    }
}
}

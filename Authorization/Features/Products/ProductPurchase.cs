using Authorization.Infrastructure.Persistence;
using Npgsql;

namespace ProductPurchase;

public sealed class ProductPurchase(string connectionString)
{
    public async Task<bool> PurchaseProductAsync(int productId, int userId,
        string userName, string email, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using (var schema = new NpgsqlCommand(
            SqlFileLoader.Load("CreatePurchasesTable.sql"), connection, transaction))
            await schema.ExecuteNonQueryAsync(cancellationToken);

        // Updating only positive stock prevents concurrent buyers from overselling.
        await using var command = new NpgsqlCommand("""
            WITH purchased AS (
                UPDATE products SET stock_quantity = stock_quantity - 1
                WHERE product_id = @productId AND stock_quantity > 0
                RETURNING product_id, name, price
            )
            INSERT INTO purchases
                (product_id, user_id, user_name, email, product_name, unit_price)
            SELECT product_id, @userId, @userName, @email, name, price
            FROM purchased
            """, connection, transaction);
        command.Parameters.AddWithValue("productId", productId);
        command.Parameters.AddWithValue("userId", userId);
        command.Parameters.AddWithValue("userName", userName);
        command.Parameters.AddWithValue("email", email);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 0)
            return false;
        await transaction.CommitAsync(cancellationToken);
        return true;
    }
}

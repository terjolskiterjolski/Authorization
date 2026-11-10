using Authorization.Infrastructure.Persistence;
using Npgsql;

namespace Authorization.Features.Favorites;

public sealed class FavoritesService(string connectionString)
{
    public void Add(string userId, string productId)
    {
        using NpgsqlConnection connection = new(connectionString);
        connection.Open();
        using NpgsqlCommand command = new(SqlFileLoader.Load("AddCustomerFavorite.sql"), connection);
        command.Parameters.AddWithValue("userId", userId);
        command.Parameters.AddWithValue("productId", productId);
        command.ExecuteNonQuery();
    }
}

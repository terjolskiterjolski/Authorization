using Authorization.Features.Products;
using Npgsql;

namespace Authorization.Features.FragranceCheck;

public sealed class Finder(ProductSearchService products)
{
    public IReadOnlyList<int> ProductsBySex(string Sex)
    {
        string connectionString = Environment.GetEnvironmentVariable("AUTHORIZATION_DATABASE_CONNECTION") ?? throw new InvalidOperationException("Set AUTHORIZATION_DATABASE_CONNECTION to the database connection string.");
        NpgsqlConnection connection = new NpgsqlConnection(connectionString);
        NpgsqlCommand command = new NpgsqlCommand("SELECT product_id FROM products WHERE sex = @sex ", connection);
        

        return null;
    }
}

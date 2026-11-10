using Authorization.Features.Products;
using Npgsql;

namespace Authorization.Features.FragranceCheck;

public sealed class Finder(ProductSearchService products)
{
    public IReadOnlyList<int> ProductsBySex(string Sex)
    {
        NpgsqlConnection connection = new NpgsqlConnection("Host=localhost;Username=postgres;Password=CHANGE_ME;Database=Authorization");
        NpgsqlCommand command = new NpgsqlCommand("SELECT product_id FROM products WHERE sex = @sex ", connection);
        

        return null;
    }
}

using Authorization.Infrastructure.Persistence;
using Npgsql;
using System.Data.Common;

namespace Authorization.Features.Products;

public sealed class ProductSearchService
{
    private readonly string connectionString;

    public ProductSearchService(string connectionString)
    {
        this.connectionString = connectionString;
        InitializeDatabase();
    }

    static Product ReadProduct(DbDataReader reader) => new()
    {

        Id = reader.GetInt32(0),
        Name = reader.GetString(1),
        Description = reader.GetString(2),
        Price = reader.GetDecimal(3),
        StockQuantity = reader.GetInt32(4),
        CreatedAtUtc = reader.GetDateTime(5),
        Rating = reader.GetInt32(6),
        InquiryFrequency = reader.GetInt32(7),
        Sex = reader.GetString(8),
        PhotoPath = reader.IsDBNull(9) ? null : reader.GetString(9)
    };

    private void InitializeDatabase()
    {
        using NpgsqlConnection connection = new(connectionString);
        connection.Open();
        using NpgsqlCommand command = connection.CreateCommand();

        foreach (string file in new[]
                 {
                     "CreateProductsTable.sql",
                     "InsertProducts.sql",
                     "CreateCustomerFavoritesTable.sql",
                     "CreateCustomerProductRatingsTable.sql"
                 })
        {
            command.CommandText = SqlFileLoader.Load(file);
            command.ExecuteNonQuery();
        }
    }

    public bool SetRating(int productId, int rating)
    {
        if (rating is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(rating));

        using NpgsqlConnection connection = new(connectionString);
        connection.Open();
        using NpgsqlCommand command = new(SqlFileLoader.Load("UpdateProductRating.sql"), connection);
        command.Parameters.AddWithValue("productId", productId);
        command.Parameters.AddWithValue("rating", rating);
        return command.ExecuteNonQuery() > 0;
    }
    public Product[] GetByPopularity(int InquiryFrequency)
    {
        using NpgsqlConnection connection = new(connectionString);
        connection.Open();
        using NpgsqlCommand command = new(SqlFileLoader.Load("GetProductsByPopularity.sql"), connection);
        using NpgsqlDataReader reader = command.ExecuteReader();
        List<Product> products = new();
        while (reader.Read())
        {
            Product product = ReadProduct(reader);

            if (product.InquiryFrequency > Product.Sold)
            {
                products.Add(product);
            }
        }

           return products.ToArray();
    }
    public IReadOnlyList<Product> Search(string prefix)
    {
        using NpgsqlConnection connection = new(connectionString);
        connection.Open();
        using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = SqlFileLoader.Load("FindProductsByPrefix.sql");
        command.Parameters.AddWithValue("prefix", prefix);

        using NpgsqlDataReader reader = command.ExecuteReader();
        List<Product> products = [];

        while (reader.Read())
        {
            products.Add(ReadProduct(reader));
        }

        return products;
    }
}

using System;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Threading.Tasks;
using ProductsEditing;
using UploadingProducts;

namespace ReviewingFeedback;

public record Rating(int ProductId, string ProductName, decimal AverageRating, int CustomerId);
public record Comment(int ProductId, string ProductName, string Feedback, int CustomerId);

public class RatingDashboard
{
    private readonly string connectionString;

    public RatingDashboard(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public async Task<IEnumerable<Rating>> GetRatingsAsync()
    {
        Npgsql.NpgsqlConnectionStringBuilder builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
        Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection(builder.ConnectionString);
        Npgsql.NpgsqlCommand command = new Npgsql.NpgsqlCommand("SELECT r.product_id, p.name, AVG(r.rating), r.customer_id FROM product_reviews r JOIN products p ON p.product_id = r.product_id GROUP BY r.product_id, p.name, r.customer_id", connection);
        return await Task.Run(() =>
        {
            var ratings = new List<Rating>();

            try
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int productId = reader.GetInt32(0);
                        string productName = reader.GetString(1);
                        decimal averageRating = reader.GetDecimal(2);

                        ratings.Add(new Rating(productId, productName, averageRating, reader.GetInt32(3)));
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                Console.WriteLine($"Error fetching ratings: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }

            return ratings.AsEnumerable();
        });

    }
}

public class CommentDashboard
{
    private readonly string connectionString;

    public CommentDashboard(string connectionString)
    {
        this.connectionString = connectionString;
    }
    
    public async Task<IEnumerable<Comment>> GetCommentsAsync()
    {
        Npgsql.NpgsqlConnectionStringBuilder builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
        Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection(builder.ConnectionString);
        Npgsql.NpgsqlCommand command = new Npgsql.NpgsqlCommand("SELECT r.product_id, p.name, r.message, r.customer_id FROM product_reviews r JOIN products p ON p.product_id = r.product_id", connection);
        return await Task.Run(() =>
        {
            var comments = new List<Comment>();

            try
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int productId = reader.GetInt32(0);
                        string productName = reader.GetString(1);
                        string feedback = reader.GetString(2);

                        comments.Add(new Comment(productId, productName, feedback, reader.GetInt32(3)));
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                Console.WriteLine($"Error fetching comments: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }

            return comments.AsEnumerable();
        });
    }

}
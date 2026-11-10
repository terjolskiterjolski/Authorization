using Npgsql;

namespace Feedback
{
    public sealed class ProductReview
    {
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int CustomerId { get; set; }
    public int Rating { get; set; } // 1–5
    public string Message { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
    }

public sealed record CustomerProductRating(
    int CustomerId,
    int ProductId,
    int Rating
);

public sealed class RatingService
{
    private readonly string connectionString;

    public RatingService(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public async Task SaveAsync(
        CustomerProductRating rating,
        CancellationToken cancellationToken = default)
    {
        if (rating.CustomerId <= 0 || rating.ProductId <= 0)
            throw new ArgumentException("Customer and product IDs must be positive.");

        if (rating.Rating is < 1 or > 5)
            throw new ArgumentOutOfRangeException(
                nameof(rating), "Rating must be between 1 and 5.");

        const string sql = """
            INSERT INTO customer_product_ratings
                (customer_id, product_id, rating)
            VALUES
                (@customerId, @productId, @rating)
            ON CONFLICT (customer_id, product_id)
            DO UPDATE SET rating = EXCLUDED.rating;
            """;

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("customerId", rating.CustomerId);
        command.Parameters.AddWithValue("productId", rating.ProductId);
        command.Parameters.AddWithValue("rating", rating.Rating);

        await command.ExecuteNonQueryAsync(cancellationToken);

        await using var updateProduct = new NpgsqlCommand(
            "UPDATE products SET rating = @rating WHERE product_id = @productId", connection, transaction);
        updateProduct.Parameters.AddWithValue("rating", rating.Rating);
        updateProduct.Parameters.AddWithValue("productId", rating.ProductId);
        await updateProduct.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

    }
}
    public sealed record Message(
    int CustomerID, 
    int ProductID,
    string Comment,
    string CustomerEmail,
    int Rating
    );

    public sealed class MessageService
    {
        private readonly string ConnectionString;

        public MessageService(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public async Task SaveAsync(
            Message comment,
             CancellationToken cancellationToken = default)
        {   
            if (string.IsNullOrWhiteSpace(comment.Comment))
                throw new ArgumentException("Comment cannot be empty.", nameof(comment));
            if (comment.ProductID <= 0 ||
                string.IsNullOrWhiteSpace(comment.CustomerEmail) || comment.Rating is < 1 or > 5)
                throw new ArgumentException("A customer, product, email, and rating between 1 and 5 are required.", nameof(comment));
            const string sql = """
                INSERT INTO product_reviews
                    (customer_id, customer_email, product_id, rating, message)
                VALUES
                    (@customerId, @customerEmail, @productId, @rating, @message)
                ON CONFLICT (customer_id, customer_email, product_id)
                DO UPDATE SET rating = EXCLUDED.rating, message = EXCLUDED.message;
                """;
            await using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("customerId", comment.CustomerID);
            command.Parameters.AddWithValue("customerEmail", comment.CustomerEmail);
            command.Parameters.AddWithValue("rating", comment.Rating);
            command.Parameters.AddWithValue("productId", comment.ProductID);
            command.Parameters.AddWithValue("message", comment.Comment);

            await command.ExecuteNonQueryAsync(cancellationToken);

        }
    }
}

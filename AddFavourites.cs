using Npgsql;

namespace Favourites.models
{
    public class AddFavourites
    {
        public string UserId { get; set; }
        public string ProductId { get; set; }

        public AddFavourites(string userId, string productId)
        {
            UserId = userId;
            ProductId = productId;
        }

        public void AddToFavourites(string userId, string productId)
        {
            UserId = userId;
            ProductId = productId;
            NpgsqlConnection connection = new NpgsqlConnection();
            connection.Open();
            NpgsqlCommand command = new NpgsqlCommand(LoadSql("AddToFavourites.sql"), connection);
            command.Parameters.AddWithValue("userId", userId);
            command.Parameters.AddWithValue("productId", productId);
            command.ExecuteNonQuery();

        }

        private static string LoadSql(string fileName)
        {
            string sqlPath = Path.Combine(AppContext.BaseDirectory, "Database", fileName);
            return File.ReadAllText(sqlPath);
        }

        
    }
}
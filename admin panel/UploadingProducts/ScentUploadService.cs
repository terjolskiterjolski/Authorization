namespace Fragrance;

public class ScentUploadService(string ConnectionString, int ProductId, string ScentName)
{
    Npgsql.NpgsqlConnectionStringBuilder builder = new Npgsql.NpgsqlConnectionStringBuilder(ConnectionString);
    Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection(builder.ConnectionString);
    
    
}
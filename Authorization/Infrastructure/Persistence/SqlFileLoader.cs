namespace Authorization.Infrastructure.Persistence;

public static class SqlFileLoader
{
    public static string Load(string fileName)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Database", fileName);
        return File.ReadAllText(path);
    }
}

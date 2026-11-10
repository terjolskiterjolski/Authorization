using Npgsql;
using ProductsEditing;

namespace UploadingProducts;

public class PhotoUploadService(string connectionString, string uploadFolder)
{
    public async Task InitializeAsync()
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("""
            ALTER TABLE public.products ADD COLUMN IF NOT EXISTS photo_path TEXT;
            DO $$
            BEGIN
                IF to_regclass('public.photo_uploaders') IS NOT NULL THEN
                    UPDATE public.products p SET photo_path = latest.photo_path
                    FROM (SELECT DISTINCT ON (product_id) product_id, photo_path
                          FROM public.photo_uploaders ORDER BY product_id, photo_id DESC) latest
                    WHERE p.product_id = latest.product_id AND p.photo_path IS NULL;
                END IF;
            END $$;
            """, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<Product> UploadPhotoAsync(Product product, Stream photo, string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension is not (".jpg" or ".jpeg" or ".png" or ".webp"))
            throw new ArgumentException("Choose a JPG, PNG, or WebP photo.");

        byte[] header = new byte[12];
        int count = await photo.ReadAtLeastAsync(header, 12, throwOnEndOfStream: false);
        bool valid = extension switch
        {
            ".jpg" or ".jpeg" => count >= 3 && header[0] == 255 && header[1] == 216 && header[2] == 255,
            ".png" => count >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            ".webp" => count >= 12 && System.Text.Encoding.ASCII.GetString(header, 0, 4) == "RIFF"
                && System.Text.Encoding.ASCII.GetString(header, 8, 4) == "WEBP",
            _ => false
        };
        if (!valid) throw new ArgumentException("The file is not a supported image.");

        Directory.CreateDirectory(uploadFolder);
        string name = Guid.NewGuid().ToString("N") + extension;
        string fullPath = Path.Combine(uploadFolder, name);
        string savedPath = "/uploads/products/" + name;
        try
        {
            await using (var output = File.Create(fullPath))
            {
                await output.WriteAsync(header.AsMemory(0, count));
                await photo.CopyToAsync(output);
            }
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand("""
                UPDATE public.products SET photo_path = @path WHERE product_id = @id;
                """, connection);
            command.Parameters.AddWithValue("id", product.Id);
            command.Parameters.AddWithValue("path", savedPath);
            if (await command.ExecuteNonQueryAsync() == 0)
                throw new InvalidOperationException("Product no longer exists.");
            product.PhotoPath = savedPath;
            return product;
        }
        catch
        {
            try { File.Delete(fullPath); } catch { }
            throw;
        }
    }

}

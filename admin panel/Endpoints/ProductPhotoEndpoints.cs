using UploadingProducts;
using ProductsEditing;


namespace AdminPanel.Endpoints;

public static class ProductPhotoEndpoints
{
    public static IEndpointRouteBuilder MapProductPhotoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/products/{id:int}/photos", async (int id, HttpRequest request, ProductEditor editor, PhotoUploadService photos, ILogger<ProductPhotoEndpointsLogger> logger) =>
        {
            if (id <= 0 || !request.HasFormContentType)
                return Results.BadRequest(new { message = "Send an existing product ID and a photo as multipart form data." });
            try
            {
                var form = await request.ReadFormAsync();
                var file = form.Files.GetFile("photo");
                if (file == null || file.Length == 0 || file.Length > 5 * 1024 * 1024)
                    return Results.BadRequest(new { message = "Choose a photo smaller than 5 MB in the photo field." });
                var product = (await editor.GetProductsAsync()).Find(product => product.Id == id);
                if (product == null) return Results.NotFound(new { message = "Product not found." });
                await using var stream = file.OpenReadStream();
                var upload = await photos.UploadPhotoAsync(product, stream, file.FileName);
                return Results.Created($"/api/products/{id}/photos", upload);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { message = exception.Message });
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to upload photo for product {ProductId}", id);
                return Results.Json(new { message = "Could not save photo. Check the database connection and permissions." }, statusCode: 503);
            }
        });
        app.MapGet("/api/products/{id:int}/photos", async (int id, ProductEditor editor, ILogger<ProductPhotoEndpointsLogger> logger) =>
        {
            try
            {
                var product = (await editor.GetProductsAsync()).Find(product => product.Id == id);
                return product == null
                    ? Results.NotFound(new { message = "Product not found." })
                    : Results.Ok(new { product.PhotoPath });
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to load photos for product {ProductId}", id);
                return Results.Json(new { message = "Could not load photos." }, statusCode: 503);
            }
        });
        return app;
    }

    private sealed class ProductPhotoEndpointsLogger { }
}

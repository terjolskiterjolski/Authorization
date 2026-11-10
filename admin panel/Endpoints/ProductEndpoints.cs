using UploadingProducts;
using ProductsEditing;
using Npgsql;
using AdminPanel.Models;

namespace AdminPanel.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/products", async (ProductInput input, ProductAdder adder, ILogger<ProductEndpointsLogger> logger) =>
        {
            if (input.ProductId <= 0 || string.IsNullOrWhiteSpace(input.ProductName)
                || input.ProductName.Trim().Length > 100 || input.Price < 0
                || input.Price > 99999999.99m || decimal.Round(input.Price, 2) != input.Price
                || input.StockQuantity < 0 || string.IsNullOrWhiteSpace(input.Description)
                || input.Sex is not ("Men" or "Women" or "Unisex"))
                return Results.BadRequest(new { message = "Enter a positive product ID, a name of up to 100 characters, a description, a valid price with at most two decimal places, nonnegative stock, and sex (Men, Women, or Unisex)." });
            try
            {
                await adder.AddProductAsync(input.ProductId, input.ProductName, input.Price, input.StockQuantity, input.Description, input.Sex);
                return Results.Created($"/api/products/{input.ProductId}", new { message = "Product added successfully." });
            }
            catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return Results.Conflict(new { message = "A product with this ID already exists. Choose another ID." });
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to add product {ProductId}", input.ProductId);
                return Results.Json(new { message = "Could not save the product. Check the database connection and table setup." }, statusCode: 503);
            }
        });
        app.MapGet("/api/products", async (ProductEditor editor, ILogger<ProductEndpointsLogger> logger) =>
        {
            try { return Results.Ok(await editor.GetProductsAsync()); }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to load products");
                return Results.Json(new { message = "Could not load products. Check the database connection and try again." }, statusCode: 503);
            }
        });
        app.MapPut("/api/products/{id:int}", async (int id, Product input, ProductEditor editor, ILogger<ProductEndpointsLogger> logger) =>
        {
            if (id <= 0) return Results.BadRequest(new { message = "Product ID must be positive." });
            try
            {
                return await editor.UpdateProductAsync(id, input)
                    ? Results.Ok(new { message = "Product updated successfully." })
                    : Results.NotFound(new { message = "This product no longer exists. Reload the product list." });
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { message = exception.Message });
            }
            catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return Results.Conflict(new { message = "Another product already uses this ID. Choose a different ID." });
            }
            catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                return Results.Conflict(new { message = "This product ID is referenced by another record and cannot be changed." });
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to update product {ProductId}", id);
                return Results.Json(new { message = "Could not save changes. Check the database connection and try again." }, statusCode: 503);
            }
        });
        return app;
    }

    private sealed class ProductEndpointsLogger { }
}

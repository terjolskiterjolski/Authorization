namespace Authorization.Features.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/products/{productId:int}/purchase", async (
            int productId, ProductPurchase.ProductPurchase purchases, HttpContext context,
            ILogger<ProductPurchase.ProductPurchase> logger) =>
        {
            if (context.User.Identity?.IsAuthenticated != true)
                return Results.Json(new { message = "Please sign in to purchase a product." }, statusCode: 401);
            string? email = context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            string? name = context.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            if (!int.TryParse(context.User.FindFirst("CustomerId")?.Value, out int customerId) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(name))
                return Results.Json(new { message = "Please sign in again with your email and password." }, statusCode: 403);
            if (productId <= 0)
                return Results.BadRequest(new { message = "Invalid product." });
            try
            {
                bool purchased = await purchases.PurchaseProductAsync(productId, customerId,
                    name, email, context.RequestAborted);
                return purchased
                    ? Results.Ok(new { message = "Purchase saved successfully." })
                    : Results.Conflict(new { message = "This product is no longer available." });
            }
            catch (Npgsql.NpgsqlException exception)
            {
                logger.LogError(exception, "Purchase failed for product {ProductId}", productId);
                return Results.Json(new { message = "Could not save your purchase. Please try again." }, statusCode: 500);
            }
        });

        endpoints.MapPost("/api/products/{productId:int}/feedback", async (
            int productId, ProductFeedbackRequest request, Feedback.MessageService messages,
            ProductSearchService products, HttpContext context) =>
        {
            string? email = context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (context.User.Identity?.IsAuthenticated != true)
                return Results.Json(new { message = "Your sign-in session has expired or is missing. Please sign in again." }, statusCode: 401);

            // Existing registered customers can have ID 0. Authentication comes
            // from the signed cookie, not from the numeric value of the ID.
            if (!int.TryParse(context.User.FindFirst("CustomerId")?.Value, out int customerId) ||
                string.IsNullOrWhiteSpace(email))
                return Results.Json(new { message = "Your session is missing your customer details. Please sign in again with your email and password." }, statusCode: 403);

            if (productId <= 0 || string.IsNullOrWhiteSpace(request.Message) ||
                request.Message.Length > 5000 || request.Rating is < 1 or > 5)
                return Results.BadRequest(new { message = "Enter feedback (up to 5,000 characters) and a rating between 1 and 5." });

            if (!products.Search("").Any(product => product.Id == productId))
                return Results.NotFound(new { message = "Product not found." });

            await messages.SaveAsync(new Feedback.Message(customerId, productId,
                request.Message.Trim(), email, request.Rating), context.RequestAborted);
            return Results.Ok(new { message = "Your feedback has been saved." });
        });

        endpoints.MapGet("/api/products", (ProductSearchService service) =>
            Results.Ok(service.Search("").OrderByDescending(product => product.Rating).ThenBy(product => product.Id)));

        endpoints.MapGet("/api/products/search", (string? prefix, ProductSearchService service) =>
            Results.Ok(service.Search(prefix?.Trim() ?? "")));
        return endpoints;
    }
}
public sealed record ProductFeedbackRequest(string? Message, int Rating);

public sealed record ProductAndScent(int Id, String scent);

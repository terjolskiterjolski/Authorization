using Authorization.Contracts;
using Authorization.Features.Products;
using products;

namespace Authorization.Features.Rating;

public static class ScoringEndpoints
{
    public static IEndpointRouteBuilder MapScoringEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/api/products/{productId:int}/rating", async (int productId, ProductRatingRequest request, ProductSearchService service, Feedback.RatingService ratingService, HttpContext context) =>
        {
            if (context.User.Identity?.IsAuthenticated != true ||
                !int.TryParse(context.User.FindFirst("CustomerId")?.Value, out int CustomerId) || CustomerId <= 0)
                return Results.Unauthorized();

            if (productId <= 0 || request.Rating is < 1 or > 5)
                return Results.BadRequest(new ApiResponse("Provide a valid product ID and a rating between 1 and 5."));

            Product? product = service.Search("").FirstOrDefault(product => product.Id == productId);
            if (product is null)
                return Results.NotFound(new ApiResponse("Product not found."));

            Feedback.CustomerProductRating ratedProduct = product.rate(request.Rating, CustomerId);
            await ratingService.SaveAsync(ratedProduct, context.RequestAborted);
            return Results.Ok(new { productId, rating = product.Rating });
        });

        endpoints.MapPost("/api/products/rate", (RateProductRequest request) =>
        {
            if (request.Product is null)
                return Results.BadRequest(new ApiResponse("A product is required."));

            if (request.Rating is < 1 or > 5)
                return Results.BadRequest(new ApiResponse("Rating must be between 1 and 5."));

            Score score = new() { rating = request.Rating };
            score.rate(request.Product);
            return Results.Ok(request.Product);
        });

        endpoints.MapPost("/api/products/order-by-rating", (Product[] products) =>
        {
            if (products.Any(product => product is null))
                return Results.BadRequest(new ApiResponse("Products must not contain null entries."));

            return Results.Ok(new Ordering().sequenceOfProducts(products));
        });

        return endpoints;
    }
}

public sealed record RateProductRequest(Product? Product, int Rating);
public sealed record ProductRatingRequest(int Rating);

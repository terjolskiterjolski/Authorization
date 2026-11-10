using ProductsEditing;
using ReviewingFeedback;
using UploadingProducts;
using SelledPerfumes;

namespace AdminPanel.Configuration;

public static class ProductServiceExtensions
{
    public static IServiceCollection AddProductServices(
        this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("AuthorizationDatabase")
            ?? throw new InvalidOperationException("Database connection string is missing.");

        services.AddSingleton(new ProductAdder(connectionString));
        services.AddSingleton(new ProductEditor(connectionString));
        services.AddSingleton(new PhotoUploadService(connectionString,
            Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", "products")));

        services.AddSingleton(new RatingDashboard(connectionString));
        services.AddSingleton(new CommentDashboard(connectionString));
        services.AddSingleton(new PurchasedProducts(connectionString));

        return services;
    }
}


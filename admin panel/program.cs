using AdminPanel.Configuration;
using AdminPanel.Endpoints;
using UploadingProducts;
using SelledPerfumes;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProductServices(builder.Configuration, builder.Environment);

var app = builder.Build();
await app.Services.GetRequiredService<PhotoUploadService>().InitializeAsync();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapProductEndpoints();
app.MapProductPhotoEndpoints();
app.MapFeedbackEndpoints();

if (app.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("OPEN_ADMIN_BROWSER"))
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(
                "http://127.0.0.1:5000/index.html") { UseShellExecute = true });
        }
        catch (Exception exception)
        {
            app.Logger.LogWarning(exception, "Could not open the browser. Open http://127.0.0.1:5000/index.html manually.");
        }
    });
}

app.MapGet("/api/purchased-products/history", () =>
    Results.Redirect("/purchased-products-history"));

app.MapGet("/purchased-products-history", () =>
    Results.Redirect("/purchased-products-history.html"));

app.MapGet("/api/purchased-products", async (int customerId, PurchasedProducts products) =>
{
    if (customerId <= 0)
        return Results.BadRequest(new { error = "Enter a positive customer ID." });

    return Results.Ok(await products.GetPurchasedProductsAsync(customerId));
});

app.Run();


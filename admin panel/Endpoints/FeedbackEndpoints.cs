using ReviewingFeedback;

namespace AdminPanel.Endpoints;

public static class FeedbackEndpoints
{
    public static IEndpointRouteBuilder MapFeedbackEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/feedback/ratings", async (RatingDashboard dashboard, ILogger<RatingDashboard> logger) =>
        {
            try { return Results.Ok(await dashboard.GetRatingsAsync()); }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to load product ratings");
                return Results.Json(new { message = "Could not load ratings. Please try again." }, statusCode: 503);
            }
        });
        app.MapGet("/api/feedback/comments", async (CommentDashboard dashboard, ILogger<CommentDashboard> logger) =>
        {
            try { return Results.Ok(await dashboard.GetCommentsAsync()); }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to load customer comments");
                return Results.Json(new { message = "Could not load comments. Please try again." }, statusCode: 503);
            }
        });
        return app;
    }
}

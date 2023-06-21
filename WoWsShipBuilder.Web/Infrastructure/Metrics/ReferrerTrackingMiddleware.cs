using WoWsShipBuilder.Infrastructure.Metrics;

namespace WoWsShipBuilder.Web.Infrastructure.Metrics;

public class ReferrerTrackingMiddleware
{
    private const string ReferrerQueryParamName = "ref";

    private readonly RequestDelegate next;

    private readonly MetricsService metricsService;

    public ReferrerTrackingMiddleware(RequestDelegate next, MetricsService metricsService)
    {
        this.next = next;
        this.metricsService = metricsService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Query.TryGetValue(ReferrerQueryParamName, out var refValue) && !string.IsNullOrWhiteSpace(refValue))
        {
            metricsService.RefCount.WithLabels(refValue!, context.Request.Path).Inc();
        }

        await next(context);
    }
}

public static class ReferrerTrackingMiddleWareExtensions
{
    public static IApplicationBuilder UseReferrerTracking(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ReferrerTrackingMiddleware>();
    }
}

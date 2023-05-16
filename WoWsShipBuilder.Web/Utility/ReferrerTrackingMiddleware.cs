using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.Web.Services;

namespace WoWsShipBuilder.Web.Utility;

public class ReferrerTrackingMiddleware
{
    private const string ReferrerQueryParamName = "ref";

    private readonly RequestDelegate next;

    private readonly IMetricsService metricsService;

    public ReferrerTrackingMiddleware(RequestDelegate next, IMetricsService metricsService)
    {
        this.next = next;
        this.metricsService = metricsService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.Query.TryGetValue(ReferrerQueryParamName, out var refValue);
        if (!string.IsNullOrWhiteSpace(refValue))
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

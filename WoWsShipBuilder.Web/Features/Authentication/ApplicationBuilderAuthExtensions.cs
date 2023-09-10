namespace WoWsShipBuilder.Web.Features.Authentication;

public static class ApplicationBuilderAuthExtensions
{
    public static IApplicationBuilder UseShipBuilderAuth(this IApplicationBuilder app)
    {
        var cookiePolicy = new CookiePolicyOptions
        {
            MinimumSameSitePolicy = SameSiteMode.Strict,
        };

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCookiePolicy(cookiePolicy);

        return app;
    }
}

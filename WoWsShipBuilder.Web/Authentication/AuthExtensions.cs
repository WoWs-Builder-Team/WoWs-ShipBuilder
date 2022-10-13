namespace WoWsShipBuilder.Web.Authentication;

public static class AuthExtensions
{
    public static void ConfigureCookiePolicy(this IApplicationBuilder app)
    {
        var cookiePolicy = new CookiePolicyOptions
        {
            MinimumSameSitePolicy = SameSiteMode.Strict,
        };
        app.UseCookiePolicy(cookiePolicy);
    }
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WoWsShipBuilder.Web.Infrastructure;

namespace WoWsShipBuilder.Web.Features.Authentication;

[Route("/api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AdminOptions options;

    private readonly ILogger<AuthController> logger;

    private readonly AuthenticationService authenticationService;

    public AuthController(IOptions<AdminOptions> options, ILogger<AuthController> logger, AuthenticationService authenticationService)
    {
        this.options = options.Value;
        this.logger = logger;
        this.authenticationService = authenticationService;
    }

    [HttpGet]
    public async Task<ActionResult> Authenticate([FromQuery(Name = "status")] string status)
    {
        if (status == "ok")
        {
            return await AuthenticationConfirmed(HttpContext.Request.Query["access_token"]!, HttpContext.Request.Query["account_id"]!, HttpContext.Request.Query["nickname"]!);
        }

        return await AuthenticationCanceled(status, HttpContext.Request.Query["message"]!, int.Parse(HttpContext.Request.Query["code"]!));
    }

    public async Task<ActionResult> AuthenticationConfirmed(string accessToken, string accountId, string nickname)
    {
        var authValid = await authenticationService.VerifyToken(accountId, accessToken);
        if (!authValid)
        {
            return Redirect("/auth-failed");
        }

        var principal = authenticationService.CreatePrincipalForUser(accessToken, accountId, nickname);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new() { IsPersistent = true });
        return Redirect("/");
    }

    public Task<ActionResult> AuthenticationCanceled(string status, string message, int code)
    {
        logger.LogDebug("Authentication canceled. Status: {Status}, Message: {Message}, Code: {Code}", status, message, code);
        return Task.FromResult<ActionResult>(Redirect("/"));
    }

    [HttpGet("login/{server}")]
    public Task<ActionResult> Login(string server)
    {
        string baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var pageUrl = $"{baseUrl}/api/auth";
        string url = @$"https://api.worldoftanks.{server}/wot/auth/login/?application_id={options.WgApiKey}&redirect_uri={pageUrl}";
        return Task.FromResult<ActionResult>(Redirect(url));
    }

    [HttpGet("logout")]
    public async Task<ActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
}

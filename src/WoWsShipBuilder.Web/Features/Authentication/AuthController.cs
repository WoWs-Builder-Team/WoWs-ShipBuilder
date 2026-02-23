using System.Globalization;
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
            return await this.AuthenticationConfirmed(this.HttpContext.Request.Query["access_token"]!, this.HttpContext.Request.Query["account_id"]!, this.HttpContext.Request.Query["nickname"]!);
        }

        return await this.AuthenticationCanceled(status, this.HttpContext.Request.Query["message"]!, int.Parse(this.HttpContext.Request.Query["code"]!, CultureInfo.InvariantCulture));
    }

    public async Task<ActionResult> AuthenticationConfirmed(string accessToken, string accountId, string nickname)
    {
        var authValid = await this.authenticationService.VerifyToken(accountId, accessToken);
        if (!authValid)
        {
            return this.Redirect("/auth-failed");
        }

        var principal = this.authenticationService.CreatePrincipalForUser(accessToken, accountId, nickname);
        await this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new() { IsPersistent = true });
        return this.Redirect("/");
    }

    public Task<ActionResult> AuthenticationCanceled(string status, string message, int code)
    {
        this.logger.LogDebug("Authentication canceled. Status: {Status}, Message: {Message}, Code: {Code}", status, message, code);
        return Task.FromResult<ActionResult>(this.Redirect("/"));
    }

    [HttpGet("login/{server:regex(^eu|asia|com$):required}")]
    public Task<ActionResult> Login(string server)
    {
        string baseUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
        var pageUrl = $"{baseUrl}/api/auth";
        string url = @$"https://api.worldoftanks.{server}/wot/auth/login/?application_id={this.options.WgApiKey}&redirect_uri={pageUrl}";
        return Task.FromResult<ActionResult>(this.Redirect(url));
    }

    [HttpGet("logout")]
    public async Task<ActionResult> Logout()
    {
        await this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return this.Redirect("/");
    }
}

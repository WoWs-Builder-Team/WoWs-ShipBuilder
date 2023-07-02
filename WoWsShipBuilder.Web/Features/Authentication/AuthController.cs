using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Web.Infrastructure;

namespace WoWsShipBuilder.Web.Features.Authentication;

[Route("/api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly HttpClient client;

    private readonly AdminOptions options;

    private readonly ILogger<AuthController> logger;

    public AuthController(HttpClient client, IOptions<AdminOptions> options, ILogger<AuthController> logger)
    {
        this.client = client;
        this.options = options.Value;
        this.logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> Authenticate([FromQuery(Name = "status")] string status)
    {
        if (status == "ok")
        {
            return await AuthenticationConfirmed(HttpContext.Request.Query["access_token"]!, HttpContext.Request.Query["account_id"]!, HttpContext.Request.Query["nickname"]!);
        }
        else
        {
            return await AuthenticationCanceled(status, HttpContext.Request.Query["message"]!, int.Parse(HttpContext.Request.Query["code"]!));
        }
    }

    public async Task<ActionResult> AuthenticationConfirmed(string accessToken, string accountId, string nickname)
    {
        var authValid = await VerifyToken(accountId, accessToken);
        if (!authValid)
        {
            return Redirect("/auth-failed");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, accountId),
            new(ClaimTypes.Name, nickname),
            new(ClaimTypes.UserData, accessToken),
        };

        if (options.AdminUsers.Contains(accountId))
        {
            claims.Add(new(ClaimTypes.Role, AppConstants.AdminRoleName));
        }

        if (options.BuildCurators.Contains(accountId))
        {
            claims.Add(new(ClaimTypes.Role, AppConstants.BuildCuratorRoleName));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
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

    internal async Task<bool> VerifyToken(string accountId, string accessToken)
    {
        long numericId = long.Parse(accountId);
        string server = numericId switch
        {
            > 500_000_000 and < 1_000_000_000 => "eu",
            > 1_000_000_000 and < 2_000_000_000 => "com",
            > 2_000_000_000 => "asia",
            _ => throw new InvalidOperationException("unsupported account id range"),
        };

        logger.LogInformation("Verifying access token for account {}", accountId);
        var checkUrl = @$"https://api.worldofwarships.{server}/wows/account/info/?application_id={options.WgApiKey}&account_id={accountId}&access_token={accessToken}&fields=private";
        var request = new HttpRequestMessage(HttpMethod.Get,checkUrl);
        var response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadFromJsonAsync<WgResponse>();
            if (responseData is not null && responseData.Status.Equals("ok"))
            {
                Dictionary<string, object>? privateData = responseData.Data.FirstOrDefault().Value?.Private;
                logger.LogInformation("Token-verification for account {} successful", accountId);
                return privateData is not null && privateData.Any();
            }
        }

        logger.LogInformation("Token-verification for account {} failed", accountId);
        return false;
    }
}

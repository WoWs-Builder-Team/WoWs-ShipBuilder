using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WoWsShipBuilder.Web.Data;
using WoWsShipBuilder.Web.Services;

namespace WoWsShipBuilder.Web.Authentication;

[Route("/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly HttpClient client;

    private readonly AdminOptions options;

    private readonly IMetricsService metricsService;

    private readonly ILogger<AuthController> logger;

    public AuthController(HttpClient client, IOptions<AdminOptions> options, IMetricsService metricsService, ILogger<AuthController> logger)
    {
        this.client = client;
        this.options = options.Value;
        this.metricsService = metricsService;
        this.logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> Authenticate([FromQuery(Name = "access_token")] string accessToken, [FromQuery(Name = "account_id")] string accountId, [FromQuery(Name = "nickname")] string nickname)
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
            claims.Add(new(ClaimTypes.Role, AuthConstants.AdminRoleName));
        }

        if (options.BuildCurators.Contains(accountId))
        {
            claims.Add(new(ClaimTypes.Role, AuthConstants.BuildCuratorRoleName));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new() { IsPersistent = true });
        return Redirect("/");
    }

    [HttpGet("login/{server}")]
    public Task<ActionResult> Login(string server)
    {
        string baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var pageUrl = $"{baseUrl}/auth";
        string url = @$"https://api.worldoftanks.{server}/wot/auth/login/?application_id={options.WgApiKey}&redirect_uri={pageUrl}";
        metricsService.Logins.Inc();
        return Task.FromResult<ActionResult>(Redirect(url));
    }

    [HttpGet("logout")]
    public async Task<ActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }

    private async Task<bool> VerifyToken(string accountId, string accessToken)
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

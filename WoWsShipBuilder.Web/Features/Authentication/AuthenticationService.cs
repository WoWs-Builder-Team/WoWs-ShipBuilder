﻿using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Web.Infrastructure;

namespace WoWsShipBuilder.Web.Features.Authentication;

public class AuthenticationService
{
    private readonly ILogger<AuthenticationService> logger;
    private readonly AdminOptions options;
    private readonly HttpClient client;

    public AuthenticationService(ILogger<AuthenticationService> logger, IOptions<AdminOptions> options, HttpClient client)
    {
        this.logger = logger;
        this.client = client;
        this.options = options.Value;
    }

    public async Task<bool> VerifyToken(string accountId, string accessToken)
    {
        long numericId = long.Parse(accountId, CultureInfo.InvariantCulture);
        string server = numericId switch
        {
            > 500_000_000 and < 1_000_000_000 => "eu",
            > 1_000_000_000 and < 2_000_000_000 => "com",
            > 2_000_000_000 => "asia",
            _ => throw new InvalidOperationException("unsupported account id range"),
        };

        this.logger.LogInformation("Verifying access token for account {}", accountId);
        var checkUrl = @$"https://api.worldofwarships.{server}/wows/account/info/?application_id={this.options.WgApiKey}&account_id={accountId}&access_token={accessToken}&fields=private";
        var request = new HttpRequestMessage(HttpMethod.Get, checkUrl);
        var response = await this.client.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadFromJsonAsync<WgResponse>();
            if (responseData is not null && responseData.Status.Equals("ok", StringComparison.Ordinal))
            {
                var privateData = responseData.Data.FirstOrDefault().Value?.Private;
                this.logger.LogInformation("Token-verification for account {} successful", accountId);
                return privateData is not null && privateData.Count != 0;
            }
        }

        this.logger.LogInformation("Token-verification for account {} failed", accountId);
        return false;
    }

    public ClaimsPrincipal CreatePrincipalForUser(string accessToken, string accountId, string nickname)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, accountId),
            new(ClaimTypes.Name, nickname),
            new(ClaimTypes.UserData, accessToken),
        };

        if (this.options.AdminUsers.Contains(accountId))
        {
            claims.Add(new(ClaimTypes.Role, AppConstants.AdminRoleName));
        }

        if (this.options.BuildCurators.Contains(accountId))
        {
            claims.Add(new(ClaimTypes.Role, AppConstants.BuildCuratorRoleName));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new(identity);
    }
}

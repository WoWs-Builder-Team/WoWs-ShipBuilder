using System.Net;
using Microsoft.AspNetCore.WebUtilities;
using WoWsShipBuilder.Infrastructure.Localization;
using WoWsShipBuilder.Infrastructure.Localization.Resources;

namespace WoWsShipBuilder.Features.Builds;

public static class BuildValidation
{
    public static async Task<BuildStringValidationResult> ValidateBuildString(string buildStr, string selectedShipIndex, ILocalizer localizer, string shortUrlUriPrefix)
    {
        var validatedBuildString = string.Empty;
        if (string.IsNullOrWhiteSpace(buildStr))
        {
            return new(null, validatedBuildString);
        }

        if (buildStr.Contains(shortUrlUriPrefix.Last().Equals('/') ? shortUrlUriPrefix : shortUrlUriPrefix + '/'))
        {
            string? longUrl = await RetrieveLongUrlFromShortLink(buildStr);

            if (longUrl is not null && QueryHelpers.ParseQuery(longUrl).TryGetValue("build", out var buildStrFromUrl))
            {
                buildStr = buildStrFromUrl.ToString();
            }
            else
            {
                return new(localizer.SimpleAppLocalization(nameof(Translation.Validation_InvalidBuild)), validatedBuildString);
            }
        }

        if (QueryHelpers.ParseQuery(buildStr).TryGetValue("build", out var buildStringFromUrl))
        {
            buildStr = buildStringFromUrl.ToString();
        }

        // this is needed to avoid executing the try/catch even when we already know it is going to fail and so get a better performance
        if (buildStr.Contains(';'))
        {
            try
            {
                var build = Build.CreateBuildFromString(buildStr);
                if (selectedShipIndex.Equals(build.ShipIndex, StringComparison.Ordinal))
                {
                    validatedBuildString = buildStr;
                    return new(null, validatedBuildString);
                }

                return new($"{localizer.SimpleAppLocalization(nameof(Translation.Validation_Incompatibility))}. {localizer.SimpleAppLocalization(nameof(Translation.Validation_SelectedShip))}: {localizer.GetGameLocalization(selectedShipIndex + "_FULL").Localization} ≠ {localizer.SimpleAppLocalization(nameof(Translation.Validation_ShipInBuild))}: {localizer.GetGameLocalization(build.ShipIndex + "_FULL").Localization}", validatedBuildString);
            }
            catch (FormatException)
            {
                return new(localizer.SimpleAppLocalization(nameof(Translation.Validation_InvalidBuild)), validatedBuildString);
            }
        }

        return new(localizer.SimpleAppLocalization(nameof(Translation.Validation_InvalidBuild)), validatedBuildString);
    }

    public static string? ValidateBuildName(string buildName)
    {
        List<char> invalidChars = Path.GetInvalidFileNameChars().ToList();
        invalidChars.Add(';');
        List<char> invalidCharsInBuildName = invalidChars.FindAll(buildName.Contains);
        return invalidCharsInBuildName.Any() ? $"Invalid characters {string.Join(' ', invalidCharsInBuildName)}" : null;
    }

    public static async Task<string?> RetrieveLongUrlFromShortLink(string shortUrl)
    {
        // this allows you to set the settings so that we can get the redirect url
        using HttpClient client = new(new HttpClientHandler
        {
            AllowAutoRedirect = false,
        });
        using var response = await client.GetAsync(shortUrl);
        using var content = response.Content;

        // Read the response to see if we have the redirected url
        string? redirectedUrl = null;
        if (response.StatusCode == HttpStatusCode.Found)
        {
            var headers = response.Headers;
            if (headers.Location is not null)
            {
                redirectedUrl = headers.Location.AbsoluteUri;
            }
        }

        return redirectedUrl;
    }

    public sealed record BuildStringValidationResult(string? ValidationMessage, string ValidatedBuildString);
}

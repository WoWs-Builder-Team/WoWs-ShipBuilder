using Microsoft.AspNetCore.WebUtilities;
using WoWsShipBuilder.Common.Builds;
using WoWsShipBuilder.Common.Infrastructure.Localization;

namespace WoWsShipBuilder.Common.Infrastructure;

public static class Validation
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
            string? longUrl = await Helpers.RetrieveLongUrlFromShortLink(buildStr);

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
                if (selectedShipIndex.Equals(build.ShipIndex))
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

    public sealed record BuildStringValidationResult(string? ValidationMessage, string ValidatedBuildString);
}

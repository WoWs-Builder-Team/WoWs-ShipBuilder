using Microsoft.AspNetCore.WebUtilities;
using WoWsShipBuilder.Core.Builds;
using WoWsShipBuilder.Core.Localization;

namespace WoWsShipBuilder.Web.Utility;

public static class Validation
{
    public static async Task<BuildStringValidationResult> ValidateBuildString(string buildStr, string selectedShipIndex, ILocalizer localizer, string shortUrlUriPrefix)
    {
        if (string.IsNullOrWhiteSpace(buildStr))
        {
            return new(false, null);
        }

        if (buildStr.Contains(shortUrlUriPrefix))
        {
            string? longUrl = await Helpers.RetrieveLongUrl(buildStr);

            if (longUrl is not null && QueryHelpers.ParseQuery(longUrl).TryGetValue("build", out var buildStrFromUrl))
            {
                buildStr = buildStrFromUrl.ToString();
            }
            else
            {
                return new(false, localizer.SimpleAppLocalization(nameof(Translation.Validation_InvalidBuild)));
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
                    return new(true, buildStr);
                }

                return new(false, $"{localizer.SimpleAppLocalization(nameof(Translation.Validation_Incompatibility))}. {localizer.SimpleAppLocalization(nameof(Translation.Validation_SelectedShip))}: {localizer.GetGameLocalization(selectedShipIndex + "_FULL").Localization} ≠ {localizer.SimpleAppLocalization(nameof(Translation.Validation_ShipInBuild))}: {localizer.GetGameLocalization(build.ShipIndex + "_FULL").Localization}");
            }
            catch (FormatException)
            {
                return new(false, localizer.SimpleAppLocalization(nameof(Translation.Validation_InvalidBuild)));
            }
        }

        return new(false, localizer.SimpleAppLocalization(nameof(Translation.Validation_InvalidBuild)));
    }

    public sealed record BuildStringValidationResult(bool IsValidBuildString, string? ValidationMessage);
}

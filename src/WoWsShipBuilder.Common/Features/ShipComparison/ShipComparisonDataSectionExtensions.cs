using WoWsShipBuilder.Infrastructure.Localization;
using WoWsShipBuilder.Infrastructure.Localization.Resources;

namespace WoWsShipBuilder.Features.ShipComparison;

public static class ShipComparisonDataSectionExtensions
{
    public static string AsString(this ShipComparisonDataSections dataSection)
    {
        return dataSection switch
        {
            ShipComparisonDataSections.General => "General",
            ShipComparisonDataSections.MainBattery => "MainBattery",
            ShipComparisonDataSections.He => "He",
            ShipComparisonDataSections.Ap => "Ap",
            ShipComparisonDataSections.Sap => "Sap",
            ShipComparisonDataSections.Torpedo => "Torpedo",
            ShipComparisonDataSections.SecondaryBattery => "SecondaryBattery",
            ShipComparisonDataSections.SecondaryBatteryShells => "SecondaryBatteryShells",
            ShipComparisonDataSections.AntiAir => "AntiAir",
            ShipComparisonDataSections.Asw => "Asw",
            ShipComparisonDataSections.AirStrike => "AirStrike",
            ShipComparisonDataSections.Maneuverability => "Maneuverability",
            ShipComparisonDataSections.Concealment => "Concealment",
            ShipComparisonDataSections.Survivability => "Survivability",
            ShipComparisonDataSections.RocketPlanes => "RocketPlanes",
            ShipComparisonDataSections.Rockets => "Rockets",
            ShipComparisonDataSections.TorpedoBombers => "TorpedoBombers",
            ShipComparisonDataSections.AerialTorpedoes => "AerialTorpedoes",
            ShipComparisonDataSections.Bombers => "Bombers",
            ShipComparisonDataSections.Bombs => "Bombs",
            ShipComparisonDataSections.Sonar => "Sonar",
            _ => dataSection.ToString(),
        };
    }

    public static string Localize(this ShipComparisonDataSections dataSection, ILocalizer localizer)
    {
        return dataSection switch
        {
            ShipComparisonDataSections.General => localizer.SimpleAppLocalization(nameof(Translation.ShipComparison_General)),
            ShipComparisonDataSections.MainBattery => localizer.SimpleAppLocalization(nameof(Translation.ShipStats_MainBattery)),
            ShipComparisonDataSections.He => localizer.SimpleAppLocalization(nameof(Translation.ArmamentType_HE)),
            ShipComparisonDataSections.Ap => localizer.SimpleAppLocalization(nameof(Translation.ArmamentType_AP)),
            ShipComparisonDataSections.Sap => localizer.SimpleAppLocalization(nameof(Translation.ArmamentType_SAP)),
            ShipComparisonDataSections.Torpedo => localizer.SimpleAppLocalization(nameof(Translation.Torpedo)),
            ShipComparisonDataSections.SecondaryBattery => localizer.SimpleAppLocalization(nameof(Translation.ShipStats_SecondaryBattery)),
            ShipComparisonDataSections.SecondaryBatteryShells => localizer.SimpleAppLocalization(nameof(Translation.ShipComparison_SecondaryBatteryShells)),
            ShipComparisonDataSections.AntiAir => localizer.SimpleAppLocalization(nameof(Translation.ShipStats_AADefense)),
            ShipComparisonDataSections.Asw => localizer.SimpleAppLocalization(nameof(Translation.ShipComparison_Asw)),
            ShipComparisonDataSections.AirStrike => localizer.SimpleAppLocalization(nameof(Translation.ShipStats_Airstrike)),
            ShipComparisonDataSections.Maneuverability => localizer.SimpleAppLocalization(nameof(Translation.ShipStats_Maneuverability)),
            ShipComparisonDataSections.Concealment => localizer.SimpleAppLocalization(nameof(Translation.ShipStats_Concealment)),
            ShipComparisonDataSections.Survivability => localizer.SimpleAppLocalization(nameof(Translation.ShipStats_Survivability)),
            ShipComparisonDataSections.RocketPlanes => localizer.SimpleAppLocalization(nameof(Translation.ShipComparison_RocketPlanes)),
            ShipComparisonDataSections.Rockets => localizer.SimpleAppLocalization(nameof(Translation.ShipStats_Fighter)),
            ShipComparisonDataSections.TorpedoBombers => localizer.SimpleAppLocalization(nameof(Translation.ShipStats_TorpedoBomber)),
            ShipComparisonDataSections.AerialTorpedoes => localizer.SimpleAppLocalization(nameof(Translation.ShipComparison_AerialTorpedoes)),
            ShipComparisonDataSections.Bombers => localizer.SimpleAppLocalization(nameof(Translation.ShipComparison_Bombers)),
            ShipComparisonDataSections.Bombs => localizer.SimpleAppLocalization(nameof(Translation.ShipComparison_Bombs)),
            ShipComparisonDataSections.Sonar => localizer.SimpleAppLocalization(nameof(Translation.ShipStats_PingerGun)),
            _ => AsString(dataSection),
        };
    }
}

using System.Text.Json.Serialization;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.DataTransfer;
using ServerType = WoWsShipBuilder.Infrastructure.GameData.ServerType;

namespace WoWsShipBuilder.Features.Settings;

public class AppSettings
{
    [JsonConstructor]
    public AppSettings(CultureDetails? selectedLanguage = null)
    {
        this.SelectedLanguage = selectedLanguage ?? AppConstants.DefaultCultureDetails;
    }

    public bool AutoUpdateEnabled { get; set; } = true;

    public CultureDetails SelectedLanguage { get; set; }

    public ServerType SelectedServerType { get; set; } = ServerType.Live;

    public DateTime? LastDataUpdateCheck { get; set; }

    public string? CustomDataPath { get; set; }

    public bool SendTelemetryData { get; set; }

    public bool OpenExplorerAfterImageSave { get; set; }

    public string? LastImageImportPath { get; set; }

    public bool IncludeSignalsForImageExport { get; set; }

    public string? CustomImagePath { get; set; }

    public bool StoreBuildOnShare { get; set; } = true;

    public DispersionPlotSettings DispersionPlotSettings { get; set; } = new();

    public bool OpenAllMainExpandersByDefault { get; set; } = true;

    public bool OpenAllAmmoExpandersByDefault { get; set; }

    public bool OpenSecondariesAndAaExpandersByDefault { get; set; }

    public List<string> BetaAccessCodes { get; set; } = new();

    public bool[] BuildImageLayoutSettings { get; set; } = { true, false, true, true, true, true };

    public bool ShipComparisonUseUpgradedModules { get; set; } = true;

    public bool ShipComparisonHideShipsWithoutSection { get; set; }

    public double ShipComparisonMainBatteryFiringRange { get; set; } = 10;

    public double ShipComparisonSecondaryBatteryFiringRange { get; set; } = 5;

    public string? ShipComparisonHiddenColumns { get; set; }

    public void ClearSettings()
    {
        this.AutoUpdateEnabled = true;
        this.SelectedLanguage = AppConstants.DefaultCultureDetails;
        this.SelectedServerType = ServerType.Live;
        this.LastDataUpdateCheck = default;
        this.CustomDataPath = default;
        this.SendTelemetryData = default;
        this.OpenExplorerAfterImageSave = default;
        this.LastImageImportPath = default;
        this.IncludeSignalsForImageExport = default;
        this.CustomImagePath = default;
        this.DispersionPlotSettings = new();
        this.OpenAllMainExpandersByDefault = true;
        this.OpenAllAmmoExpandersByDefault = false;
        this.OpenSecondariesAndAaExpandersByDefault = false;
        this.BetaAccessCodes = new();
        this.BuildImageLayoutSettings = new[] { true, false, true, true, true, true };
        this.ShipComparisonMainBatteryFiringRange = 10;
        this.ShipComparisonSecondaryBatteryFiringRange = 5;
        this.ShipComparisonUseUpgradedModules = true;
        this.ShipComparisonHideShipsWithoutSection = false;
        this.ShipComparisonHiddenColumns = default;
    }

    public void UpdateFromSettings(AppSettings settings)
    {
        this.AutoUpdateEnabled = settings.AutoUpdateEnabled;
        this.SelectedLanguage = settings.SelectedLanguage;
        this.SelectedServerType = settings.SelectedServerType;
        this.LastDataUpdateCheck = settings.LastDataUpdateCheck;
        this.CustomDataPath = settings.CustomDataPath;
        this.SendTelemetryData = settings.SendTelemetryData;
        this.OpenExplorerAfterImageSave = settings.OpenExplorerAfterImageSave;
        this.LastImageImportPath = settings.LastImageImportPath;
        this.IncludeSignalsForImageExport = settings.IncludeSignalsForImageExport;
        this.CustomImagePath = settings.CustomImagePath;
        this.DispersionPlotSettings = settings.DispersionPlotSettings;
        this.OpenAllMainExpandersByDefault = settings.OpenAllMainExpandersByDefault;
        this.OpenAllAmmoExpandersByDefault = settings.OpenAllAmmoExpandersByDefault;
        this.OpenSecondariesAndAaExpandersByDefault = settings.OpenSecondariesAndAaExpandersByDefault;
        this.BetaAccessCodes = settings.BetaAccessCodes;
        this.BuildImageLayoutSettings = settings.BuildImageLayoutSettings;
        this.ShipComparisonMainBatteryFiringRange = settings.ShipComparisonMainBatteryFiringRange;
        this.ShipComparisonSecondaryBatteryFiringRange = settings.ShipComparisonSecondaryBatteryFiringRange;
        this.ShipComparisonUseUpgradedModules = settings.ShipComparisonUseUpgradedModules;
        this.ShipComparisonHideShipsWithoutSection = settings.ShipComparisonHideShipsWithoutSection;
        this.ShipComparisonHiddenColumns = settings.ShipComparisonHiddenColumns;
    }
}

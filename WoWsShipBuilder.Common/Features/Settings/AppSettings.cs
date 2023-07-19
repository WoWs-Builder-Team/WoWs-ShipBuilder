using Newtonsoft.Json;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.DataTransfer;
using ServerType = WoWsShipBuilder.Infrastructure.GameData.ServerType;

namespace WoWsShipBuilder.Features.Settings;

public class AppSettings
{
    [JsonConstructor]
    public AppSettings(CultureDetails? selectedLanguage = null)
    {
        SelectedLanguage = selectedLanguage ?? AppConstants.DefaultCultureDetails;
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

    public bool OpenAllAmmoExpandersByDefault { get; set; } = true;

    public bool OpenSecondariesAndAaExpandersByDefault { get; set; } = true;

    public List<string> BetaAccessCodes { get; set; } = new();

    public bool[] BuildImageLayoutSettings { get; set; } = { true, false, true, true, true, true };

    public void ClearSettings()
    {
        AutoUpdateEnabled = true;
        SelectedLanguage = AppConstants.DefaultCultureDetails;
        SelectedServerType = ServerType.Live;
        LastDataUpdateCheck = default;
        CustomDataPath = default;
        SendTelemetryData = default;
        OpenExplorerAfterImageSave = default;
        LastImageImportPath = default;
        IncludeSignalsForImageExport = default;
        CustomImagePath = default;
        DispersionPlotSettings = new();
        OpenAllMainExpandersByDefault = true;
        OpenAllAmmoExpandersByDefault = false;
        OpenSecondariesAndAaExpandersByDefault = false;
        BetaAccessCodes = new();
        BuildImageLayoutSettings = new[] { true, false, true, true, true, true };
    }

    public void UpdateFromSettings(AppSettings settings)
    {
        AutoUpdateEnabled = settings.AutoUpdateEnabled;
        SelectedLanguage = settings.SelectedLanguage;
        SelectedServerType = settings.SelectedServerType;
        LastDataUpdateCheck = settings.LastDataUpdateCheck;
        CustomDataPath = settings.CustomDataPath;
        SendTelemetryData = settings.SendTelemetryData;
        OpenExplorerAfterImageSave = settings.OpenExplorerAfterImageSave;
        LastImageImportPath = settings.LastImageImportPath;
        IncludeSignalsForImageExport = settings.IncludeSignalsForImageExport;
        CustomImagePath = settings.CustomImagePath;
        DispersionPlotSettings = settings.DispersionPlotSettings;
        OpenAllMainExpandersByDefault = settings.OpenAllMainExpandersByDefault;
        OpenAllAmmoExpandersByDefault = settings.OpenAllAmmoExpandersByDefault;
        OpenSecondariesAndAaExpandersByDefault = settings.OpenSecondariesAndAaExpandersByDefault;
        BetaAccessCodes = settings.BetaAccessCodes;
        BuildImageLayoutSettings = settings.BuildImageLayoutSettings;
    }
}

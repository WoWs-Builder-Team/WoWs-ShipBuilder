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

    public WebAppSettings? WebAppSettings { get; set; }

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
        WebAppSettings = null;
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
        WebAppSettings = settings.WebAppSettings;
    }
}

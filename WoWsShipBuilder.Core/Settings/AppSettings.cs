using System;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;

namespace WoWsShipBuilder.Core.Settings
{
    public class AppSettings
    {
        [JsonConstructor]
        public AppSettings(CultureDetails? selectedLanguage = null)
        {
            SelectedLanguage = selectedLanguage ?? AppDataHelper.Instance.DefaultCultureDetails;
        }

        public bool AutoUpdateEnabled { get; set; } = true;

        public CultureDetails SelectedLanguage { get; set; }

        public ServerType SelectedServerType { get; set; } = ServerType.Live;

        public DateTime? LastDataUpdateCheck { get; set; }

        public string? CustomDataPath { get; set; }

        public bool SendTelemetryData { get; set; }

        public bool OpenExplorerAfterImageSave { get; set; } = true;

        public string? LastImageImportPath { get; set; }

        public bool IncludeSignalsForImageExport { get; set; }
    }
}

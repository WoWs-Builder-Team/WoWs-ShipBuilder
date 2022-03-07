using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.Core.Translations;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.ViewModels.Other
{
    public abstract class SettingsWindowViewModelBase : ViewModelBase
    {
        protected readonly IAppDataService AppDataService;

        private readonly IClipboardService clipboardService;

        private readonly IFileSystem fileSystem;

        private bool autoUpdate;

        private string? customBuildImagePath;

        private string? customPath;

        private string dataVersion = default!;

        private bool isCustomBuildImagePathEnabled;

        private bool isCustomPathEnabled;

        private List<CultureDetails> languagesList;

        private bool openExplorerAfterImageSave;

        private CultureDetails selectedLanguage = null!;

        private string selectedServer = null!;

        private List<string> servers = null!;

        private bool telemetryDataEnabled;

        public SettingsWindowViewModelBase(IFileSystem fileSystem, IClipboardService clipboardService, IAppDataService appDataService)
        {
            Logging.Logger.Info("Creating setting window view model");
            this.fileSystem = fileSystem;
            this.clipboardService = clipboardService;
            this.AppDataService = appDataService;
            Version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "Undefined";
            languagesList = AppConstants.SupportedLanguages.ToList(); // Copy existing list. Do not change!
            SelectedLanguage = languagesList.FirstOrDefault(languageDetails => languageDetails.CultureInfo.Equals(AppData.Settings.SelectedLanguage.CultureInfo))
                               ?? AppConstants.DefaultCultureDetails;
#if DEBUG
            Servers = Enum.GetNames<ServerType>().ToList();
#else
            Servers = new List<ServerType> { ServerType.Live, ServerType.Pts }.Select(serverType => Enum.GetName(serverType) ?? serverType.StringName()).ToList();
#endif
            SelectedServer = Enum.GetName(typeof(ServerType), AppData.Settings.SelectedServerType)!;
            AutoUpdate = AppData.Settings.AutoUpdateEnabled;
            CustomPath = AppData.Settings.CustomDataPath;
            CustomBuildImagePath = AppData.Settings.CustomImagePath;
            IsCustomPathEnabled = CustomPath is not null;
            IsCustomBuildImagePathEnabled = CustomBuildImagePath is not null;
            TelemetryDataEnabled = AppData.Settings.SendTelemetryData;
            OpenExplorerAfterImageSave = AppData.Settings.OpenExplorerAfterImageSave;

            if (AppData.DataVersion is null)
            {
                Logging.Logger.Info("AppData.DataVersion is null, reading from VersionInfo.");

                var localVersionInfo = appDataService.ReadLocalVersionInfo(AppData.Settings.SelectedServerType);
                if (localVersionInfo?.CurrentVersion?.MainVersion != null)
                {
                    AppData.DataVersion = localVersionInfo.CurrentVersion.MainVersion.ToString(3) + "#" + localVersionInfo.CurrentVersion.DataIteration;
                }
                else
                {
                    AppData.DataVersion = "No VersionInfo found";
                }
            }

            DataVersion = AppData.DataVersion;
        }

        public string DataVersion
        {
            get => dataVersion;
            set => this.RaiseAndSetIfChanged(ref dataVersion, value);
        }

        public string? CustomPath
        {
            get => customPath;
            set => this.RaiseAndSetIfChanged(ref customPath, value);
        }

        public bool IsCustomPathEnabled
        {
            get => isCustomPathEnabled;
            set => this.RaiseAndSetIfChanged(ref isCustomPathEnabled, value);
        }

        public bool IsCustomBuildImagePathEnabled
        {
            get => isCustomBuildImagePathEnabled;
            set => this.RaiseAndSetIfChanged(ref isCustomBuildImagePathEnabled, value);
        }

        public string? CustomBuildImagePath
        {
            get => customBuildImagePath;
            set => this.RaiseAndSetIfChanged(ref customBuildImagePath, value);
        }

        public string Version { get; }

        public List<CultureDetails> LanguagesList
        {
            get => languagesList;
            set => this.RaiseAndSetIfChanged(ref languagesList, value);
        }

        public CultureDetails SelectedLanguage
        {
            get => selectedLanguage;
            set => this.RaiseAndSetIfChanged(ref selectedLanguage, value);
        }

        public string SelectedServer
        {
            get => selectedServer;
            set => this.RaiseAndSetIfChanged(ref selectedServer, value);
        }

        public bool AutoUpdate
        {
            get => autoUpdate;
            set => this.RaiseAndSetIfChanged(ref autoUpdate, value);
        }

        public List<string> Servers
        {
            get => servers;
            set => this.RaiseAndSetIfChanged(ref servers, value);
        }

        public bool TelemetryDataEnabled
        {
            get => telemetryDataEnabled;
            set => this.RaiseAndSetIfChanged(ref telemetryDataEnabled, value);
        }

        public bool OpenExplorerAfterImageSave
        {
            get => openExplorerAfterImageSave;
            set => this.RaiseAndSetIfChanged(ref openExplorerAfterImageSave, value);
        }

        public Interaction<(string title, string text), Unit> ShowErrorInteraction { get; } = new();

        public Interaction<Unit, Unit> ShutdownInteraction { get; } = new();

        public Interaction<Unit, Unit> ShowDownloadWindowInteraction { get; } = new();

        public Interaction<Unit, Unit> CloseInteraction { get; } = new();

        public Interaction<string, string?> SelectFolderInteraction { get; } = new();

        public Interaction<Unit, bool> RestartAppMessageInteraction { get; } = new();

        public void ResetSettings()
        {
            var cleanSettings = new AppSettings();
            AppData.Settings = cleanSettings;
        }

        public abstract void CleanAppData();

        public async void Save()
        {
            Logging.Logger.Info("Saving settings");
            bool serverChanged = AppData.Settings.SelectedServerType != Enum.Parse<ServerType>(SelectedServer);
            bool pathChanged = AppData.Settings.CustomDataPath != null && !IsCustomPathEnabled;
            bool imagePathChanged = IsCustomBuildImagePathEnabled ? !(CustomBuildImagePath?.Equals(AppData.Settings.CustomImagePath) ?? false) : AppData.Settings.CustomImagePath != null;
            bool cultureChanged = false;
            if (IsCustomPathEnabled)
            {
                if (!IsValidPath(CustomPath!))
                {
                    await ShowErrorInteraction.Handle((Translation.MessageBox_Error, Translation.SettingsWindow_InvalidCustomPath));
                    return;
                }
                else
                {
                    pathChanged = !AppData.Settings.CustomDataPath?.Equals(CustomPath) ?? CustomPath != null;
                    AppData.Settings.CustomDataPath = CustomPath;
                }
            }
            else
            {
                AppData.Settings.CustomDataPath = null;
            }

            if (imagePathChanged)
            {
                if (!IsCustomBuildImagePathEnabled || string.IsNullOrWhiteSpace(CustomBuildImagePath))
                {
                    AppData.Settings.CustomImagePath = null;
                }
                else if (IsValidPath(CustomBuildImagePath ?? string.Empty))
                {
                    AppData.Settings.CustomImagePath = CustomBuildImagePath;
                }
                else
                {
                    await ShowErrorInteraction.Handle((Translation.MessageBox_Error, Translation.SettingsWindow_BuildImagePathInvalid));
                    return;
                }
            }

            AppData.Settings.AutoUpdateEnabled = AutoUpdate;
            AppData.Settings.SelectedServerType = Enum.Parse<ServerType>(SelectedServer);

            // AppData.Settings.Locale = languages[SelectedLanguage];
            if (!AppData.Settings.SelectedLanguage.Equals(SelectedLanguage))
            {
                AppData.Settings.SelectedLanguage = SelectedLanguage;
                cultureChanged = true;
            }

            AppData.Settings.SendTelemetryData = TelemetryDataEnabled;
            AppData.Settings.OpenExplorerAfterImageSave = OpenExplorerAfterImageSave;

            if (serverChanged || pathChanged)
            {
                AppData.ResetCaches();
                await ShowDownloadWindowInteraction.Handle(Unit.Default);
            }

            if (cultureChanged)
            {
                await OnSelectedLocaleChangedAsync();
            }

            await CloseInteraction.Handle(Unit.Default);
        }

        public async void SelectCachePath()
        {
            string? cachePath = await SelectFolder();
            if (!string.IsNullOrWhiteSpace(cachePath))
            {
                CustomPath = cachePath;
                IsCustomPathEnabled = true;
            }
        }

        public async void SelectBuildImagePath()
        {
            string? imagePath = await SelectFolder();
            if (!string.IsNullOrWhiteSpace(imagePath))
            {
                CustomBuildImagePath = imagePath;
            }
        }

        public async void Cancel()
        {
            await CloseInteraction.Handle(Unit.Default);
        }

        public async void CopyVersion()
        {
            var appVersion = $"App Version: {Version}{Environment.NewLine}Data Version: {DataVersion}";
            await clipboardService.SetTextAsync(appVersion);
        }

        protected virtual Task OnSelectedLocaleChangedAsync()
        {
            return Task.CompletedTask;
        }

        private async Task<string?> SelectFolder()
        {
            return await SelectFolderInteraction.Handle(AppDataService.AppDataDirectory);
        }

        private bool IsValidPath(string path, bool exactPath = true)
        {
            bool isValid;
            try
            {
                if (exactPath)
                {
                    string root = fileSystem.Path.GetPathRoot(path)!;
                    isValid = string.IsNullOrEmpty(root.Trim('\\', '/')) == false;
                }
                else
                {
                    isValid = fileSystem.Path.IsPathRooted(path);
                }
            }
            catch (Exception)
            {
                isValid = false;
            }

            return isValid;
        }
    }
}

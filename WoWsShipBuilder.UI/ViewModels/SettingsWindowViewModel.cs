using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using Squirrel;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.UI.Translations;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.UI.Views;

namespace WoWsShipBuilder.UI.ViewModels
{
    class SettingsWindowViewModel : ViewModelBase
    {
        private readonly SettingsWindow? self;

        private readonly IFileSystem fileSystem;

        public SettingsWindowViewModel()
            : this(null, new FileSystem())
        {
            if (!Design.IsDesignMode)
            {
                throw new InvalidOperationException("This constructor must not be used in the live application.");
            }
        }

        public SettingsWindowViewModel(SettingsWindow? win, IFileSystem? fileSystem = null)
        {
            Logging.Logger.Info("Creating setting window view model");
            self = win;
            this.fileSystem = fileSystem ?? new FileSystem();
            languagesList = AppDataHelper.Instance.SupportedLanguages.ToList(); // Copy existing list. Do not change!
            SelectedLanguage = languagesList.FirstOrDefault(languageDetails => languageDetails.CultureInfo.Equals(AppData.Settings.SelectedLanguage.CultureInfo))
                               ?? AppDataHelper.Instance.DefaultCultureDetails;
            Servers = Enum.GetNames<ServerType>().ToList();
            SelectedServer = Enum.GetName(typeof(ServerType), AppData.Settings.SelectedServerType)!;
            AutoUpdate = AppData.Settings.AutoUpdateEnabled;
            CustomPath = AppData.Settings.CustomDataPath;
            IsCustomPathEnabled = !(CustomPath is null);
            TelemetryDataEnabled = AppData.Settings.SendTelemetryData;
            OpenExplorerAfterImageSave = AppData.Settings.OpenExplorerAfterImageSave;

            if (AppData.DataVersion is null)
            {
                Logging.Logger.Info("AppData.DataVersion is null, reading from VersionInfo.");

                var localVersionInfo = AppDataHelper.Instance.ReadLocalVersionInfo(AppData.Settings.SelectedServerType);
                AppData.DataVersion = localVersionInfo?.VersionName ?? "No VersionInfo found";
            }

            DataVersion = AppData.DataVersion;
        }

        private string dataVersion = default!;

        public string DataVersion
        {
            get => dataVersion;
            set => this.RaiseAndSetIfChanged(ref dataVersion, value);
        }

        private string? customPath;

        public string? CustomPath
        {
            get => customPath;
            set => this.RaiseAndSetIfChanged(ref customPath, value);
        }

        private bool isCustomPathEnabled;

        public bool IsCustomPathEnabled
        {
            get => isCustomPathEnabled;
            set => this.RaiseAndSetIfChanged(ref isCustomPathEnabled, value);
        }

        private string version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

        public string Version
        {
            get => version;
            set => this.RaiseAndSetIfChanged(ref version, value);
        }

        private List<CultureDetails> languagesList;

        public List<CultureDetails> LanguagesList
        {
            get => languagesList;
            set => this.RaiseAndSetIfChanged(ref languagesList, value);
        }

        private CultureDetails selectedLanguage = null!;

        public CultureDetails SelectedLanguage
        {
            get => selectedLanguage;
            set => this.RaiseAndSetIfChanged(ref selectedLanguage, value);
        }

        private string selectedServer = null!;

        public string SelectedServer
        {
            get => selectedServer;
            set => this.RaiseAndSetIfChanged(ref selectedServer, value);
        }

        private bool autoUpdate;

        public bool AutoUpdate
        {
            get => autoUpdate;
            set => this.RaiseAndSetIfChanged(ref autoUpdate, value);
        }

        private List<string> servers = null!;

        public List<string> Servers
        {
            get => servers;
            set => this.RaiseAndSetIfChanged(ref servers, value);
        }

        private bool telemetryDataEnabled;

        public bool TelemetryDataEnabled
        {
            get => telemetryDataEnabled;
            set => this.RaiseAndSetIfChanged(ref telemetryDataEnabled, value);
        }

        private bool openExplorerAfterImageSave;

        public bool OpenExplorerAfterImageSave
        {
            get => openExplorerAfterImageSave;
            set => this.RaiseAndSetIfChanged(ref openExplorerAfterImageSave, value);
        }

        public void ResetSettings()
        {
            var cleanSettings = new AppSettings();
            AppData.Settings = cleanSettings;
        }

        [SuppressMessage("System.IO.Abstractions", "IO0007", Justification = "Method just delete a folder.")]
        public async void CleanAppData()
        {
            var result = await MessageBox.Show(self, $"Do you want to delete all data?{Environment.NewLine}This will close the program.", "Warning", MessageBox.MessageBoxButtons.YesNo, MessageBox.MessageBoxIcon.Warning);
            if (result == MessageBox.MessageBoxResult.Yes)
            {
                var appData = AppDataHelper.Instance.AppDataDirectory;
                var appDataDir = new DirectoryInfo(appData);
                if (appDataDir.Exists)
                {
                    appDataDir.Delete(true);
                    (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
                }
                else
                {
                    await MessageBox.Show(self, $"Error in deleting data. Data folder does not exit", "Warning", MessageBox.MessageBoxButtons.Ok, MessageBox.MessageBoxIcon.Error);
                }
            }
        }

        public void Donate()
        {
            string url = "https://www.buymeacoffee.com/WoWsShipBuilder";

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
        }

        public async void Save()
        {
            Logging.Logger.Info("Saving settings");
            bool serverChanged = AppData.Settings.SelectedServerType != Enum.Parse<ServerType>(SelectedServer);
            bool pathChanged = AppData.Settings.CustomDataPath != null && !IsCustomPathEnabled;
            bool cultureChanged = false;
            if (IsCustomPathEnabled)
            {
                if (!IsValidPath(CustomPath!))
                {
                    await MessageBox.Show(self, Translation.SettingsWindow_InvalidCustomPath, Translation.MessageBox_Error, MessageBox.MessageBoxButtons.Ok, MessageBox.MessageBoxIcon.Error);
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
                await new DownloadWindow().ShowDialog(self);
            }

            if (cultureChanged)
            {
                var result = await MessageBox.Show(
                    null,
                    Translation.Settingswindow_LanguageChanged,
                    Translation.SettingsWindow_LanguageChanged_Title,
                    MessageBox.MessageBoxButtons.YesNo,
                    MessageBox.MessageBoxIcon.Question,
                    sizeToContent: SizeToContent.Height);
                if (result == MessageBox.MessageBoxResult.Yes)
                {
                    AppSettingsHelper.SaveSettings();
                    UpdateManager.RestartApp();
                }
            }

            self?.Close();
        }

        public async void SelectFolder()
        {
            var dialog = new OpenFolderDialog
            {
                Directory = AppDataHelper.Instance.AppDataDirectory,
            };
            var path = await dialog.ShowAsync(self!);
            if (!string.IsNullOrEmpty(path))
            {
                CustomPath = path;
                IsCustomPathEnabled = true;
            }
        }

        public void Cancel()
        {
            self?.Close();
        }

        public async void CopyVersion()
        {
            var appVersion = $"App Version: {Version}{Environment.NewLine}Data Version: {DataVersion}";
            await Application.Current.Clipboard.SetTextAsync(appVersion);
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

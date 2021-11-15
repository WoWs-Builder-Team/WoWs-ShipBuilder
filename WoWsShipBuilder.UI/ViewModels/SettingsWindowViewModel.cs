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
using Newtonsoft.Json;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.UI.Translations;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.UI.Views;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    class SettingsWindowViewModel : ViewModelBase
    {
        private readonly SettingsWindow self;

        private readonly IFileSystem fileSystem;

        public SettingsWindowViewModel(SettingsWindow win, IFileSystem? fileSystem = null)
        {
            Logging.Logger.Info("Creating setting window view model");
            self = win;
            this.fileSystem = fileSystem ?? new FileSystem();
            LanguagesList = languages.Keys.ToList();
            SelectedLanguage = languages.Keys.First();
            Servers = Enum.GetNames<ServerType>().ToList();
            SelectedServer = Enum.GetName(typeof(ServerType), AppData.Settings.SelectedServerType)!;
            AutoUpdate = AppData.Settings.AutoUpdateEnabled;
            CustomPath = AppData.Settings.CustomDataPath;
            IsCustomPathEnabled = !(CustomPath is null);

            if (AppData.DataVersion is null)
            {
                Logging.Logger.Info("AppData.DataVersion is null, reading from VersionInfo.");
                string dataPath = AppDataHelper.Instance.GetDataPath(AppData.Settings.SelectedServerType);
                string localVersionInfoPath = this.fileSystem.Path.Combine(dataPath, "VersionInfo.json");
                VersionInfo localVersionInfo = JsonConvert.DeserializeObject<VersionInfo>(this.fileSystem.File.ReadAllText(localVersionInfoPath))!;
                AppData.DataVersion = localVersionInfo.VersionName;
            }

            DataVersion = AppData.DataVersion;
        }

        // Add here all the currently supported languages
        private readonly Dictionary<string, string> languages = new()
        {
            { "English", "en-GB" },
        };

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

        private bool isCustomPathEnabled = false;

        public bool IsCustomPathEnabled
        {
            get => isCustomPathEnabled;
            set => this.RaiseAndSetIfChanged(ref isCustomPathEnabled, value);
        }

        private string version = $"{Assembly.GetExecutingAssembly().GetName().Version!.Major}.{Assembly.GetExecutingAssembly().GetName().Version!.Minor}.{Assembly.GetExecutingAssembly().GetName().Version!.Build}";

        public string Version
        {
            get => version;
            set => this.RaiseAndSetIfChanged(ref version, value);
        }

        private List<string>? languagesList;

        public List<string>? LanguagesList
        {
            get => languagesList;
            set => this.RaiseAndSetIfChanged(ref languagesList, value);
        }

        private string selectedLanguage = null!;

        public string SelectedLanguage
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
                    AppData.Settings.SelectedServerType = Enum.Parse<ServerType>(SelectedServer);
                    AppData.Settings.AutoUpdateEnabled = AutoUpdate;
                    AppData.Settings.Locale = languages[SelectedLanguage];
                }
            }
            else
            {
                AppData.Settings.CustomDataPath = null;
                AppData.Settings.SelectedServerType = Enum.Parse<ServerType>(SelectedServer);
                AppData.Settings.AutoUpdateEnabled = AutoUpdate;
                AppData.Settings.Locale = languages[SelectedLanguage];
            }

            if (serverChanged || pathChanged)
            {
                await new DownloadWindow().ShowDialog(self);
            }

            self.Close();
        }

        public async void SelectFolder()
        {
            OpenFolderDialog dialog = new OpenFolderDialog();
            dialog.Directory = AppDataHelper.Instance.AppDataDirectory;
            var path = await dialog.ShowAsync(self);
            if (!string.IsNullOrEmpty(path))
            {
                CustomPath = path;
                IsCustomPathEnabled = true;
            }
        }

        public void Cancel()
        {
            self.Close();
        }

        [SuppressMessage("System.IO.Abstractions", "IO0006:Replace Path class with IFileSystem.Path for improved testability", Justification = "Checking Path Existence only")]
        private bool IsValidPath(string path, bool exactPath = true)
        {
            bool isValid;
            try
            {
                string fullPath = Path.GetFullPath(path);

                if (exactPath)
                {
                    string root = Path.GetPathRoot(path)!;
                    isValid = string.IsNullOrEmpty(root.Trim(new char[] { '\\', '/' })) == false;
                }
                else
                {
                    isValid = Path.IsPathRooted(path);
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

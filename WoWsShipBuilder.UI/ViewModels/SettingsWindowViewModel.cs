using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.UI.Views;

namespace WoWsShipBuilder.UI.ViewModels
{
    class SettingsWindowViewModel : ViewModelBase
    {
        private readonly SettingsWindow self;

        public SettingsWindowViewModel(SettingsWindow win)
        {
            self = win;
            ResetSettingsCommand = ReactiveCommand.Create(() => ResetSettings());
            CleanAppDataCommand = ReactiveCommand.Create(() => CleanAppData());
            DonateCommand = ReactiveCommand.Create(() => OpenPaypalPage());
            LanguagesList = languages.Keys.ToList();
            SelectedLanguage = languages.Keys.First();
            Servers = Enum.GetNames<ServerType>().ToList();
            SelectedServer = Enum.GetName(typeof(ServerType), AppData.Settings.SelectedServerType)!;
        }

        // Add here all the currently supported languages
        private readonly Dictionary<string, string> languages = new()
        {
            { "English", "en-GB" },
        };

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
            set
            {
                this.RaiseAndSetIfChanged(ref selectedLanguage, value);
                SelectedLanguageChanged();
            }
        }

        private string selectedServer = null!;

        public string SelectedServer
        {
            get => selectedServer;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedServer, value);
                ServerChanged(value);
            }
        }

        private List<string> servers = null!;

        public List<string> Servers
        {
            get => servers;
            set => this.RaiseAndSetIfChanged(ref servers, value);
        }

        private void SelectedLanguageChanged()
        {
            AppData.Settings!.Locale = languages[SelectedLanguage];
        }

        public ICommand ResetSettingsCommand { get; }

        public ICommand CleanAppDataCommand { get; }

        public ICommand DonateCommand { get; }

        private void ResetSettings()
        {
            var cleanSettings = new AppSettings();
            AppData.Settings = cleanSettings;
        }

        [SuppressMessage("System.IO.Abstractions", "IO0007", Justification = "Method just delete a folder.")]
        private async void CleanAppData()
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

        private void OpenPaypalPage()
        {
            Debug.WriteLine("Paypal");
            string url = "";

            string business = "my@paypalemail.com";  // your paypal email
            string description = "Donation";            // '%20' represents a space. remember HTML!
            string country = "AU";                  // AU, US, etc.
            string currency = "AUD";                 // AUD, USD, etc.

            url += "https://www.paypal.com/cgi-bin/webscr" +
                "?cmd=" + "_donations" +
                "&business=" + business +
                "&lc=" + country +
                "&item_name=" + description +
                "&currency_code=" + currency +
                "&bn=" + "PP%2dDonationsBF";

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
        }

        private bool autoUpdate;

        public bool AutoUpdate
        {
            get => autoUpdate;
            set
            {
                this.RaiseAndSetIfChanged(ref autoUpdate, value);
                AutoUpdateChanged(value);
            }
        }

        private void AutoUpdateChanged(bool autoUpdate)
        {
            AppData.Settings!.AutoUpdateEnabled = autoUpdate;
        }

        private void ServerChanged(string server)
        {
            AppData.Settings!.SelectedServerType = Enum.Parse<ServerType>(server);
        }
    }
}

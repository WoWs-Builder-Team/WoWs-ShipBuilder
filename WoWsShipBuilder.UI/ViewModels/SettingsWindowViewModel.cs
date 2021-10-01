using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.UI.Views;

namespace WoWsShipBuilder.UI.ViewModels
{
    class SettingsWindowViewModel : ViewModelBase
    {
        private SettingsWindow self;

        public SettingsWindowViewModel(SettingsWindow win)
        {
            self = win;
            ResetSettingsCommand = ReactiveCommand.Create(() => ResetSettings());
            CleanAppDataCommand = ReactiveCommand.Create(() => CleanAppData());
            DonateCommand = ReactiveCommand.Create(() => OpenPaypalPage());
            selectedLanguage = languages.Keys.First();
            LanguagesList = languages.Keys.ToList();
        }

        // Add here all the currently supported languages
        private Dictionary<string, string> languages = new Dictionary<string, string>()
        {
            { "English", "en_GB" },
        };

        private List<string>? languagesList;

        public List<string>? LanguagesList
        {
            get => languagesList;
            set => this.RaiseAndSetIfChanged(ref languagesList, value);
        }

        private string selectedLanguage;

        public string SelectedLanguage
        {
            get => selectedLanguage;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedLanguage, value);
                SelectedLanguageChanged();
            } 
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

        private async void CleanAppData()
        {
            var result = await MessageBox.Show(self, $"Do you want to delete all data?{Environment.NewLine}This will restart the program.", "Warning", MessageBox.MessageBoxButtons.YesNo, MessageBox.MessageBoxIcon.Warning);
            Debug.WriteLine(result);

            // var appData = AppDataHelper.AppDataDirectory;
            // var appDataDir = new DirectoryInfo(appData);
            // appDataDir.Delete(true);
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
    }
}

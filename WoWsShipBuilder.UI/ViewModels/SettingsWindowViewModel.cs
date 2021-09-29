using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using Avalonia.Controls;
using ReactiveUI;
using WoWsShipBuilder.UI.Views;

namespace WoWsShipBuilder.UI.ViewModels
{
    class SettingsWindowViewModel : ViewModelBase
    {
        private SettingsWindow self;

        public SettingsWindowViewModel(SettingsWindow window)
        {
            self = window;
            ResetSettingsCommand = ReactiveCommand.Create(() => ResetSettings());
            CleanAppDataCommand = ReactiveCommand.Create(() => CleanAppData());
            DonateCommand = ReactiveCommand.Create(() => OpenPaypalPage());
            selectedLanguage = languages[0];
        }

        private List<string> languages = new List<string>() { "English", "German", "language" };

        public List<string> Languages
        {
            get => languages;
            set => this.RaiseAndSetIfChanged(ref languages, value);
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
            Debug.WriteLine("Language changed to: " + selectedLanguage);
        }

        public ICommand ResetSettingsCommand { get; }

        public ICommand CleanAppDataCommand { get; }

        public ICommand DonateCommand { get; }

        private void ResetSettings()
        {
            Debug.WriteLine("Reset");
        }

        private void CleanAppData()
        {
            Debug.WriteLine("Clean");
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

            System.Diagnostics.Process.Start(new ProcessStartInfo
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
                AutoUpdateChanged();
            }
        }

        private void AutoUpdateChanged()
        {
            Debug.WriteLine("Auto update: " + autoUpdate);
        }
    }
}

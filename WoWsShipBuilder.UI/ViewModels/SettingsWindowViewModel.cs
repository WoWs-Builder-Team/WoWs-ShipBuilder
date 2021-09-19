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

        private void ResetSettings()
        {
            Debug.WriteLine("Reset");
        }

        private void CleanAppData()
        {
            Debug.WriteLine("Clean");
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

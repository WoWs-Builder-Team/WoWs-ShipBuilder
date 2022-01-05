using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.Translations;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.UI.Views;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class BuildCreationWindowViewModel : ViewModelBase
    {
        private readonly BuildCreationWindow? self;
        private readonly Build build;

        public BuildCreationWindowViewModel()
            : this(null, new("Test-build - Test-ship"), "Test-ship")
        {
            if (!Design.IsDesignMode)
            {
                throw new InvalidOperationException();
            }
        }

        public BuildCreationWindowViewModel(BuildCreationWindow? win, Build currentBuild, string shipName)
        {
            self = win;
            build = currentBuild;
            ShipName = shipName;
            BuildName = build.BuildName.Replace(" - " + ShipName, string.Empty);
            IsNewBuild = string.IsNullOrEmpty(BuildName);
            IncludeSignals = AppData.Settings.IncludeSignalsForImageExport;
        }

        private string shipName = default!;

        public string ShipName
        {
            get => shipName;
            set => this.RaiseAndSetIfChanged(ref shipName, value);
        }

        private string? buildName;

        public string? BuildName
        {
            get => buildName;
            set => this.RaiseAndSetIfChanged(ref buildName, value);
        }

        private bool includeSignals;

        public bool IncludeSignals
        {
            get => includeSignals;
            set => this.RaiseAndSetIfChanged(ref includeSignals, value);
        }

        public bool IsNewBuild { get; }

        public async void SaveBuild()
        {
            build.BuildName = CreateEffectiveBuildName();
            var buildString = build.CreateStringFromBuild();
            var oldBuild = AppData.Builds.FirstOrDefault(existingBuild => existingBuild.BuildName.Equals(build.BuildName));
            if (oldBuild != null)
            {
                Logging.Logger.Info("Removing old build with identical name from list of saved builds to replace with new build.");
                AppData.Builds.Remove(oldBuild);
            }

            AppData.Builds.Insert(0, build);
            await Application.Current!.Clipboard!.SetTextAsync(buildString);
            await MessageBox.Show(self, Translation.BuildCreationWindow_SavedClipboard, Translation.BuildCreationWindow_BuildSaved, MessageBox.MessageBoxButtons.Ok, MessageBox.MessageBoxIcon.Info);
            self?.Close((true, false));
        }

        [DependsOn(nameof(BuildName))]
        public bool CanSaveBuild(object parameter) => !string.IsNullOrWhiteSpace(BuildName);

        public void ExportScreenshot()
        {
            build.BuildName = CreateEffectiveBuildName();
            self?.Close((true, IncludeSignals));
        }

        [DependsOn(nameof(BuildName))]
        public bool CanExportScreenshot(object parameter) => CanSaveBuild(parameter);

        public void CloseBuild()
        {
            self?.Close((false, false));
        }

        private string CreateEffectiveBuildName() => BuildName + " - " + ShipName;
    }
}

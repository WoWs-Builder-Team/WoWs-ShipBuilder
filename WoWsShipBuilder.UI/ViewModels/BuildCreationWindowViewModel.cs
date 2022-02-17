using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Metadata;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.Translations;
using WoWsShipBuilder.UI.Views;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class BuildCreationWindowViewModel : ViewModelBase
    {
        private const string BuildNameRegex = "^[a-zA-Z0-9][a-zA-Z0-9_ -]*$";
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
            BuildName = build.BuildName;
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

        [RegularExpression(BuildNameRegex, ErrorMessageResourceName = "Validation_BuildName", ErrorMessageResourceType = typeof(Translation))]
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

        private void SaveBuild()
        {
            build.BuildName = BuildName!;
            var oldBuild = AppData.Builds.FirstOrDefault(existingBuild => existingBuild.BuildName.Equals(build.BuildName));
            if (oldBuild != null)
            {
                Logging.Logger.Info("Removing old build with identical name from list of saved builds to replace with new build.");
                AppData.Builds.Remove(oldBuild);
            }

            AppData.Builds.Insert(0, build);
        }

        private bool CanSaveBuild()
        {
            return !string.IsNullOrWhiteSpace(BuildName) && Regex.IsMatch(BuildName, BuildNameRegex);
        }

        public void SaveAndCopyString()
        {
            SaveBuild();
            self?.Close(new BuildCreationResult(true, IncludeSignals));
        }

        [DependsOn(nameof(BuildName))]
        public bool CanSaveAndCopyString(object parameter) => CanSaveBuild();

        public void SaveAndCopyImage()
        {
            SaveBuild();
            self?.Close(new BuildCreationResult(true, IncludeSignals, true));
        }

        [DependsOn(nameof(BuildName))]
        public bool CanSaveAndCopyImage(object parameter) => CanSaveBuild();

        public void CloseBuild()
        {
            self?.Close(new BuildCreationResult(false));
        }
    }

    public sealed record BuildCreationResult(bool Save, bool IncludeSignals = false, bool CopyImageToClipboard = false)
    {
        public static BuildCreationResult Canceled { get; } = new(false);
    }
}

using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using Avalonia.Metadata;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.Translations;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class BuildCreationWindowViewModel : ViewModelBase
    {
        private const string BuildNameRegex = "^[a-zA-Z0-9][a-zA-Z0-9_ -]*$";
        private readonly Build build;

        public BuildCreationWindowViewModel()
            : this(new("Test-build - Test-ship"), "Test-ship")
        {
        }

        public BuildCreationWindowViewModel(Build currentBuild, string shipName)
        {
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

        public Interaction<BuildCreationResult, Unit> CloseInteraction { get; } = new();

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

        public async void SaveAndCopyString()
        {
            SaveBuild();
            await CloseInteraction.Handle(new(true, IncludeSignals));
        }

        [DependsOn(nameof(BuildName))]
        public bool CanSaveAndCopyString(object parameter) => CanSaveBuild();

        public async void SaveAndCopyImage()
        {
            SaveBuild();
            await CloseInteraction.Handle(new(true, IncludeSignals, true));
        }

        [DependsOn(nameof(BuildName))]
        public bool CanSaveAndCopyImage(object parameter) => CanSaveBuild();

        public async void CloseBuild()
        {
            await CloseInteraction.Handle(new(false));
        }
    }

    public sealed record BuildCreationResult(bool Save, bool IncludeSignals = false, bool CopyImageToClipboard = false)
    {
        public static BuildCreationResult Canceled { get; } = new(false);
    }
}

using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.ViewModels.Helper
{
    public class BuildCreationWindowViewModel : ViewModelBase
    {
        private const string BuildNameRegex = "^[a-zA-Z0-9][a-zA-Z0-9_ -]*$";
        private readonly Build build;

        private readonly AppSettings appSettings;

        public BuildCreationWindowViewModel()
            : this(new(), new("Test-build - Test-ship"), "Test-ship")
        {
        }

        public BuildCreationWindowViewModel(AppSettings appSettings, Build currentBuild, string shipName)
        {
            this.appSettings = appSettings;
            build = currentBuild;
            ShipName = shipName;
            BuildName = build.BuildName;
            IsNewBuild = string.IsNullOrEmpty(BuildName);
            IncludeSignals = appSettings.IncludeSignalsForImageExport;

            var canSaveExecute = this.WhenAnyValue(x => x.BuildName, CanSaveBuild);
            SaveAndCopyStringCommand = ReactiveCommand.CreateFromTask(SaveAndCopyString, canSaveExecute);
            SaveAndCopyImageCommand = ReactiveCommand.CreateFromTask(SaveAndCopyImage, canSaveExecute);
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

        public ReactiveCommand<Unit, Unit> SaveAndCopyStringCommand { get; }

        public ReactiveCommand<Unit, Unit> SaveAndCopyImageCommand { get; }

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

        private bool CanSaveBuild(string? name)
        {
            return !string.IsNullOrWhiteSpace(name) && Regex.IsMatch(name, BuildNameRegex);
        }

        private async Task SaveAndCopyString()
        {
            SaveBuild();
            await CloseInteraction.Handle(new(true, IncludeSignals));
        }

        private async Task SaveAndCopyImage()
        {
            SaveBuild();
            await CloseInteraction.Handle(new(true, IncludeSignals, true));
        }

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

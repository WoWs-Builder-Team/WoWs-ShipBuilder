using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using WoWsShipBuilder.Common.Builds;
using WoWsShipBuilder.Common.Infrastructure;
using WoWsShipBuilder.Common.Infrastructure.Data;
using WoWsShipBuilder.Common.Infrastructure.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.ViewModels.Base;
using WoWsShipBuilder.ViewModels.Helper;

namespace WoWsShipBuilder.ViewModels.Other
{
    public abstract class StartMenuViewModelBase : ViewModelBase
    {
        protected readonly INavigationService NavigationService;

        protected readonly IClipboardService ClipboardService;

        protected readonly IAppDataService AppDataService;

        private readonly ILocalizer localizer;

        private int? selectedBuild;

        public StartMenuViewModelBase(INavigationService navigationService, IClipboardService clipboardService, IAppDataService appDataService, IUserDataService userDataService, ILocalizer localizer)
        {
            NavigationService = navigationService;
            ClipboardService = clipboardService;
            AppDataService = appDataService;
            this.localizer = localizer;
            if (!AppData.Builds.Any())
            {
                userDataService.LoadBuilds();
            }

            var placeholderBuild = new Build(Translation.StartMenu_ImportBuild, string.Empty, Nation.Common, null!, null!, null!, null!, null!, null!);
            var builds = new List<Build> { placeholderBuild };
            builds.AddRange(AppData.Builds);
            BuildList = new(builds);
            BuildList.CollectionChanged += BuildList_CollectionChanged;

            var canDeleteBuild = this.WhenAnyValue(x => x.SelectedBuild, buildNumber => buildNumber > 0);
            DeleteBuildCommand = ReactiveCommand.Create(DeleteBuild, canDeleteBuild);
        }

        public int? SelectedBuild
        {
            get => selectedBuild;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedBuild, value);
                this.RaisePropertyChanged(nameof(LoadBuildButtonText));
            }
        }

        public ObservableCollection<Build> BuildList { get; }

        public string LoadBuildButtonText => SelectedBuild is > 0 ? Translation.StartMenu_LoadBuild : Translation.StartMenu_ImportBuild;

        public Interaction<ShipSelectionWindowViewModel, List<ShipSummary>?> SelectShipInteraction { get; } = new();

        public Interaction<BuildImportViewModelBase, Build?> BuildImportInteraction { get; } = new();

        public Interaction<(string title, string text, bool autoSize), Unit> MessageBoxInteraction { get; } = new();

        public Interaction<SettingsWindowViewModelBase, Unit> ShowSettingsInteraction { get; } = new();

        public ReactiveCommand<Unit, Unit> DeleteBuildCommand { get; }

        public async void NewBuild()
        {
            var dc = new ShipSelectionWindowViewModel(false, ShipSelectionWindowViewModel.LoadParams(localizer));
            var result = (await SelectShipInteraction.Handle(dc))?.FirstOrDefault();
            if (result != null)
            {
                Logging.Logger.LogInformation("Selected ship with index {}", result.Index);
                try
                {
                    var ship = AppData.FindShipFromSummary(result);
                    await NavigationService.OpenMainWindow(ship, result, closeMainWindow: true);
                }
                catch (Exception e)
                {
                    Logging.Logger.LogError(e, "Error during the loading of the local json files");
                    await MessageBoxInteraction.Handle((Translation.MessageBox_Error, Translation.MessageBox_LoadingError, true));
                }
            }
        }

        public void OpenDispersionGraphWindow()
        {
            NavigationService.OpenDispersionPlotWindow(true);
        }

        public async void LoadBuild()
        {
            Build? build;
            if (SelectedBuild is > 0)
            {
                build = BuildList.ElementAt(SelectedBuild.Value);
            }
            else
            {
                build = await BuildImportInteraction.Handle(CreateImportViewModel());
                if (build is null)
                {
                    return;
                }
            }

            Logging.Logger.LogInformation("Loading build {@Build}", build);

            var summary = AppData.ShipSummaryList.SingleOrDefault(ship => ship.Index.Equals(build.ShipIndex));
            if (summary is null)
            {
                await MessageBoxInteraction.Handle((Translation.MessageBox_Error, Translation.StartMenu_BuildLoadingError, false));
                return;
            }

            try
            {
                var ship = AppData.FindShipFromSummary(summary);
                await NavigationService.OpenMainWindow(ship, summary, build, true);
            }
            catch (Exception e)
            {
                Logging.Logger.LogError(e, "Error during the loading of the local json files");
                await MessageBoxInteraction.Handle((Translation.MessageBox_Error, Translation.MessageBox_LoadingError, true));
            }
        }

        protected abstract BuildImportViewModelBase CreateImportViewModel();

        private void DeleteBuild()
        {
            AppData.Builds.RemoveAt(SelectedBuild!.Value - 1);
            BuildList.RemoveAt(SelectedBuild!.Value);
        }

        public abstract void Setting();

        private void BuildList_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.RaisePropertyChanged(nameof(BuildList));
        }
    }
}

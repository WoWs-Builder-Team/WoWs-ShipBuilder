using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.Desktop.Services;
using WoWsShipBuilder.Desktop.ViewModels.Dialog;
using WoWsShipBuilder.Desktop.ViewModels.Helper;
using WoWsShipBuilder.Features.Builds;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.Infrastructure.Data;
using WoWsShipBuilder.Infrastructure.Localization;
using WoWsShipBuilder.Infrastructure.Localization.Resources;
using BuildImportViewModelBase = WoWsShipBuilder.Desktop.ViewModels.Helper.BuildImportViewModelBase;

namespace WoWsShipBuilder.Desktop.ViewModels
{
    public class StartMenuViewModel : ViewModelBase
    {
        private readonly IAppDataService appDataService;

        private readonly IClipboardService clipboardService;

        private readonly IFileSystem fileSystem;

        private readonly ILocalizer localizer;

        private readonly INavigationService navigationService;

        private int? selectedBuild;

        public StartMenuViewModel(IFileSystem fileSystem, INavigationService navigationService, IClipboardService clipboardService, IAppDataService appDataService, IUserDataService userDataService, ILocalizer localizer, AppNotificationService notificationService)
        {
            this.fileSystem = fileSystem;
            NotificationService = notificationService;
            this.navigationService = navigationService;
            this.clipboardService = clipboardService;
            this.appDataService = appDataService;
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

        public AppNotificationService NotificationService { get; }

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

        public Interaction<SettingsWindowViewModel, Unit> ShowSettingsInteraction { get; } = new();

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
                    await navigationService.OpenMainWindow(ship, result, closeMainWindow: true);
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
            navigationService.OpenDispersionPlotWindow(true);
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
                await navigationService.OpenMainWindow(ship, summary, build, true);
            }
            catch (Exception e)
            {
                Logging.Logger.LogError(e, "Error during the loading of the local json files");
                await MessageBoxInteraction.Handle((Translation.MessageBox_Error, Translation.MessageBox_LoadingError, true));
            }
        }

        public async void Setting()
        {
            await ShowSettingsInteraction.Handle(new(fileSystem, clipboardService, appDataService));
        }

        protected BuildImportViewModelBase CreateImportViewModel() => new BuildImportViewModel(fileSystem);

        private void DeleteBuild()
        {
            AppData.Builds.RemoveAt(SelectedBuild!.Value - 1);
            BuildList.RemoveAt(SelectedBuild!.Value);
        }

        private void BuildList_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.RaisePropertyChanged(nameof(BuildList));
        }
    }
}

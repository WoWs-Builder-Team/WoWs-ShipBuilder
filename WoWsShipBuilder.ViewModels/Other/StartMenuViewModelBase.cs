using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Newtonsoft.Json;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.Core.Translations;
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

        protected readonly AppSettings AppSettings;

        private int? selectedBuild;

        public StartMenuViewModelBase(INavigationService navigationService, IClipboardService clipboardService, IAppDataService appDataService, IUserDataService userDataService, ILocalizer localizer, AppSettings appSettings)
        {
            NavigationService = navigationService;
            ClipboardService = clipboardService;
            AppDataService = appDataService;
            this.localizer = localizer;
            AppSettings = appSettings;
            if (!AppData.Builds.Any())
            {
                userDataService.LoadBuilds();
            }

            var builds = new List<Build> { new(Translation.StartMenu_ImportBuild) };
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
            var dc = new ShipSelectionWindowViewModel(false, await ShipSelectionWindowViewModel.LoadParamsAsync(AppDataService, AppSettings, localizer));
            var result = (await SelectShipInteraction.Handle(dc))?.FirstOrDefault();
            if (result != null)
            {
                Logging.Logger.Info($"Selected ship with index {result.Index}");
                try
                {
                    var ship = await AppDataService.GetShipFromSummary(result);
                    await NavigationService.OpenMainWindow(ship!, result, closeMainWindow: true);
                }
                catch (Exception e)
                {
                    Logging.Logger.Error(e, $"Error during the loading of the local json files");
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

            Logging.Logger.Info("Loading build {0}", JsonConvert.SerializeObject(build));

            if (AppData.ShipSummaryList == null)
            {
                Logging.Logger.Info("Ship summary is null, loading it.");
                AppData.ShipSummaryList = await AppDataService.GetShipSummaryList(AppSettings.SelectedServerType);
            }

            var summary = AppData.ShipSummaryList.SingleOrDefault(ship => ship.Index.Equals(build.ShipIndex));
            if (summary is null)
            {
                await MessageBoxInteraction.Handle((Translation.MessageBox_Error, Translation.StartMenu_BuildLoadingError, false));
                return;
            }

            try
            {
                var ship = await AppDataService.GetShipFromSummary(summary);
                await NavigationService.OpenMainWindow(ship!, summary, build, true);
            }
            catch (Exception e)
            {
                Logging.Logger.Error(e, $"Error during the loading of the local json files");
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

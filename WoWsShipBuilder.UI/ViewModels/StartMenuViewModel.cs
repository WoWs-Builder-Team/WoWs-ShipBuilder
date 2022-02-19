using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Newtonsoft.Json;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.UI.Services;
using WoWsShipBuilder.UI.Translations;
using WoWsShipBuilder.UI.Utilities;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class StartMenuViewModel : ViewModelBase, IScalableViewModel
    {
        private readonly IFileSystem fileSystem;

        private readonly INavigationService navigationService;

        private double contentScaling = 1;

        private int? selectedBuild;

        public StartMenuViewModel()
            : this(new FileSystem(), new NavigationService())
        {
        }

        public StartMenuViewModel(IFileSystem fileSystem, INavigationService navigationService)
        {
            this.fileSystem = fileSystem;
            this.navigationService = navigationService;
            if (!AppData.Builds.Any())
            {
                AppDataHelper.Instance.LoadBuilds();
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

        public Interaction<BuildImportViewModel, Build?> BuildImportInteraction { get; } = new();

        public Interaction<(string title, string text, bool autoSize), Unit> MessageBoxInteraction { get; } = new();

        public Interaction<SettingsWindowViewModel, Unit> ShowSettingsInteraction { get; } = new();

        public ReactiveCommand<Unit, Unit> DeleteBuildCommand { get; }

        public double ContentScaling
        {
            get => contentScaling;
            set => this.RaiseAndSetIfChanged(ref contentScaling, value);
        }

        public async void NewBuild()
        {
            var dc = new ShipSelectionWindowViewModel(false);
            var result = (await SelectShipInteraction.Handle(dc))?.FirstOrDefault();
            if (result != null)
            {
                Logging.Logger.Info($"Selected ship with index {result.Index}");
                try
                {
                    var ship = AppDataHelper.Instance.GetShipFromSummary(result)!;
                    AppDataHelper.Instance.LoadNationFiles(result.Nation);
                    navigationService.OpenMainWindow(ship, result, closeMainWindow: true);
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
                build = await BuildImportInteraction.Handle(new(fileSystem));
                if (build is null)
                {
                    return;
                }
            }

            Logging.Logger.Info("Loading build {0}", JsonConvert.SerializeObject(build));

            if (AppData.ShipSummaryList == null)
            {
                Logging.Logger.Info("Ship summary is null, loading it.");
                AppData.ShipSummaryList = AppDataHelper.Instance.GetShipSummaryList(AppData.Settings.SelectedServerType);
            }

            var summary = AppData.ShipSummaryList.SingleOrDefault(ship => ship.Index.Equals(build.ShipIndex));
            if (summary is null)
            {
                await MessageBoxInteraction.Handle((Translation.MessageBox_Error, Translation.StartMenu_BuildLoadingError, false));
                return;
            }

            try
            {
                var ship = AppDataHelper.Instance.GetShipFromSummary(summary)!;
                AppDataHelper.Instance.LoadNationFiles(summary.Nation);
                navigationService.OpenMainWindow(ship, summary, build, true);

                // MainWindow win = new();
                // win.DataContext = new MainWindowViewModel(fileSystem, ship!, summary, build);
                // win.Show();
                // await CloseInteraction.Handle(Unit.Default);
            }
            catch (Exception e)
            {
                Logging.Logger.Error(e, $"Error during the loading of the local json files");
                await MessageBoxInteraction.Handle((Translation.MessageBox_Error, Translation.MessageBox_LoadingError, true));
            }
        }

        private void DeleteBuild()
        {
            AppData.Builds.RemoveAt(SelectedBuild!.Value - 1);
            BuildList.RemoveAt(SelectedBuild!.Value);
        }

        public async void Setting()
        {
            await ShowSettingsInteraction.Handle(new(fileSystem));
        }

        private void BuildList_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.RaisePropertyChanged(nameof(BuildList));
        }
    }
}

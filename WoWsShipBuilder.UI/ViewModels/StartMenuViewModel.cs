using System.IO.Abstractions;
using System.Reactive.Linq;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.UI.Services;
using WoWsShipBuilder.UI.ViewModels.Dialog;
using WoWsShipBuilder.ViewModels.Helper;
using WoWsShipBuilder.ViewModels.Other;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class StartMenuViewModel : StartMenuViewModelBase
    {
        private readonly IFileSystem fileSystem;

        public StartMenuViewModel()
            : this(new FileSystem(), new NavigationService(), new AvaloniaClipboardService(), DesktopAppDataService.PreviewInstance)
        {
        }

        public StartMenuViewModel(IFileSystem fileSystem, INavigationService navigationService, IClipboardService clipboardService, IAppDataService appDataService)
            : base(navigationService, clipboardService, appDataService)
        {
            this.fileSystem = fileSystem;
        }

        public override async void Setting()
        {
            await ShowSettingsInteraction.Handle(new SettingsWindowViewModel(fileSystem, ClipboardService, AppDataService));
        }

        protected override BuildImportViewModelBase CreateImportViewModel() => new BuildImportViewModel(fileSystem);
    }
}

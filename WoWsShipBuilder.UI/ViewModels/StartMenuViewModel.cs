using System.IO.Abstractions;
using System.Reactive.Linq;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.UI.Services;
using WoWsShipBuilder.ViewModels.Other;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class StartMenuViewModel : StartMenuViewModelBase
    {
        public StartMenuViewModel()
            : this(new FileSystem(), new NavigationService(), new AvaloniaClipboardService(), DesktopAppDataService.PreviewInstance)
        {
        }

        public StartMenuViewModel(IFileSystem fileSystem, INavigationService navigationService, IClipboardService clipboardService, IAppDataService appDataService)
            : base(fileSystem, navigationService, clipboardService, appDataService)
        {
        }

        public override async void Setting()
        {
            await ShowSettingsInteraction.Handle(new SettingsWindowViewModel(FileSystem, ClipboardService, AppDataService));
        }
    }
}

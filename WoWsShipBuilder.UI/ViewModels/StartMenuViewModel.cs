using System.IO.Abstractions;
using System.Reactive.Linq;
using Splat;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.UI.Services;
using WoWsShipBuilder.UI.Settings;
using WoWsShipBuilder.UI.ViewModels.Dialog;
using WoWsShipBuilder.ViewModels.Helper;
using WoWsShipBuilder.ViewModels.Other;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class StartMenuViewModel : StartMenuViewModelBase
    {
        private readonly IFileSystem fileSystem;

        public StartMenuViewModel()
            : this(new FileSystem(), new NavigationService(), new AvaloniaClipboardService(), DesktopAppDataService.PreviewInstance, DesktopAppDataService.PreviewInstance, DataHelper.DemoLocalizer, new())
        {
        }

        public StartMenuViewModel(IFileSystem fileSystem, INavigationService navigationService, IClipboardService clipboardService, IAppDataService appDataService, IUserDataService userDataService, ILocalizer localizer, AppNotificationService notificationService)
            : base(navigationService, clipboardService, appDataService, userDataService, localizer, AppSettingsHelper.Settings)
        {
            this.fileSystem = fileSystem;
            NotificationService = notificationService;
        }

        public AppNotificationService NotificationService { get; }

        public override async void Setting()
        {
            await ShowSettingsInteraction.Handle(new SettingsWindowViewModel(fileSystem, ClipboardService, AppDataService));
        }

        protected override BuildImportViewModelBase CreateImportViewModel() => new BuildImportViewModel(fileSystem, Locator.Current.GetServiceSafe<ILocalizer>());
    }
}

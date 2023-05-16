using System.IO.Abstractions;
using System.Reactive.Linq;
using WoWsShipBuilder.Common.Infrastructure;
using WoWsShipBuilder.Common.Infrastructure.Data;
using WoWsShipBuilder.Common.Infrastructure.Localization;
using WoWsShipBuilder.Common.Infrastructure.Navigation;
using WoWsShipBuilder.Core.Localization;
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

        public StartMenuViewModel(IFileSystem fileSystem, INavigationService navigationService, IClipboardService clipboardService, IAppDataService appDataService, IUserDataService userDataService, ILocalizer localizer, AppNotificationService notificationService)
            : base(navigationService, clipboardService, appDataService, userDataService, localizer)
        {
            this.fileSystem = fileSystem;
            NotificationService = notificationService;
        }

        public AppNotificationService NotificationService { get; }

        public override async void Setting()
        {
            await ShowSettingsInteraction.Handle(new SettingsWindowViewModel(fileSystem, ClipboardService, AppDataService));
        }

        protected override BuildImportViewModelBase CreateImportViewModel() => new BuildImportViewModel(fileSystem);
    }
}

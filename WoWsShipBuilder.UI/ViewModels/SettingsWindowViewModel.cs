using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using Squirrel;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.UI.Services;
using WoWsShipBuilder.UI.Settings;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.ViewModels.Other;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class SettingsWindowViewModel : SettingsWindowViewModelBase
    {
        private readonly IFileSystem fileSystem;

        public SettingsWindowViewModel()
            : this(new FileSystem(), new AvaloniaClipboardService(), DesktopAppDataService.PreviewInstance)
        {
        }

        public SettingsWindowViewModel(IFileSystem fileSystem, IClipboardService clipboardService, IAppDataService appDataService)
            : base(clipboardService, appDataService, AppSettingsHelper.Settings)
        {
            this.fileSystem = fileSystem;
        }

        public Interaction<(string title, string text), MessageBox.MessageBoxResult> ShowWarningInteraction { get; } = new();

        [SuppressMessage("System.IO.Abstractions", "IO0007", Justification = "Method just delete a folder.")]
        public override async void CleanAppData()
        {
            var result = await ShowWarningInteraction.Handle(("Warning", $"Do you want to delete all data?{Environment.NewLine}This will close the program."));
            if (result == MessageBox.MessageBoxResult.Yes)
            {
                var appData = AppDataService.AppDataDirectory;
                var appDataDir = new DirectoryInfo(appData);
                if (appDataDir.Exists)
                {
                    foreach (var directoryInfo in appDataDir.GetDirectories())
                    {
                        if (!directoryInfo.Name.Equals("logs"))
                        {
                            directoryInfo.Delete(true);
                        }
                    }

                    foreach (var fileInfo in appDataDir.GetFiles())
                    {
                        fileInfo.Delete();
                    }

                    await ShutdownInteraction.Handle(Unit.Default);
                }
                else
                {
                    await ShowErrorInteraction.Handle(("Warning", "Error in deleting data. Data folder does not exit"));
                }
            }
        }

        protected override async Task OnSelectedLocaleChangedAsync()
        {
            bool result = await RestartAppMessageInteraction.Handle(Unit.Default);
            if (result)
            {
                AppSettingsHelper.SaveSettings();
                if (OperatingSystem.IsWindows())
                {
                    UpdateManager.RestartApp();
                }
            }
        }

        protected override bool IsValidPath(string path, bool exactPath = true)
        {
            bool isValid;
            try
            {
                if (exactPath)
                {
                    string root = fileSystem.Path.GetPathRoot(path)!;
                    isValid = string.IsNullOrEmpty(root.Trim('\\', '/')) == false;
                }
                else
                {
                    isValid = fileSystem.Path.IsPathRooted(path);
                }
            }
            catch (Exception)
            {
                isValid = false;
            }

            return isValid;
        }
    }
}

using System.IO.Abstractions;
using System.Reactive;
using System.Reactive.Linq;
using Newtonsoft.Json;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.UI.Settings;
using WoWsShipBuilder.UI.Utilities;
using WoWsShipBuilder.ViewModels.Helper;

namespace WoWsShipBuilder.UI.ViewModels.Dialog
{
    public class BuildImportViewModel : BuildImportViewModelBase
    {
        private readonly IFileSystem fileSystem;

        public BuildImportViewModel()
            : this(new FileSystem(), DataHelper.DemoLocalizer)
        {
        }

        public BuildImportViewModel(IFileSystem fileSystem, ILocalizer localizer)
            : base(localizer)
        {
            this.fileSystem = fileSystem;
        }

        public async void LoadFromImage(object parameter)
        {
            string[]? result = await FileDialogInteraction.Handle(Unit.Default);
            if (result is not { Length: > 0 })
            {
                return;
            }

            AppSettingsHelper.Settings.LastImageImportPath = fileSystem.Path.GetDirectoryName(result[0]);
            string buildJson = BuildImageProcessor.ExtractBuildData(result[0]);

            JsonSerializerSettings serializerSettings = new()
            {
                Error = (_, args) =>
                {
                    if (args.ErrorContext.Error is JsonReaderException)
                    {
                        Logging.Logger.Info("Tried to load an invalid build string from an image file. Message: {}", args.ErrorContext.Error.Message);
                        args.ErrorContext.Handled = true;
                    }
                },
            };
            var build = JsonConvert.DeserializeObject<Build>(buildJson, serializerSettings);

            if (build == null)
            {
                await MessageBoxInteraction.Handle(("Could not read build data from file. Has the file been compressed?", "Invalid file"));
                return;
            }

            await ProcessLoadedBuild(build);
        }
    }
}

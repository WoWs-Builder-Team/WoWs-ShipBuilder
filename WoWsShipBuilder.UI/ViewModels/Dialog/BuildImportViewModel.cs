using System;
using System.IO.Abstractions;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Features.Builds;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.UI.Settings;
using WoWsShipBuilder.UI.Utilities;
using WoWsShipBuilder.ViewModels.Helper;
using AppSettingsHelper = WoWsShipBuilder.UI.Settings.AppSettingsHelper;

namespace WoWsShipBuilder.UI.ViewModels.Dialog
{
    public class BuildImportViewModel : BuildImportViewModelBase
    {
        private readonly IFileSystem fileSystem;

        public BuildImportViewModel()
            : this(new FileSystem())
        {
        }

        public BuildImportViewModel(IFileSystem fileSystem)
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
                        Logging.Logger.LogInformation("Tried to load an invalid build string from an image file. Message: {}", args.ErrorContext.Error.Message);
                        args.ErrorContext.Handled = true;
                    }
                },
            };

            var build = JsonConvert.DeserializeObject<Build>(buildJson, serializerSettings);

            if (build == null)
            {
                try
                {
                    build = Build.CreateBuildFromString(buildJson);
                }
                catch (FormatException)
                {
                    await MessageBoxInteraction.Handle(("Could not read build data from file. Has the file been compressed?", "Invalid file"));
                    return;
                }
            }

            await ProcessLoadedBuild(build);
        }
    }
}

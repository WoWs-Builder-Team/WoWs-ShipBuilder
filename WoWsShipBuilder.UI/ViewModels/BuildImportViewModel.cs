using System;
using System.IO.Abstractions;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Metadata;
using Newtonsoft.Json;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.Utilities;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class BuildImportViewModel : ViewModelBase
    {
        private readonly IFileSystem fileSystem;

        public BuildImportViewModel()
            : this(new FileSystem())
        {
            if (!Design.IsDesignMode)
            {
                throw new InvalidOperationException();
            }
        }

        public BuildImportViewModel(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        private bool importOnly = true;

        public bool ImportOnly
        {
            get => importOnly;
            set => this.RaiseAndSetIfChanged(ref importOnly, value);
        }

        private string? buildString;

        public string? BuildString
        {
            get => buildString;
            set => this.RaiseAndSetIfChanged(ref buildString, value);
        }

        public Interaction<Build?, Unit> CloseInteraction { get; } = new();

        public Interaction<Unit, string[]?> FileDialogInteraction { get; } = new();

        public Interaction<(string text, string title), Unit> MessageBoxInteraction { get; } = new();

        public async void Cancel()
        {
            BuildString = null;
            await CloseInteraction.Handle(null); // TODO: async?
        }

        public async void LoadFromImage(object parameter)
        {
            string[]? result = await FileDialogInteraction.Handle(Unit.Default);
            if (result is not { Length: > 0 })
            {
                return;
            }

            AppData.Settings.LastImageImportPath = fileSystem.Path.GetDirectoryName(result[0]);
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

        public async void Import(object parameter)
        {
            Build build;
            Logging.Logger.Info("Trying to import build string: {0}", BuildString);
            try
            {
                build = Build.CreateBuildFromString(BuildString!);
                Logging.Logger.Info("Build correctly created");
            }
            catch (Exception e)
            {
                Logging.Logger.Warn(e, "Error in creating the build.");
                await MessageBoxInteraction.Handle(("The Build string is not in the correct format.", "Invalid string."));
                return;
            }

            await ProcessLoadedBuild(build);
        }

        [DependsOn(nameof(BuildString))]
        public bool CanImport(object parameter)
        {
            return !string.IsNullOrWhiteSpace(BuildString);
        }

        private async Task ProcessLoadedBuild(Build build)
        {
            if (!ImportOnly)
            {
                Logging.Logger.Info("Adding build to saved ones.");
                AppData.Builds.Insert(0, build);
            }

            await CloseInteraction.Handle(build); // TODO: async?
        }
    }
}

using System;
using System.IO.Abstractions;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.ViewModels.Helper
{
    public class BuildImportViewModelBase : ViewModelBase
    {
        protected readonly IFileSystem FileSystem;

        public BuildImportViewModelBase(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;

            var canImportExecute = this.WhenAnyValue(x => x.BuildString, buildStr => !string.IsNullOrWhiteSpace(buildStr));
            ImportCommand = ReactiveCommand.CreateFromTask(Import, canImportExecute);
        }

        public ReactiveCommand<Unit, Unit> ImportCommand { get; }

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

        private async Task Import()
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

        protected async Task ProcessLoadedBuild(Build build)
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

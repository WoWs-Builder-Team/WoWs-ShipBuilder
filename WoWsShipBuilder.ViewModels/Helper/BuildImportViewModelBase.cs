using System;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using WoWsShipBuilder.Common.Features.Builds;
using WoWsShipBuilder.Common.Infrastructure;
using WoWsShipBuilder.Common.Infrastructure.Data;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;

namespace WoWsShipBuilder.ViewModels.Helper;

public partial class BuildImportViewModelBase : ViewModelBase
{
    protected BuildImportViewModelBase()
    {
        IObservable<bool> canImportExecute = this.WhenAnyValue(x => x.BuildString, buildStr => !string.IsNullOrWhiteSpace(buildStr));
        ImportCommand = ReactiveCommand.CreateFromTask(Import, canImportExecute);
    }

    public ReactiveCommand<Unit, Unit> ImportCommand { get; }

    [Observable]
    private bool importOnly = true;

    [Observable]
    private string? buildString;

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
        Logging.Logger.LogInformation("Trying to import build string: {BuildString}", BuildString);
        try
        {
            build = Build.CreateBuildFromString(BuildString!);
            Logging.Logger.LogInformation("Build correctly created");
        }
        catch (Exception e)
        {
            Logging.Logger.LogWarning(e, "Error in creating the build");
            await MessageBoxInteraction.Handle(("The Build string is not in the correct format.", "Invalid string."));
            return;
        }

        await ProcessLoadedBuild(build);
    }

    protected async Task ProcessLoadedBuild(Build build)
    {
        if (!ImportOnly)
        {
            Logging.Logger.LogInformation("Adding build to saved ones");
            AppData.Builds.Insert(0, build);
        }

        await CloseInteraction.Handle(build); // TODO: async?
    }
}

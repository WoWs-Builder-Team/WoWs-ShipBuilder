using System.ComponentModel.DataAnnotations;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ReactiveUI;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Features.Settings;
using WoWsShipBuilder.Infrastructure;

namespace WoWsShipBuilder.Desktop.ViewModels.Helper;

public partial class BuildCreationWindowViewModel : ViewModelBase
{
    private const string BuildNameRegex = "^[a-zA-Z0-9][a-zA-Z0-9_ -]*$";

    private string? buildName;

    [Observable]
    private bool includeSignals;

    [Observable]
    private string shipName = default!;

    public BuildCreationWindowViewModel(AppSettings appSettings, string shipName, string? buildName)
    {
        ShipName = shipName;
        BuildName = buildName;
        IsNewBuild = string.IsNullOrEmpty(BuildName);
        IncludeSignals = appSettings.IncludeSignalsForImageExport;

        var canSaveExecute = this.WhenAnyValue(x => x.BuildName, CanSaveBuild);
        SaveAndCopyStringCommand = ReactiveCommand.CreateFromTask(SaveAndCopyString, canSaveExecute);
        SaveAndCopyImageCommand = ReactiveCommand.CreateFromTask(SaveAndCopyImage, canSaveExecute);
    }

    [RegularExpression(BuildNameRegex, ErrorMessageResourceName = "Validation_BuildName", ErrorMessageResourceType = typeof(Translation))]
    public string? BuildName
    {
        get => buildName;
        set => this.RaiseAndSetIfChanged(ref buildName, value);
    }

    public bool IsNewBuild { get; }

    public Interaction<BuildCreationResult, Unit> CloseInteraction { get; } = new();

    public ReactiveCommand<Unit, Unit> SaveAndCopyStringCommand { get; }

    public ReactiveCommand<Unit, Unit> SaveAndCopyImageCommand { get; }

    public async void CloseBuild()
    {
        await CloseInteraction.Handle(new(false));
    }

    private bool CanSaveBuild(string? name)
    {
        return !string.IsNullOrWhiteSpace(name) && Regex.IsMatch(name, BuildNameRegex);
    }

    private async Task SaveAndCopyString()
    {
        await CloseInteraction.Handle(new(true, BuildName, IncludeSignals));
    }

    private async Task SaveAndCopyImage()
    {
        await CloseInteraction.Handle(new(true, BuildName, IncludeSignals, true));
    }
}

public sealed record BuildCreationResult(bool Save, string? BuildName = null, bool IncludeSignals = false, bool CopyImageToClipboard = false)
{
    public static BuildCreationResult Canceled { get; } = new(false);
}

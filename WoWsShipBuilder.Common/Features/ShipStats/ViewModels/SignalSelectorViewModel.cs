using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Exterior;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.ShipStats.ViewModels;

public class SignalSelectorViewModel : ReactiveObject
{
    private readonly ILogger<SignalSelectorViewModel> logger;

    private int signalsNumber;

    public SignalSelectorViewModel()
    {
        this.logger = Logging.LoggerFactory.CreateLogger<SignalSelectorViewModel>();
        this.SignalList = LoadSignalList();

        this.WhenAnyValue(x => x.SignalsNumber).Do(_ => this.UpdateCanToggleSkill()).Subscribe();
    }

    public List<KeyValuePair<string, SignalItemViewModel>> SignalList { get; }

    public int SignalsNumber
    {
        get => this.signalsNumber;
        set => this.RaiseAndSetIfChanged(ref this.signalsNumber, value);
    }

    public CustomObservableCollection<Exterior> SelectedSignals { get; } = new();

    // TODO: update to new nullability state
    private static List<KeyValuePair<string, SignalItemViewModel>> LoadSignalList()
    {
        var list = AppData.ExteriorCache[Nation.Common]
            .Select(entry => new KeyValuePair<string, SignalItemViewModel>(entry.Key, new(entry.Value)))
            .Where(x => x.Value.Signal.Type.Equals(ExteriorType.Flags) && x.Value.Signal.Group == 0).OrderBy(x => x.Value.Signal.SortOrder).ToList();
        KeyValuePair<string, SignalItemViewModel> nullPair = new(string.Empty, new(new()));

        // this is so the two in the bottom row are centered
        list.Insert(12, nullPair);
        list.Insert(13, nullPair);
        return list;
    }

    public void SignalCommandExecute(Exterior flag)
    {
        if (this.SelectedSignals.Contains(flag))
        {
            this.SelectedSignals.Remove(flag);
            this.SignalsNumber--;
        }
        else
        {
            this.SelectedSignals.Add(flag);
            this.SignalsNumber++;
        }
    }

    public List<(string, float)> GetModifierList()
    {
        return this.SelectedSignals.SelectMany(m => m.Modifiers.Select(effect => (effect.Key, (float)effect.Value))).ToList();
    }

    public List<string> GetFlagList()
    {
        return this.SelectedSignals.Select(signal => signal.Index).ToList();
    }

    public void LoadBuild(IReadOnlyList<string> initialSignalsNames)
    {
        this.logger.LogInformation("Initial signal configuration found {SignalNames}", string.Join(", ", initialSignalsNames));
        var list = this.SignalList.Select(x => x.Value.Signal).Where(signal => initialSignalsNames.Contains(signal.Index));
        this.SelectedSignals.AddRange(list);
        this.SignalsNumber = this.SelectedSignals.Count;
    }

    private void UpdateCanToggleSkill()
    {
        foreach (var (_, value) in this.SignalList)
        {
            value.CanExecute = this.CheckSignalCommandExecute(value.Signal);
        }
    }

    private bool CheckSignalCommandExecute(object parameter)
    {
        if (parameter is not Exterior flag)
        {
            return false;
        }

        return this.SelectedSignals.Contains(flag) || this.SignalsNumber < 8;
    }
}

public class SignalItemViewModel : ReactiveObject
{
    private bool canExecute;

    public SignalItemViewModel(Exterior exterior)
    {
        this.Signal = exterior;
    }

    public Exterior Signal { get; }

    public bool CanExecute
    {
        get => this.canExecute;
        set => this.RaiseAndSetIfChanged(ref this.canExecute, value);
    }
}

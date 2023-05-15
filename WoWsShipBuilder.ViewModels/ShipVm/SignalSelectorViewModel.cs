using System;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.DataStructures.Exterior;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.ViewModels.ShipVm;

public class SignalSelectorViewModel : ViewModelBase
{
    private readonly ILogger<SignalSelectorViewModel> logger;

    public SignalSelectorViewModel()
    {
        logger = Logging.LoggerFactory.CreateLogger<SignalSelectorViewModel>();
        SignalList = LoadSignalList();

        this.WhenAnyValue(x => x.SignalsNumber).Do(_ => UpdateCanToggleSkill()).Subscribe();
    }

    public List<KeyValuePair<string, SignalItemViewModel>> SignalList { get; }

    private int signalsNumber;

    public int SignalsNumber
    {
        get => signalsNumber;
        set => this.RaiseAndSetIfChanged(ref signalsNumber, value);
    }

    public CustomObservableCollection<Exterior> SelectedSignals { get; } = new();

    private void UpdateCanToggleSkill()
    {
        foreach (var (_, value) in SignalList)
        {
            value.CanExecute = CheckSignalCommandExecute(value.Signal);
        }
    }

    private bool CheckSignalCommandExecute(object parameter)
    {
        if (parameter is not Exterior flag)
        {
            return false;
        }

        return SelectedSignals.Contains(flag) || SignalsNumber < 8;
    }

    public void SignalCommandExecute(Exterior flag)
    {
        if (SelectedSignals.Contains(flag))
        {
            SelectedSignals.Remove(flag);
            SignalsNumber--;
        }
        else
        {
            SelectedSignals.Add(flag);
            SignalsNumber++;
        }
    }

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

    public List<(string, float)> GetModifierList()
    {
        return SelectedSignals.SelectMany(m => m.Modifiers.Select(effect => (effect.Key, (float)effect.Value))).ToList();
    }

    public List<string> GetFlagList()
    {
        return SelectedSignals.Select(signal => signal.Index).ToList();
    }

    public void LoadBuild(IReadOnlyList<string> initialSignalsNames)
    {
        logger.LogInformation("Initial signal configuration found {SignalNames}", string.Join(", ", initialSignalsNames));
        var list = SignalList.Select(x => x.Value.Signal).Where(signal => initialSignalsNames.Contains(signal.Index));
        SelectedSignals.AddRange(list);
        SignalsNumber = SelectedSignals.Count;
    }
}

public class SignalItemViewModel : ViewModelBase
{
    public SignalItemViewModel(Exterior exterior)
    {
        Signal = exterior;
    }

    public Exterior Signal { get; }

    private bool canExecute;

    public bool CanExecute
    {
        get => canExecute;
        set => this.RaiseAndSetIfChanged(ref canExecute, value);
    }
}

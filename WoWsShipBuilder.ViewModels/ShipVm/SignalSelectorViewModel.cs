using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NLog;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.ViewModels.ShipVm
{
    public class SignalSelectorViewModel : ViewModelBase
    {
        private readonly Logger logger;

        public SignalSelectorViewModel(List<KeyValuePair<string, SignalItemViewModel>> availableSignals)
        {
            logger = Logging.GetLogger("SignalSelectorVM");
            SignalList = availableSignals;

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

        public static async Task<List<KeyValuePair<string, SignalItemViewModel>>> LoadSignalList(IAppDataService appDataService, AppSettings appSettings)
        {
            var dict = await appDataService.GetExteriorList(Nation.Common, appSettings.SelectedServerType);
            if (dict == null)
            {
                Logging.Logger.Warn("Unable to load signals from local appdata. Data may be corrupted. Current application state: {0}", AppData.GenerateLogDump(appSettings));
            }

            var list = dict!
                .Select(entry => new KeyValuePair<string, SignalItemViewModel>(entry.Key, new(entry.Value)))
                .Where(x => x.Value.Signal.Type.Equals(ExteriorType.Flags) && x.Value.Signal.Group == 0).OrderBy(x => x.Value.Signal.SortOrder).ToList();
            KeyValuePair<string, SignalItemViewModel> nullPair = new("", new SignalItemViewModel(new Exterior()));

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
            return SelectedSignals.Select(signal => signal.Name).ToList();
        }

        public void LoadBuild(IReadOnlyList<string> initialSignalsNames)
        {
            logger.Info("Initial signal configuration found {0}", string.Join(", ", initialSignalsNames));
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
}

using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Metadata;
using NLog;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    internal class SignalSelectorViewModel : ViewModelBase
    {
        private readonly Logger logger;

        public SignalSelectorViewModel()
        {
            logger = Logging.GetLogger("SignalSelectorVM");
            SignalList = LoadSignalList();
        }

        private List<KeyValuePair<string, Exterior>> signalList = new();

        public List<KeyValuePair<string, Exterior>> SignalList
        {
            get => signalList;
            set => this.RaiseAndSetIfChanged(ref signalList, value);
        }

        private int signalsNumber = 0;

        public int SignalsNumber
        {
            get => signalsNumber;
            set => this.RaiseAndSetIfChanged(ref signalsNumber, value);
        }

        private AvaloniaList<Exterior> selectedSignals = new();

        public AvaloniaList<Exterior> SelectedSignals
        {
            get => selectedSignals;
            set => this.RaiseAndSetIfChanged(ref selectedSignals, value);
        }

        [DependsOn(nameof(SignalsNumber))]
        public bool CanSignalCommandExecute(object parameter)
        {
            if (parameter == null)
            {
                return false;
            }

            Exterior? flag = parameter as Exterior;
            if (flag != null && SelectedSignals.Contains(flag))
            {
                return true;
            }
            else
            {
                if (signalsNumber < 8)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
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

        private List<KeyValuePair<string, Exterior>> LoadSignalList()
        {
            var dict = AppDataHelper.Instance.ReadLocalJsonData<Exterior>(Nation.Common, AppData.Settings.SelectedServerType);
            if (dict == null)
            {
                logger.Warn("Unable to load signals from local appdata. Data may be corrupted. Current application state: {0}", AppData.GenerateLogDump());
            }

            var list = dict!.Where(x => x.Value.Type.Equals(ExteriorType.Flags) && x.Value.Group == 0).OrderBy(x => x.Value.SortOrder).ToList();
            KeyValuePair<string, Exterior> nullPair = new("", new Exterior());

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

        public void LoadBuild(List<string> initialSignalsNames)
        {
            logger.Info("Initial signal configuration found {0}", string.Join(", ", initialSignalsNames));
            var list = SignalList.Select(x => x.Value).Where(signal => initialSignalsNames.Contains(signal.Name));
            SelectedSignals.AddRange(list);
            SignalsNumber = SelectedSignals.Count;
        }
    }
}

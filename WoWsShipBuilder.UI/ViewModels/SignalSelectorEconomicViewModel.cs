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
    internal class SignalSelectorEconomicViewModel : ViewModelBase
    {
        private readonly Logger logger;

        public SignalSelectorEconomicViewModel()
        {
            logger = Logging.GetLogger("SignalSelectorEconomicVM");
            SignalList = LoadSignalList();
        }

        private List<KeyValuePair<string, Exterior>> signalList = new();

        public List<KeyValuePair<string, Exterior>> SignalList
        {
            get => signalList;
            set => this.RaiseAndSetIfChanged(ref signalList, value);
        }

        private AvaloniaList<Exterior> selectedSignals = new();

        public AvaloniaList<Exterior> SelectedSignals
        {
            get => selectedSignals;
            set => this.RaiseAndSetIfChanged(ref selectedSignals, value);
        }

        public void SignalCommandExecute(Exterior flag)
        {
            if (SelectedSignals.Contains(flag))
            {
                SelectedSignals.Remove(flag);
            }
            else
            {
                SelectedSignals.Add(flag);
            }
        }

        private List<KeyValuePair<string, Exterior>> LoadSignalList()
        {
            var dict = AppDataHelper.Instance.ReadLocalJsonData<Exterior>(Nation.Common, AppData.Settings.SelectedServerType);
            if (dict == null)
            {
                logger.Warn("Unable to load economic signals from local appdata. Data may be corrupted. Current application state: {0}", AppData.GenerateLogDump());
            }

            var list = dict!.Where(x => x.Value.Type.Equals(ExteriorType.Flags) && (x.Value.Group == 1 || x.Value.Group == 2)).OrderBy(x => x.Value.SortOrder).ToList();
            KeyValuePair<string, Exterior> nullPair = new("", new Exterior());

            // Delete Service cost signal from list
            list.RemoveAt(1);
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
    }
}

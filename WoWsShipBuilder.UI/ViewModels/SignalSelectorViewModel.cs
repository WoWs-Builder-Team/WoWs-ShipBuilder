using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Metadata;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    internal class SignalSelectorViewModel : ViewModelBase
    {
        public SignalSelectorViewModel()
            : this(0, _ => { })
        {
        }

        public SignalSelectorViewModel(int signalsNumber, Action<Exterior> addSignalFlagModifiersCommand)
        {
            SignalsNumber = signalsNumber;
            signalCommand = addSignalFlagModifiersCommand;
            SignalList = LoadSignalList();
        }

        private List<KeyValuePair<string, Exterior>> signalList = new();

        public List<KeyValuePair<string, Exterior>> SignalList
        {
            get => signalList;
            set => this.RaiseAndSetIfChanged(ref signalList, value);
        }

        private int signalsNumber;

        public int SignalsNumber
        {
            get => signalsNumber;
            set => this.RaiseAndSetIfChanged(ref signalsNumber, value);
        }

        public AvaloniaList<Exterior> SelectedSignals { get; set; } = new AvaloniaList<Exterior>();

        private Action<Exterior>? signalCommand;

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
            signalCommand!.Invoke(flag);
        }

        private List<KeyValuePair<string, Exterior>> LoadSignalList()
        {
            var dict = AppDataHelper.Instance.ReadLocalJsonData<Exterior>(Nation.Common, ServerType.Live);
            var list = dict!.Where(x => x.Value.Type.Equals(ExteriorType.Flags) && x.Value.SortOrder < 14).OrderBy(x => x.Value.SortOrder).ToList();
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
    }
}

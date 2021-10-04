using System;
using System.Collections.Generic;
using Avalonia.Metadata;
using ReactiveUI;

namespace WoWsShipBuilder.UI.ViewModels
{
    internal class SignalSelectorViewModel : ViewModelBase
    {
        public SignalSelectorViewModel()
            : this(0, _ => { })
        {
        }

        public SignalSelectorViewModel(int signalsNumber, Action<string> addSignalFlagModifiersCommand)
        {
            SignalsNumber = signalsNumber;
            signalCommand = addSignalFlagModifiersCommand;
        }

        private int signalsNumber;

        public int SignalsNumber
        {
            get => signalsNumber;
            set => this.RaiseAndSetIfChanged(ref signalsNumber, value);
        }

        public List<string> SelectedSignalIndex { get; set; } = new List<string>();

        private Action<string>? signalCommand;

        [DependsOn(nameof(SignalsNumber))]
        public bool CanSignalCommandExecute(object parameter)
        {
            string? flagIndex = parameter.ToString();
            if (flagIndex != null && SelectedSignalIndex.Contains(flagIndex))
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

        public void SignalCommandExecute(string flagIndex)
        {
            signalCommand!.Invoke(flagIndex);
        }
    }
}

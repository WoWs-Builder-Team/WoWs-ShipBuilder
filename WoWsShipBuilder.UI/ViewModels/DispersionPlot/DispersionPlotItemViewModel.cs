using ReactiveUI;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.UI.ViewModels.DispersionPlot
{
    public class DispersionPlotItemViewModel : ViewModelBase
    {
        public DispersionPlotItemViewModel(DispersionEllipse dispersionEllipse)
        {
            this.dispersionEllipse = dispersionEllipse;
        }

        private DispersionEllipse dispersionEllipse;

        public DispersionEllipse DispersionEllipse
        {
            get => dispersionEllipse;
            set => this.RaiseAndSetIfChanged(ref dispersionEllipse, value);
        }

        private bool isLast = true;

        public bool IsLast
        {
            get => isLast;
            set => this.RaiseAndSetIfChanged(ref isLast, value);
        }

        public void UpdateIsLast(bool newValue)
        {
            IsLast = newValue;
        }
    }
}

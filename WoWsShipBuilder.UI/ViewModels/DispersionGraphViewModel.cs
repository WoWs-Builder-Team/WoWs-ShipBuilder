using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Metadata;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.Translations;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.UI.Views;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class DispersionGraphViewModel : ViewModelBase
    {
        private readonly DispersionGraphsWindow self;

        public DispersionGraphViewModel(DispersionGraphsWindow win, Dispersion disp, double maxRange, string shipIndex)
        {
            self = win;
            var name = Localizer.Instance[$"{shipIndex}_FULL"].Localization;
            var hModel = InitializeBaseModel(Translation.ShipStats_HorizontalDisp);
            var hDisp = CreateHorizontalDispersionSeries(disp, maxRange, name);
            hModel.Series.Add(hDisp);
            HorizontalModel = hModel;

            var vModel = InitializeBaseModel(Translation.ShipStats_VerticalDisp);
            var vDisp = CreateVerticalDispersionSeries(disp, maxRange, name);
            vModel.Series.Add(vDisp);
            VerticalModel = vModel;

            shipNames.Add(name);
        }

        private AvaloniaList<string> shipNames = new();

        public AvaloniaList<string> ShipNames
        {
            get => shipNames;
            set => this.RaiseAndSetIfChanged(ref shipNames, value);
        }

        private PlotModel? horizontalModel;

        public PlotModel? HorizontalModel
        {
            get => horizontalModel;
            set => this.RaiseAndSetIfChanged(ref horizontalModel, value);
        }

        private PlotModel? verticalModel;

        public PlotModel? VerticalModel
        {
            get => verticalModel;
            set => this.RaiseAndSetIfChanged(ref verticalModel, value);
        }

        public async void AddShip()
        {
            ShipSummary result = await ShipSelectionWindow.ShowShipSelection(self);
            Ship? ship = AppDataHelper.Instance.GetShipFromSummary(result, false);
            if (ship != null)
            {
                var shipName = Localizer.Instance[$"{ship.Index}_FULL"].Localization;
                if (!shipNames.Contains(shipName))
                {
                    if (ship.MainBatteryModuleList.Count > 0)
                    {
                        var guns = ship.MainBatteryModuleList.Values.First();

                        var hSeries = CreateHorizontalDispersionSeries(guns.DispersionValues, (double)guns.MaxRange, shipName);
                        var vSeries = CreateVerticalDispersionSeries(guns.DispersionValues, (double)guns.MaxRange, shipName);
                        shipNames.Add(shipName);
                        HorizontalModel!.Series.Add(hSeries);
                        VerticalModel!.Series.Add(vSeries);
                        HorizontalModel!.InvalidatePlot(true);
                        verticalModel!.InvalidatePlot(true);
                        this.RaisePropertyChanged(nameof(ShipNames));
                    }
                    else
                    {
                        var noGunMessage = Translation.ResourceManager.GetString("MessageBox_ShipNoGun", Translation.Culture);
                        var errorTitle = Translation.ResourceManager.GetString("MessageBox_Error", Translation.Culture);
                        await MessageBox.Show(self, noGunMessage!, errorTitle!, MessageBox.MessageBoxButtons.Ok, MessageBox.MessageBoxIcon.Error);
                    }
                }
                else
                {
                    var dupeMessage = Translation.ResourceManager.GetString("MessageBox_DuplicateShip", Translation.Culture);
                    var errorTitle = Translation.ResourceManager.GetString("MessageBox_Error", Translation.Culture);
                    await MessageBox.Show(self, dupeMessage!, errorTitle!, MessageBox.MessageBoxButtons.Ok, MessageBox.MessageBoxIcon.Error);
                }
            }
        }

        public async void RemoveShip()
        {
            var result = await DispersionShipRemovalDialog.ShowShipRemoval(self, shipNames.ToList());
            if (result.Count > 0)
            {
                foreach (var ship in result)
                {
                    var index = shipNames.IndexOf(ship);
                    HorizontalModel!.Series.RemoveAt(index);
                    VerticalModel!.Series.RemoveAt(index);
                    shipNames.RemoveAt(index);
                }

                HorizontalModel!.InvalidatePlot(true);
                VerticalModel!.InvalidatePlot(true);
                this.RaisePropertyChanged(nameof(ShipNames));
            }
        }

        [DependsOn(nameof(ShipNames))]
        public bool CanRemoveShip(object parameter) => shipNames.Count > 0;

        // This are all default values to make the graph looks nice
        private PlotModel InitializeBaseModel(string name)
        {
            var foreground = ConvertColorFromResource("ThemeForegroundColor");
            var foregroundLow = ConvertColorFromResource("ThemeForegroundLowColor");
            var background = ConvertColorFromResource("ThemeBackgroundColor");

            PlotModel model = new()
            {
                Title = name,
                TextColor = foreground,
                PlotAreaBorderColor = foreground,
                LegendPosition = LegendPosition.TopLeft,
                LegendBorder = foreground,
                LegendBorderThickness = 1,
                LegendBackground = background,
                LegendFontSize = 13,
            };

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Range",
                IsPanEnabled = false,
                Unit = "Km",
                Minimum = 0,
                AxislineColor = foreground,
                TextColor = foreground,
                TicklineColor = foreground,
                MajorGridlineThickness = 1,
                MajorGridlineStyle = LineStyle.Dash,
                MajorGridlineColor = foregroundLow,
            };

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Dispersion",
                IsPanEnabled = false,
                Unit = "m",
                Minimum = 0,
                AxislineColor = foreground,
                TextColor = foreground,
                TicklineColor = foreground,
                MajorGridlineThickness = 1,
                MajorGridlineStyle = LineStyle.Dash,
                MajorGridlineColor = foregroundLow,
            };
            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);

            return model;
        }

        private FunctionSeries CreateHorizontalDispersionSeries(Dispersion dispersion, double maxRange, string name)
        {
            var dispSeries = new FunctionSeries(range => dispersion.CalculateHorizontalDispersion(range * 1000), 0, (maxRange * 1.5) / 1000, 0.01, name);
            dispSeries.TrackerFormatString = "{0}\n{1}: {1}: {2:#.00} Km\n{3}: {4:#.00} m";
            dispSeries.StrokeThickness = 4;
            return dispSeries;
        }

        private FunctionSeries CreateVerticalDispersionSeries(Dispersion dispersion, double maxRange, string name)
        {
            var dispSeries = new FunctionSeries(range => dispersion.CalculateVerticalDispersion(maxRange, range * 1000), 0, (maxRange * 1.5) / 1000, 0.01, name);
            dispSeries.TrackerFormatString = "{0}\n{1}: {1}: {2:#.00} Km\n{3}: {4:#.00} m";
            dispSeries.StrokeThickness = 4;
            return dispSeries;
        }

        private OxyColor ConvertColorFromResource(string resourceKey)
        {
            var color = self.FindResource(resourceKey) as Color?;
            return OxyColor.FromUInt32(color!.Value.ToUint32());
        }
    }
}

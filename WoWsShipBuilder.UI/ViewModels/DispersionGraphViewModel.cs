using System;
using System.Collections.Generic;
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
using WoWsShipBuilder.UI.Views;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class DispersionGraphViewModel : ViewModelBase
    {
        private readonly DispersionGraphsWindow self;

        public DispersionGraphViewModel(DispersionGraphsWindow win, Dispersion disp, double maxRange, string shipName)
        {
            self = win;
            var hModel = InitializeBaseModel("Horizontal Dispersion");
            var hdisp = CreateHorizontalDispersionSeries(disp, maxRange, shipName);
            hModel.Series.Add(hdisp);
            HorizontalModel = hModel;
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
            Ship? ship = AppDataHelper.Instance.GetShipFromSummary(result);
            if (ship != null)
            {
                var guns = ship.MainBatteryModuleList.Values.First();
                var series = CreateHorizontalDispersionSeries(guns.DispersionValues, (double)guns.MaxRange, ship.Index);
                HorizontalModel!.Series.Add(series);
                HorizontalModel!.InvalidatePlot(true);
                this.RaisePropertyChanged(nameof(ShipNames));
            }
        }

        public async void RemoveShip()
        {
            var result = await DispersionShipRemovalDialog.ShowShipRemoval(self, shipNames.ToList());
            if (result.Count > 0)
            {
                shipNames.RemoveAll(result);
                var temp = new List<Series>();
                foreach (var serie in HorizontalModel!.Series)
                {
                    if (!result.Contains(serie.Title))
                    {
                        temp.Add(serie);
                    }
                }

                HorizontalModel.Series.Clear();
                foreach (var serie in temp)
                {
                    HorizontalModel.Series.Add(serie);
                }

                HorizontalModel!.InvalidatePlot(true);
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

            PlotModel model = new PlotModel();
            model.Title = name;
            model.TextColor = foreground;
            model.PlotAreaBorderColor = foreground;
            model.LegendPosition = LegendPosition.TopLeft;
            model.LegendBorder = foreground;
            model.LegendBorderThickness = 1;
            model.LegendBackground = background;
            model.LegendFontSize = 13;

            var xAxis = new LinearAxis()
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

            var yAxis = new LinearAxis()
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
            Func<double, double> dispFunc = (range) =>
            {
                double baseValue = range * 1000 * ((dispersion.IdealRadius * 30) - (dispersion.MinRadius * 30)) / (dispersion.IdealDistance * 30);
                if (range * 1000 <= dispersion.TaperDist)
                {
                    return baseValue + (dispersion.MinRadius * 30 * ((range * 1000) / dispersion.TaperDist));
                }
                else
                {
                    return baseValue + (dispersion.MinRadius * 30);
                }
            };
            var shipName = Localizer.Instance[$"{name}_FULL"].Localization;
            shipNames.Add(shipName);
            var dispSeries = new FunctionSeries(dispFunc, 0, maxRange / 1000, 0.01, shipName);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Metadata;
using NLog;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataUI;
using WoWsShipBuilder.Core.Translations;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.UI.Views;
using WoWsShipBuilder.ViewModels.Base;
using WoWsShipBuilder.ViewModels.Helper;
using static WoWsShipBuilder.UI.CustomControls.DispersionPlot;

namespace WoWsShipBuilder.UI.ViewModels.DispersionPlot
{
    public class DispersionGraphViewModel : ViewModelBase
    {
        private readonly DispersionGraphsWindow? self;

        private readonly Logger logger;

        private bool refreshNeeded;

        public enum Tabs
        {
            Dispersion,
            Plot,
            Ballistic,
        }

        public DispersionGraphViewModel()
            : this(null)
        {
            if (!Design.IsDesignMode)
            {
                throw new NotSupportedException();
            }
        }

        public DispersionGraphViewModel(DispersionGraphsWindow? window)
            : this(window, null, 0, string.Empty, null, Tabs.Dispersion, 0)
        {
        }

        public DispersionGraphViewModel(DispersionGraphsWindow? win, Dispersion? disp, double maxRange, string shipIndex, ArtilleryShell? shell, Tabs initialTab, decimal sigma)
        {
            logger = Logging.GetLogger("DispersionGraphVM");
            logger.Info("Opening with initial tab: {0}", initialTab.ToString());

            self = win;
            AddShipInteraction = new();

            AimingRange = AppData.Settings.DispersionPlotSettings.AimingRange;
            ShotsNumber = AppData.Settings.DispersionPlotSettings.ShotsNumber;
            IsVertical = AppData.Settings.DispersionPlotSettings.IsVertical;

            logger.Info("Creating base plot models");
            var hModel = InitializeDispersionBaseModel(Translation.ShipStats_HorizontalDisp);
            var vModel = InitializeDispersionBaseModel(Translation.ShipStats_VerticalDisp);
            var penModel = InitializeBallisticBaseModel(Translation.ShipStats_Penetration, "mm", LegendPosition.TopRight, 0.7);
            var fTModel = InitializeBallisticBaseModel(Translation.DispersionGraphWindow_FlightTime, "s", LegendPosition.TopLeft, 0.2);
            var iVModel = InitializeBallisticBaseModel(Translation.DispersionGraphWindow_ImpactVelocity, "m/s", LegendPosition.TopRight, 0.2);
            var iAModel = InitializeBallisticBaseModel(Translation.DispersionGraphWindow_ImpactAngle, "°", LegendPosition.TopLeft, 0.2);

            if (maxRange > 0 && disp != null && shell != null)
            {
                string shipName = Localizer.Instance[$"{shipIndex}_FULL"].Localization;
                string shellName = Localizer.Instance[$"{shell.Name}"].Localization;
                var name = $"{shipName} - {shellName}";
                logger.Info("Creating series for {0}", name);
                logger.Info("Creating series");
                var hDisp = CreateHorizontalDispersionSeries(disp, maxRange, name);
                var vDisp = CreateVerticalDispersionSeries(disp, maxRange, name);
                var ballisticSeries = CreateBallisticSeries(shell, maxRange, name);

                logger.Info("Adding series and setting models");

                hModel.Series.Add(hDisp);
                vModel.Series.Add(vDisp);

                penModel.Series.Add(ballisticSeries.Penetration);
                fTModel.Series.Add(ballisticSeries.FlightTime);
                iVModel.Series.Add(ballisticSeries.ImpactVelocity);
                iAModel.Series.Add(ballisticSeries.ImpactAngle);
                shipNames.Add(name);

                var plotItemViewModel = new DispersionPlotItemViewModel(DispersionPlotHelper.CalculateDispersionPlotParameters(name, disp, shell, maxRange, AimingRange * 1000, (double)sigma, ShotsNumber));
                DispersionPlotList.Add(plotItemViewModel);
            }

            FlightTimeModel = fTModel;
            ImpactVelocityModel = iVModel;
            ImpactAngleModel = iAModel;
            PenetrationModel = penModel;
            HorizontalModel = hModel;
            VerticalModel = vModel;
            InitialTab = (int)initialTab;

            DispersionPlotList.CollectionChanged += DispersionPlotList_CollectionChanged;
        }

        private void DispersionPlotList_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.RaisePropertyChanged(nameof(DispersionPlotList));
        }

        private int initialTab;

        public int InitialTab
        {
            get => initialTab;
            set => this.RaiseAndSetIfChanged(ref initialTab, value);
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

        private bool showPen = true;

        public bool ShowPen
        {
            get => showPen;
            set => this.RaiseAndSetIfChanged(ref showPen, value);
        }

        private PlotModel? penetrationModel;

        public PlotModel? PenetrationModel
        {
            get => penetrationModel;
            set => this.RaiseAndSetIfChanged(ref penetrationModel, value);
        }

        private bool showFlightTime = true;

        public bool ShowFlightTime
        {
            get => showFlightTime;
            set => this.RaiseAndSetIfChanged(ref showFlightTime, value);
        }

        private PlotModel? flightTimeModel;

        public PlotModel? FlightTimeModel
        {
            get => flightTimeModel;
            set => this.RaiseAndSetIfChanged(ref flightTimeModel, value);
        }

        private bool showImpactVelocity = true;

        public bool ShowImpactVelocity
        {
            get => showImpactVelocity;
            set => this.RaiseAndSetIfChanged(ref showImpactVelocity, value);
        }

        private PlotModel? impactVelocityModel;

        public PlotModel? ImpactVelocityModel
        {
            get => impactVelocityModel;
            set => this.RaiseAndSetIfChanged(ref impactVelocityModel, value);
        }

        private bool showImpactAngle = true;

        public bool ShowImpactAngle
        {
            get => showImpactAngle;
            set => this.RaiseAndSetIfChanged(ref showImpactAngle, value);
        }

        private PlotModel? impactAngleModel;

        public PlotModel? ImpactAngleModel
        {
            get => impactAngleModel;
            set => this.RaiseAndSetIfChanged(ref impactAngleModel, value);
        }

        public AvaloniaList<DispersionPlotItemViewModel> DispersionPlotList { get; } = new();

        private int shotsNumber = 100;

        public int ShotsNumber
        {
            get => shotsNumber;
            set
            {
                this.RaiseAndSetIfChanged(ref shotsNumber, value);
                refreshNeeded = true;
            }
        }

        private double aimingRange = 10;

        public double AimingRange
        {
            get => aimingRange;
            set
            {
                this.RaiseAndSetIfChanged(ref aimingRange, value);
                refreshNeeded = true;
            }
        }

        private decimal plotScaling = 0.7m;

        public decimal PlotScaling
        {
            get => plotScaling;
            set => this.RaiseAndSetIfChanged(ref plotScaling, value);
        }

        private List<EllipsePlanes> ellipsePlanesList = Enum.GetValues<EllipsePlanes>().ToList();

        public List<EllipsePlanes> EllipsePlanesList
        {
            get => ellipsePlanesList;
            set => this.RaiseAndSetIfChanged(ref ellipsePlanesList, value);
        }

        private EllipsePlanes selectedEllipsePlane = EllipsePlanes.HorizontalPlane;

        public EllipsePlanes SelectedEllipsePlane
        {
            get => selectedEllipsePlane;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedEllipsePlane, value);
                if (value == EllipsePlanes.HorizontalPlane)
                {
                    IsHorizontalPlane = true;
                    IsVerticalPlane = false;
                    IsRealPlane = false;
                }
                else if (value == EllipsePlanes.VerticalPlane)
                {
                    IsHorizontalPlane = false;
                    IsVerticalPlane = true;
                    IsRealPlane = false;
                }
                else
                {
                    IsHorizontalPlane = false;
                    IsVerticalPlane = false;
                    IsRealPlane = true;
                }
            }
        }

        private List<FusoPositions> fusoPositionsList = Enum.GetValues<FusoPositions>().ToList();

        public List<FusoPositions> FusoPositionsList
        {
            get => fusoPositionsList;
            set => this.RaiseAndSetIfChanged(ref fusoPositionsList, value);
        }

        private FusoPositions selectedFusoPosition = FusoPositions.DontShow;

        public FusoPositions SelectedFusoPosition
        {
            get => selectedFusoPosition;
            set => this.RaiseAndSetIfChanged(ref selectedFusoPosition, value);
        }

        private bool isVertical = true;

        public bool IsVertical
        {
            get => isVertical;
            set
            {
                this.RaiseAndSetIfChanged(ref isVertical, value);
                AppData.Settings.DispersionPlotSettings.IsVertical = value;
            }
        }

        private bool isVerticalPlane;

        public bool IsVerticalPlane
        {
            get => isVerticalPlane;
            set => this.RaiseAndSetIfChanged(ref isVerticalPlane, value);
        }

        private bool isHorizontalPlane = true;

        public bool IsHorizontalPlane
        {
            get => isHorizontalPlane;
            set => this.RaiseAndSetIfChanged(ref isHorizontalPlane, value);
        }

        private bool isRealPlane;

        public bool IsRealPlane
        {
            get => isRealPlane;
            set => this.RaiseAndSetIfChanged(ref isRealPlane, value);
        }

        public Interaction<ShipSelectionWindowViewModel, List<ShipSummary>?> AddShipInteraction { get; }

        public Interaction<ValueSelectionViewModel, string?> ShellSelectionInteraction { get; } = new();

        /// <summary>
        /// Add a ship to the ones currently visualized.
        /// </summary>
        public async void AddShip()
        {
            // Open the ship selection window to let the user select a ship
            List<ShipSummary?>? resultList = (await AddShipInteraction.Handle(new(true)))!;

            if (resultList is not { Count: > 0 })
            {
                return;
            }

            foreach (var result in resultList)
            {
                var ship = result != null ? DesktopAppDataService.Instance.GetShipFromSummary(result, false) : null;
                if (ship == null)
                {
                    continue;
                }

                string shipName = Localizer.Instance[$"{ship.Index}_FULL"].Localization;
                logger.Info("Trying to add ship: {0} - {1}", ship.Index, shipName);

                // Check if the ship actually has main guns
                if (ship.MainBatteryModuleList is { Count: > 0 })
                {
                    logger.Info("Found guns on ship, asking for shell.");

                    // Get all the shell of that ship, and propose the choice to the user.
                    List<string> shellsName = ship.MainBatteryModuleList.SelectMany(x => x.Value.Guns.SelectMany(gun => gun.AmmoList)).Distinct().ToList();
                    string? shellIndex;
                    if (shellsName.Count == 1)
                    {
                        shellIndex = shellsName.First();
                    }
                    else
                    {
                        var vm = new ValueSelectionViewModel(shipName + " " + Translation.DispersionGraphWindow_SelectShellDesc, Translation.DispersionGraphWindow_SelectShell, shellsName);
                        shellIndex = await ShellSelectionInteraction.Handle(vm);
                    }

                    // If the user didn't select a shell, return and do nothing.
                    if (shellIndex is null)
                    {
                        return;
                    }

                    logger.Info("Shell selected: {0}", shellIndex);

                    // Get the gun with the corresponding shell. This is needed for stuff like Mogami, that change dispersion pattern based on the caliber
                    var guns = ship.MainBatteryModuleList.Select(x => x.Value).First(x => x.Guns.First().AmmoList.Contains(shellIndex));

                    // calculate the name that will be shown for this particular series
                    string shellName = Localizer.Instance[$"{shellIndex}"].Localization;

                    var name = $"{shipName} - {shellName}";

                    logger.Info("Trying to add series with name: {0}", name);

                    // check if we are adding a duplicate
                    if (!shipNames.Contains(name))
                    {
                        logger.Info("Ship is not a duplicate, start creating series.");

                        // Create and add the dispersion series
                        var hSeries = CreateHorizontalDispersionSeries(guns.DispersionValues, (double)guns.MaxRange, name);
                        var vSeries = CreateVerticalDispersionSeries(guns.DispersionValues, (double)guns.MaxRange, name);
                        HorizontalModel!.Series.Add(hSeries);
                        VerticalModel!.Series.Add(vSeries);

                        // create and add the ballistic series
                        var shell = DesktopAppDataService.Instance.GetProjectile<ArtilleryShell>(shellIndex);

                        var ballisticSeries = CreateBallisticSeries(shell, (double)guns.MaxRange, name);

                        // create and add the dispersion plot
                        if (refreshNeeded)
                        {
                            RefreshPlot();
                        }

                        DispersionPlotList.LastOrDefault()?.UpdateIsLast(false);
                        var newPlot = DispersionPlotHelper.CalculateDispersionPlotParameters(name, guns.DispersionValues, shell, (double)guns.MaxRange, AimingRange * 1000, (double)guns.Sigma, ShotsNumber);
                        DispersionPlotList.Add(new(newPlot));

                        PenetrationModel!.Series.Add(ballisticSeries.Penetration);
                        FlightTimeModel!.Series.Add(ballisticSeries.FlightTime);
                        ImpactVelocityModel!.Series.Add(ballisticSeries.ImpactVelocity);
                        ImpactAngleModel!.Series.Add(ballisticSeries.ImpactAngle);

                        // Add the new ship to the list
                        shipNames.Add(name);

                        // Invalidate all plots to update them
                        HorizontalModel!.InvalidatePlot(true);
                        VerticalModel!.InvalidatePlot(true);
                        PenetrationModel!.InvalidatePlot(true);
                        FlightTimeModel!.InvalidatePlot(true);
                        ImpactVelocityModel!.InvalidatePlot(true);
                        ImpactAngleModel!.InvalidatePlot(true);

                        this.RaisePropertyChanged(nameof(ShipNames));
                    }
                    else
                    {
                        logger.Warn($"{shipName} has already been added!");
                        await MessageBox.Show(self, shipName + " " + Translation.MessageBox_DuplicateShip, Translation.MessageBox_Error, MessageBox.MessageBoxButtons.Ok, MessageBox.MessageBoxIcon.Error);
                    }
                }
                else
                {
                    logger.Warn($"{shipName} has no guns!");
                    await MessageBox.Show(self, shipName + " " + Translation.MessageBox_ShipNoGun, Translation.MessageBox_Error, MessageBox.MessageBoxButtons.Ok, MessageBox.MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Remove ships from the ones currently visualized.
        /// </summary>
        public async void RemoveShip()
        {
            logger.Info("Trying to remove ship from series");

            List<string> result = await DispersionShipRemovalDialog.ShowShipRemoval(self!, shipNames.ToList());
            if (result.Count <= 0)
            {
                return;
            }

            foreach (string ship in result)
            {
                int index = shipNames.IndexOf(ship);
                logger.Info("Found {0} at index {1}", ship, index);

                HorizontalModel!.Series.RemoveAt(index);
                VerticalModel!.Series.RemoveAt(index);
                PenetrationModel!.Series.RemoveAt(index);
                FlightTimeModel!.Series.RemoveAt(index);
                ImpactAngleModel!.Series.RemoveAt(index);
                ImpactVelocityModel!.Series.RemoveAt(index);
                DispersionPlotList.RemoveAt(index);
                if (DispersionPlotList.Count > 0)
                {
                    DispersionPlotList.Last().UpdateIsLast(true);
                }

                shipNames.RemoveAt(index);
            }

            HorizontalModel!.InvalidatePlot(true);
            VerticalModel!.InvalidatePlot(true);
            PenetrationModel!.InvalidatePlot(true);
            FlightTimeModel!.InvalidatePlot(true);
            ImpactVelocityModel!.InvalidatePlot(true);
            ImpactAngleModel!.InvalidatePlot(true);
            this.RaisePropertyChanged(nameof(ShipNames));
        }

        public void RefreshPlot()
        {
            refreshNeeded = false;
            if (DispersionPlotList.Count == 0)
            {
                return;
            }

            AppData.Settings.DispersionPlotSettings.AimingRange = AimingRange;
            AppData.Settings.DispersionPlotSettings.ShotsNumber = ShotsNumber;

            foreach (var itemViewModel in DispersionPlotList)
            {
                var dispersionPlot = itemViewModel.DispersionEllipse;
                var newPlot = DispersionPlotHelper.CalculateDispersionPlotParameters(dispersionPlot.Name, dispersionPlot.DispersionData, dispersionPlot.Shell, dispersionPlot.MaxRange, AimingRange * 1000, dispersionPlot.Sigma, ShotsNumber);
                itemViewModel.DispersionEllipse = newPlot;
                itemViewModel.IsLast = false;
            }

            DispersionPlotList.LastOrDefault()?.UpdateIsLast(true);
        }

        [DependsOn(nameof(ShipNames))]
        public bool CanRemoveShip(object parameter) => shipNames.Count > 0;

        /// <summary>
        /// Initialize the model for the dispersions plot with common settings.
        /// </summary>
        /// <param name="name">Title of the plot and of the Y axis.</param>
        /// <returns>The dispersion model.</returns>
        private PlotModel InitializeDispersionBaseModel(string name)
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
                Title = Translation.ShipStats_Range,
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
                Title = name,
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
            model.DefaultColors = GenerateColors();

            return model;
        }

        /// <summary>
        /// Initialize the model fo the ballistic plot with common settings.
        /// </summary>
        /// <param name="name">Title of the plot and of the Y axis.</param>
        /// <param name="yUnit">Unit of the Y axis.</param>
        /// <param name="legendPosition">Position of the legend inside the graph.</param>
        /// <param name="yMaximumMargin">Max margin value for y.</param>
        /// <returns>The ballistic model.</returns>
        private PlotModel InitializeBallisticBaseModel(string name, string yUnit, LegendPosition legendPosition, double yMaximumMargin)
        {
            var foreground = ConvertColorFromResource("ThemeForegroundColor");
            var foregroundLow = ConvertColorFromResource("ThemeForegroundLowColor");
            var background = ConvertColorFromResource("ThemeBackgroundColor");

            PlotModel model = new()
            {
                Title = name,
                TextColor = foreground,
                PlotAreaBorderColor = foreground,
                LegendPosition = legendPosition,
                LegendBorder = foreground,
                LegendBorderThickness = 1,
                LegendBackground = background,
                LegendFontSize = 13,
            };

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = Translation.ShipStats_Range,
                IsPanEnabled = false,
                Unit = "Km",
                Minimum = 0,
                AxislineColor = foreground,
                TextColor = foreground,
                TicklineColor = foreground,
                MajorGridlineThickness = 1,
                MajorGridlineStyle = LineStyle.Dash,
                MajorGridlineColor = foregroundLow,
                FormatAsFractions = true,
                FractionUnit = 1000,
            };

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = name,
                IsPanEnabled = false,
                Unit = yUnit,
                Minimum = 0,
                AxislineColor = foreground,
                TextColor = foreground,
                TicklineColor = foreground,
                MajorGridlineThickness = 1,
                MajorGridlineStyle = LineStyle.Dash,
                MajorGridlineColor = foregroundLow,
                MaximumPadding = yMaximumMargin,
            };
            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);
            model.DefaultColors = GenerateColors();

            return model;
        }

        /// <summary>
        /// Create the series for the horizontal dispersion.
        /// </summary>
        /// <param name="dispersion"><see cref="Dispersion"/> data of the gun.</param>
        /// <param name="maxRange">Max range of the gun.</param>
        /// <param name="name">Name for the series.</param>
        /// <returns>The horizontal dispersion series for the given parameter.</returns>
        private FunctionSeries CreateHorizontalDispersionSeries(Dispersion dispersion, double maxRange, string name)
        {
            var dispSeries = new FunctionSeries(range => dispersion.CalculateHorizontalDispersion(range * 1000), 0, (maxRange * 1.5) / 1000, 0.01, name)
            {
                TrackerFormatString = "{0}\n{1}: {2:#.00} Km\n{3}: {4:#.00} m",
                StrokeThickness = 4,
            };
            return dispSeries;
        }

        /// <summary>
        /// Create the series for the vertical dispersion.
        /// </summary>
        /// <param name="dispersion"><see cref="Dispersion"/> data of the gun.</param>
        /// <param name="maxRange">Max range of the gun.</param>
        /// <param name="name">Name for the series.</param>
        /// <returns>The vertical dispersion series for the given parameter.</returns>
        private FunctionSeries CreateVerticalDispersionSeries(Dispersion dispersion, double maxRange, string name)
        {
            var dispSeries = new FunctionSeries(range => dispersion.CalculateVerticalDispersion(maxRange, range * 1000), 0, (maxRange * 1.5) / 1000, 0.01, name)
            {
                TrackerFormatString = "{0}\n{1}: {2:#.00} Km\n{3}: {4:#.00} m",
                StrokeThickness = 4,
            };
            return dispSeries;
        }

        /// <summary>
        /// Create the ballistic series for the shell.
        /// </summary>
        /// <param name="shell">The <see cref="ArtilleryShell"/> to calculate the ballistic of.</param>
        /// <param name="maxRange">Max range of the gun.</param>
        /// <param name="name">Name for the series.</param>
        /// <returns>A tuple with series for Penetration, flight time, impact velocity and impact angle.</returns>
        private (LineSeries Penetration, LineSeries FlightTime, LineSeries ImpactVelocity, LineSeries ImpactAngle) CreateBallisticSeries(
            ArtilleryShell shell,
            double maxRange,
            string name)
        {
            Dictionary<double, Ballistic> ballistic = BallisticHelper.CalculateBallistic(shell, maxRange);

            IEnumerable<DataPoint> penData = ballistic.Select(x => new DataPoint(x.Key, x.Value.Penetration));
            var penSeries = new LineSeries
            {
                Title = name,
                ItemsSource = penData,
                TrackerFormatString = "{0}\n{1}: {2:0,.00} Km\n{3}: {4:#.00} mm",
                StrokeThickness = 4,
            };

            IEnumerable<DataPoint> flightTimeData = ballistic.Select(x => new DataPoint(x.Key, x.Value.FlightTime));
            var flightTimeSeries = new LineSeries
            {
                Title = name,
                ItemsSource = flightTimeData,
                TrackerFormatString = "{0}\n{1}: {2:0,.00} Km\n{3}: {4:#.00} s",
                StrokeThickness = 4,
            };

            IEnumerable<DataPoint> impactVelocityData = ballistic.Select(x => new DataPoint(x.Key, x.Value.Velocity));
            var impactVelocitySeries = new LineSeries
            {
                Title = name,
                ItemsSource = impactVelocityData,
                TrackerFormatString = "{0}\n{1}: {2:0,.00} Km\n{3}: {4:#.00} m/s",
                StrokeThickness = 4,
            };

            IEnumerable<DataPoint> impactAngleData = ballistic.Select(x => new DataPoint(x.Key, x.Value.ImpactAngle));
            var impactAngleSeries = new LineSeries
            {
                Title = name,
                ItemsSource = impactAngleData,
                TrackerFormatString = "{0}\n{1}: {2:0,.00} Km\n{3}: {4:#.00}°",
                StrokeThickness = 4,
            };

            return (penSeries, flightTimeSeries, impactVelocitySeries, impactAngleSeries);
        }

        /// <summary>
        /// Get color from resourceKey and convert it into an <see cref="OxyColor"/>.
        /// </summary>
        /// <param name="resourceKey">Resource Key of the color to convert.</param>
        /// <returns>The corresponding <see cref="OxyColor"/>.</returns>
        private OxyColor ConvertColorFromResource(string resourceKey)
        {
            var color = self!.FindResource(resourceKey) as Color?;
            return OxyColor.FromUInt32(color!.Value.ToUint32());
        }

        private List<OxyColor> GenerateColors()
        {
            OxyColor[] colors =
            {
                OxyColor.Parse("#288753"),
                OxyColor.Parse("#ef6fcc"),
                OxyColor.Parse("#62ce75"),
                OxyColor.Parse("#f53a4c"),
                OxyColor.Parse("#11ccdc"),
                OxyColor.Parse("#9166aa"),
                OxyColor.Parse("#a4c28a"),
                OxyColor.Parse("#c15734"),
                OxyColor.Parse("#faa566"),
                OxyColor.Parse("#6c7b66"),
                OxyColor.Parse("#eda4ba"),
                OxyColor.Parse("#2d6df9"),
                OxyColor.Parse("#f62ef3"),
                OxyColor.Parse("#957206"),
                OxyColor.Parse("#a45dff"),
            };
            return colors.ToList();
        }
    }
}

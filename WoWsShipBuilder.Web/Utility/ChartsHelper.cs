using ChartJs.Blazor.Common;
using ChartJs.Blazor.Common.Axes;
using ChartJs.Blazor.Common.Enums;
using ChartJs.Blazor.LineChart;
using ChartJs.Blazor.Util;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Translations;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Web.Utility;

public static class ChartsHelper
{
    public static LineConfig SetChartConfig(ILocalizer localizer, string title, string yLabel, double range, bool isShellTrajectoryChart = false)
    {
        var xLabel = $"{localizer.GetAppLocalization(nameof(Translation.ShipStats_Range)).Localization} [{localizer.GetAppLocalization(nameof(Translation.Unit_KM)).Localization}]";
        string? foregroundColor = ColorUtil.FromDrawingColor(System.Drawing.Color.DarkGray);
        string? gridColor = ColorUtil.FromDrawingColor(System.Drawing.Color.DimGray);
        var gridDash = new double[] {5, 5};
        return new()
        {
            Options = new()
            {
                Responsive = true,
                SpanGaps = true,
                Title = new()
                {
                    Display = true,
                    Text = title,
                    FontColor = foregroundColor,
                    FontSize = 30,
                },
                Tooltips = new()
                {
                    Mode = InteractionMode.Nearest,
                    Intersect = true,
                    DisplayColors = true,
                },
                Hover = new()
                {
                    Mode = InteractionMode.Nearest,
                    Intersect = true,
                },
                Legend = new()
                {
                    Labels = new()
                    {
                        FontColor = foregroundColor,
                    },
                },
                Scales = new()
                {
                    XAxes = new List<CartesianAxis>
                    {
                        new LinearCartesianAxis
                        {
                            Ticks = new()
                            {
                                Display = true,
                                FontColor = foregroundColor,
                                MaxTicksLimit = 10,
                                BeginAtZero = true,
                            },
                            ScaleLabel = new()
                            {
                                LabelString = xLabel,
                                FontColor = foregroundColor,
                                Display = true,
                                FontSize = 15,
                            },
                            GridLines = new()
                            {
                                ZeroLineColor = foregroundColor,
                                Display = true,
                                DrawTicks = true,
                                TickMarkLength = 5,
                                DrawBorder = true,
                                ZeroLineWidth = 3,
                                DrawOnChartArea = true,
                                Color = gridColor,
                                BorderDash = gridDash,
                            },

                        },
                    },
                    YAxes = new List<CartesianAxis>
                    {
                        new LinearCartesianAxis
                        {
                            Ticks = new()
                            {
                                Display = true,
                                FontColor = foregroundColor,
                                MaxTicksLimit = 10,
                                BeginAtZero = true,
                                SuggestedMax = isShellTrajectoryChart ? (int)(range / 4 * 1000) : 0,
                            },
                            ScaleLabel = new()
                            {
                                LabelString = yLabel,
                                FontColor = foregroundColor,
                                Display = true,
                                FontSize = 15,
                            },
                            GridLines = new()
                            {
                                ZeroLineColor = foregroundColor,
                                Display = true,
                                DrawTicks = true,
                                TickMarkLength = 5,
                                DrawBorder = true,
                                ZeroLineWidth = 3,
                                DrawOnChartArea = true,
                                Color = gridColor,
                                BorderDash = gridDash,
                            },
                        },
                    },
                },
            },
        };
    }

    /// <summary>
    /// Create the series for the horizontal dispersion.
    /// </summary>
    /// <param name="dispersion"><see cref="Dispersion"/> data of the gun.</param>
    /// <param name="maxRange">Max range of the gun.</param>
    /// <returns>The horizontal dispersion series for the given parameter.</returns>
    public static IEnumerable<Point> CreateHorizontalDispersionSeries(Dispersion dispersion, double maxRange)
    {
        return CreateFunctionSeries(x => dispersion.CalculateHorizontalDispersion(x * 1000), 0, (maxRange * 1.5) / 1000, 0.1);
    }

    /// <summary>
    /// Create the series for the vertical dispersion.
    /// </summary>
    /// <param name="dispersion"><see cref="Dispersion"/> data of the gun.</param>
    /// <param name="maxRange">Max range of the gun.</param>
    /// <param name="impactAngles">Dictionary containing the impact angle for each range.</param>
    /// <returns>The vertical dispersion series for the given parameter.</returns>
    public static (IEnumerable<Point> vertDispAtImpactAngle, IEnumerable<Point> vertDispOnWater, IEnumerable<Point> vertDispOnPerpendicularToWater) CreateVerticalDispersionSeries(Dispersion dispersion, double maxRange, Dictionary<double, Ballistic> impactAngles)
    {
        List<Point> series = CreateFunctionSeries(x => dispersion.CalculateVerticalDispersion(maxRange, x * 1000), 0, (maxRange * 1.5) / 1000, 0.1).ToList();

        List<Point> vertDispOnWater = new();
        List<Point> vertDispOnPerpendicularToWater = new();

        foreach ((double range, var data) in impactAngles)
        {
            double disp = dispersion.CalculateVerticalDispersion(maxRange, range);
            vertDispOnWater.Add(new(range / 1000, disp / Math.Sin(Math.PI / 180 * data.ImpactAngle)));
            vertDispOnPerpendicularToWater.Add(new(range / 1000, disp / Math.Cos(Math.PI / 180 * data.ImpactAngle)));
        }

        return (series, vertDispOnWater, vertDispOnPerpendicularToWater);
    }

    /// <summary>
    /// Create data series using a function <code>f(x)</code>.
    /// </summary>
    /// <param name="f">The function <code>f(x)</code>.</param>
    /// <param name="x0">The start x value.</param>
    /// <param name="x1">The end x value.</param>
    /// <param name="dx">The increment in x.</param>
    /// <returns>A <see cref="List{Point}"/> of <see cref="Point"/></returns>
    public static IEnumerable<Point> CreateFunctionSeries(Func<double, double> f, double x0, double x1, double dx)
    {
        List<Point> dispSeries = new();
        for (double x = x0; x <= x1 + dx * 0.5; x += dx)
            dispSeries.Add(new(x, f(x)));
        return dispSeries;
    }

    public static System.Drawing.Color[] GenerateColors()
    {
        var colors = new[]
        {
            System.Drawing.ColorTranslator.FromHtml("#ef6fcc"),
            System.Drawing.ColorTranslator.FromHtml("#62ce75"),
            System.Drawing.ColorTranslator.FromHtml("#f53a4c"),
            System.Drawing.ColorTranslator.FromHtml("#11ccdc"),
            System.Drawing.ColorTranslator.FromHtml("#9166aa"),
            System.Drawing.ColorTranslator.FromHtml("#a4c28a"),
            System.Drawing.ColorTranslator.FromHtml("#c15734"),
            System.Drawing.ColorTranslator.FromHtml("#faa566"),
            System.Drawing.ColorTranslator.FromHtml("#6c7b66"),
            System.Drawing.ColorTranslator.FromHtml("#eda4ba"),
            System.Drawing.ColorTranslator.FromHtml("#2d6df9"),
            System.Drawing.ColorTranslator.FromHtml("#f62ef3"),
            System.Drawing.ColorTranslator.FromHtml("#957206"),
            System.Drawing.ColorTranslator.FromHtml("#a45dff"),
        };
        return colors;
    }

    public enum EllipsePlanes
    {
        RealPlane,
        HorizontalPlane,
        VerticalPlane,
    }

    public enum BallisticParameter
    {
        Penetration,
        FlightTime,
        ImpactVelocity,
        ImpactAngle,
    }

    public enum ChartDatasets
    {
        HorizontalDispersion,
        VerticalDispersion,
        Penetration,
        FlightTime,
        ImpactVelocity,
        ImpactAngle,
        Trajectory,
    }
}

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using DynamicData;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Features.BallisticCharts.Data;

namespace WoWsShipBuilder.Features.BallisticCharts;

public static class ChartsHelper
{
    /// <summary>
    /// Create the series for the horizontal dispersion.
    /// </summary>
    /// <param name="dispersion"><see cref="Dispersion"/> data of the gun.</param>
    /// <param name="maxRange">Max range of the gun.</param>
    /// <param name="modifier">The dispersion modifier.</param>
    /// <returns>The horizontal dispersion series for the given parameter.</returns>
    public static IEnumerable<Point> CreateHorizontalDispersionChartDataset(Dispersion dispersion, double maxRange, double modifier)
    {
        return CreateFunctionSeries(x => dispersion.CalculateHorizontalDispersion(x * 1000, modifier), 0, (maxRange * 1.5) / 1000, 0.1);
    }

    /// <summary>
    /// Create the series for the vertical dispersion.
    /// </summary>
    /// <param name="dispersion"><see cref="Dispersion"/> data of the gun.</param>
    /// <param name="maxRange">Max range of the gun.</param>
    /// <param name="impactAngles">Dictionary containing the impact angle for each range.</param>
    /// <param name="modifier">The dispersion modifier.</param>
    /// <returns>The vertical dispersion series for the given parameter.</returns>
    public static VerticalDispersions CreateVerticalDispersionSeries(Dispersion dispersion, double maxRange, Dictionary<double, Ballistic> impactAngles, double modifier)
    {
        List<Point> series = CreateFunctionSeries(x => dispersion.CalculateDispersion(maxRange, modifier, x * 1000).Vertical, 0, (maxRange * 1.5) / 1000, 0.1).ToList();

        List<Point> vertDispOnWater = new();
        List<Point> vertDispOnPerpendicularToWater = new();

        foreach ((double range, var data) in impactAngles)
        {
            double disp = dispersion.CalculateDispersion(maxRange, modifier, range).Vertical;
            vertDispOnWater.Add(new(range / 1000, disp / Math.Sin(Math.PI / 180 * data.ImpactAngle)));
            vertDispOnPerpendicularToWater.Add(new(range / 1000, disp / Math.Cos(Math.PI / 180 * data.ImpactAngle)));
        }

        vertDispOnWater.RemoveAt(0);
        return new(series, vertDispOnWater, vertDispOnPerpendicularToWater);
    }

    /// <summary>
    /// Create data series using a function f(x).
    /// </summary>
    /// <param name="f">The function f(x).</param>
    /// <param name="x0">The start x value.</param>
    /// <param name="x1">The end x value.</param>
    /// <param name="dx">The increment in x.</param>
    /// <returns>A <see cref="List{Point}"/> of <see cref="Point"/>.</returns>
    private static IEnumerable<Point> CreateFunctionSeries(Func<double, double> f, double x0, double x1, double dx)
    {
        List<Point> dispSeries = new();
        for (double x = x0; x <= x1 + (dx * 0.5); x += dx)
        {
            dispSeries.Add(new(x, f(x)));
        }

        return dispSeries;
    }

    public static IEnumerable<Point> SelectVerticalDispersionDataset(VerticalDispersions vertDispSeries, EllipsePlanes selectedVertDispPlane)
    {
        IEnumerable<Point> verticalDispSeries;
        switch (selectedVertDispPlane)
        {
            case EllipsePlanes.HorizontalPlane:
                verticalDispSeries = vertDispSeries.VerticalDispersionOnWater;
                break;
            case EllipsePlanes.VerticalPlane:
                verticalDispSeries = vertDispSeries.VerticalDispersionOnPerpendicularToWater;
                break;
            case EllipsePlanes.RealPlane:
                verticalDispSeries = vertDispSeries.VerticalDispersionAtImpactAngle;
                break;
            default:
                verticalDispSeries = vertDispSeries.VerticalDispersionAtImpactAngle;
                break;
        }

        return verticalDispSeries;
    }

    public static IEnumerable<Point> CreateTrajectoryDataset(Dictionary<double, Ballistic> ballisticSeries, double range)
    {
        var validData = new List<KeyValuePair<double, Ballistic>>
        {
            ballisticSeries.Where(x => x.Key / 1000 > range),
        };
        if (validData.Count == 0)
        {
            validData.Add(ballisticSeries.Last());
        }

        IEnumerable<Point> trajectory = validData[0].Value.Coordinates.Select(x => new Point(x.X / 1000, x.Y));
        return trajectory;
    }

    public static IEnumerable<Point> CreateBallisticChartDataset(Dictionary<double, Ballistic> data, BallisticParameter ballisticParameter)
    {
        IEnumerable<Point> pointsList = ballisticParameter switch
        {
            BallisticParameter.Penetration => data.Select(x => new Point(x.Key / 1000, x.Value.Penetration)),
            BallisticParameter.ImpactVelocity => data.Select(x => new Point(x.Key / 1000, x.Value.Velocity)),
            BallisticParameter.FlightTime => data.Select(x => new Point(x.Key / 1000, x.Value.FlightTime)),
            BallisticParameter.ImpactAngle => data.Select(x => new Point(x.Key / 1000, x.Value.ImpactAngle)),
            _ => throw new InvalidEnumArgumentException(),
        };

        return pointsList;
    }

    public static string GetPlaneIcon(EllipsePlanes selectedPlane)
    {
        var impactAnglePath = "<path style=\"stroke:#808080;stroke-width:1\" d=\"M 0 24 L 19 5 Z\"/>";
        if (selectedPlane == EllipsePlanes.RealPlane)
        {
            impactAnglePath = "<path style=\"stroke:#FF2E2E;stroke-width:1\" d=\"M 0 24 L 19 5 Z\"/>";
        }

        var horizontalPath = "<path style=\"stroke:#808080;stroke-width:2\" d=\"M 0 24 H 24 Z\"/>";
        if (selectedPlane == EllipsePlanes.HorizontalPlane)
        {
            horizontalPath = "<path style=\"stroke:#FF2E2E;stroke-width:2\" d=\"M 0 24 H 24 Z\"/>";
        }

        var verticalPath = "<path style=\"stroke:#808080;stroke-width:2\" d=\"M 0 24 V -24 Z\"/>";
        if (selectedPlane == EllipsePlanes.VerticalPlane)
        {
            verticalPath = "<path style=\"stroke:#FF2E2E;stroke-width:2\" d=\"M 0 24 V -24 Z\"/>";
        }

        return impactAnglePath + verticalPath + horizontalPath;
    }

    [SuppressMessage("Naming Rules", "SA1300", Justification = "Lowercase needed for chartjs to recognize to which axis the data belongs")]
    public sealed record Point(double x, double y);
}

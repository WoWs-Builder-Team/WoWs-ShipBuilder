using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Projectile;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Features.Charts;

public static class DispersionPlotHelper
{
    /// <summary>
    /// Calculates the radii of the dispersion ellipse.
    /// </summary>
    /// <param name="dispersionData">Contains the parameters to calculate horizontal and vertical dispersions.</param>
    /// <param name="maxRange">Max range a ship can fire at.</param>
    /// <param name="aimingRange">Range the ship is currently aiming at.</param>
    /// <param name="modifier">The current dispersion modifier.</param>
    /// <returns>The length of the horizontal and vertical radii of the dispersion ellipse.</returns>
    private static (double horizontalRadius, double verticalRadius) GetDispersionEllipse(Dispersion dispersionData, double maxRange, double aimingRange, double modifier)
    {
        (double horizontalRadius, double verticalRadius) = dispersionData.CalculateDispersion(maxRange, modifier, aimingRange);
        return (horizontalRadius, verticalRadius);
    }

    /// <summary>
    /// Calculates the projection ratio and the vertical radius length of the dispersion ellipse projected on water.
    /// </summary>
    /// <param name="shell">The shell to calculate the <see cref="Ballistic"/> of.</param>
    /// <param name="maxRange">Max range a ship can fire at.</param>
    /// <param name="aimingRange">Range the ship is currently aiming at.</param>
    /// <param name="verticalRadius">The length of the dispersion ellipse vertical radius.</param>
    /// <returns>The projection ratio on water and on the perpendicular plane to the water, the length of the vertical radius projected on water and on the perpendicular plane to the water.</returns>
    private static (double waterLineProjection, double perpendicularToWaterProjection, double projectedOnWaterVerticalRadius, double perpendicularToWaterVerticalRadius) GetProjectedEllipse(ArtilleryShell shell, double maxRange, double aimingRange, double verticalRadius)
    {
        double impactAngle;
        List<KeyValuePair<double, Ballistic>> ballistic = BallisticHelper.CalculateBallistic(shell, maxRange, shell.Penetration).Where(x => x.Key >= aimingRange).ToList();
        if (ballistic.Any())
        {
            impactAngle = ballistic.First().Value.ImpactAngle;
        }
        else
        {
            return (0, 0, 0, 0);
        }

        var waterLineProjection = Math.Sin(Math.PI / 180 * impactAngle);
        var perpendicularToWaterProjection = Math.Cos(Math.PI / 180 * impactAngle);
        var projectedOnWaterVerticalRadius = verticalRadius / waterLineProjection;
        var perpendicularToWaterVerticalRadius = verticalRadius / perpendicularToWaterProjection;

        return (waterLineProjection, perpendicularToWaterProjection, projectedOnWaterVerticalRadius, perpendicularToWaterVerticalRadius);
    }

    /// <summary>
    /// Calculates the ratio to identify the area where 50% of the shots lends on average.
    /// </summary>
    /// <param name="sigma">The sigma of the ship.</param>
    /// <returns>The ratio to identify the area where 50% of the shots lends on average.</returns>
    private static double GetHalfHitsRatio(double sigma)
    {
        double left = -sigma;
        double right = sigma;
        double z = MathHelper.Cdf(right) - MathHelper.Cdf(left);
        double halfRatio = MathHelper.InvCdf((0.25 * z) + MathHelper.Cdf(left)) / left;

        return halfRatio;
    }

    /// <summary>
    /// Calculates the coordinates of the shots hit points on the real plane, projected on water and projected on the perpendicular plane to the water.
    /// </summary>
    /// <param name="sigma">The sigma of the ship.</param>
    /// <param name="horizontalRadius">The horizontal radius of the dispersion ellipse.</param>
    /// <param name="verticalRadius">The vertical radius of the dispersion ellipse.</param>
    /// <param name="shotsNumber">The number of shots to simulate.</param>
    /// <param name="waterLineProjection">The projection ratio to project the hit points on the waterline.</param>
    /// <param name="perpendicularToWaterLineProjection">The projection ratio to project the hit points on the perpendicular plane to the waterline.</param>
    /// <returns>3 lists of hit points.</returns>
    private static (List<Point> RealPlane, List<Point> OnWaterLine, List<Point> PerpendicularToWaterLine) GetHitPoints(double sigma, double horizontalRadius, double verticalRadius, int shotsNumber, double waterLineProjection, double perpendicularToWaterLineProjection)
    {
        Random random = new();
        List<Point> realHitPoints = new();
        List<Point> onWaterHitPoints = new();
        List<Point> onVerticalHitPoints = new();
        for (int i = 0; i < shotsNumber; i++)
        {
            var randomRad = 2 * Math.PI * random.NextDouble();
            var randomLen = MathHelper.AdjustedGaussian(random, 0.0, 1 / sigma, -1, 1);
            var x = randomLen * (horizontalRadius * Math.Cos(randomRad));
            var y = randomLen * (verticalRadius * Math.Sin(randomRad));

            if (y > 0)
            {
                y = 300 * Math.Log((0.1 * y / 30) + 1.0);
            }

            realHitPoints.Add(new(x, y));
            onWaterHitPoints.Add(new(x, y / waterLineProjection));
            onVerticalHitPoints.Add(new(x, y / perpendicularToWaterLineProjection));
        }

        return (realHitPoints, onWaterHitPoints, onVerticalHitPoints);
    }

    /// <summary>
    /// Calls all the necessary methods to get all the data for the dispersion plot.
    /// </summary>
    /// <param name="name">The name of the represented entity.</param>
    /// <param name="dispersionData">Contains the parameters to calculate horizontal and vertical dispersions.</param>
    /// <param name="shell">The shell to calculate the <see cref="Ballistic"/> of.</param>
    /// <param name="maxRange">Max range a ship can fire at.</param>
    /// <param name="aimingRange">Range the ship is currently aiming at.</param>
    /// <param name="sigma">The sigma of the ship.</param>
    /// <param name="shotsNumber">The number of shots to simulate.</param>
    /// <param name="modifier">The dispersion modifier.</param>
    /// <returns>A DispersionEllipse object containing all the information.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1117:Parameters should be on same line or separate lines", Justification = "Way too long.")]
    public static DispersionEllipse CalculateDispersionPlotParameters(string name, Dispersion dispersionData, ArtilleryShell shell, double maxRange, double aimingRange, double sigma, int shotsNumber, double modifier)
    {
        (double horizontalRadius, double verticalRadius) = GetDispersionEllipse(dispersionData, maxRange, aimingRange, modifier);
        var projectedEllipse = GetProjectedEllipse(shell, maxRange, aimingRange, verticalRadius);
        if (projectedEllipse == (0, 0, 0, 0))
        {
            return new(name, dispersionData, shell, sigma, maxRange, modifier);
        }

        double halfRatio = GetHalfHitsRatio(sigma);
        (List<Point> realPlane, List<Point> onWaterLine, List<Point> perpendicularToWaterLine) = GetHitPoints(sigma, horizontalRadius, verticalRadius, shotsNumber, projectedEllipse.waterLineProjection, projectedEllipse.perpendicularToWaterProjection);

        return new(name, dispersionData, shell, sigma, maxRange, modifier, horizontalRadius, verticalRadius,
            projectedEllipse.projectedOnWaterVerticalRadius, projectedEllipse.perpendicularToWaterVerticalRadius,
            realPlane, onWaterLine, perpendicularToWaterLine,
            horizontalRadius * halfRatio, verticalRadius * halfRatio, projectedEllipse.projectedOnWaterVerticalRadius * halfRatio,
            projectedEllipse.perpendicularToWaterVerticalRadius * halfRatio);
    }

    public readonly record struct Point(double X, double Y);
}

public sealed record DispersionEllipse(string Label, Dispersion DispersionData, ArtilleryShell Shell, double Sigma, double MaxRange, double Modifier)
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1117:Parameters should be on same line or separate lines", Justification = "Way too long.")]
    public DispersionEllipse(string name, Dispersion dispersionData, ArtilleryShell shell, double sigma, double maxRange, double modifier,
        double horizontalRadius, double verticalRadius, double projectedOnWaterVerticalRadius, double projectedOnPerpendicularToWaterVerticalRadius,
        List<DispersionPlotHelper.Point> realHitPoints, List<DispersionPlotHelper.Point> onWaterHitPoints, List<DispersionPlotHelper.Point> onPerpendicularToWaterHitPoints,
        double horizontalRadiusHalfHitPoints, double verticalRadiusHalfHitPoints, double projectedOnWaterVerticalRadiusHalfHitPoints,
        double projectedOnPerpendicularToWaterVerticalRadiusHalfHitPoints)
        : this(name, dispersionData, shell, sigma, maxRange, modifier)
    {
        HorizontalRadius = horizontalRadius;
        VerticalRadius = verticalRadius;
        ProjectedOnWaterVerticalRadius = projectedOnWaterVerticalRadius;
        ProjectedOnPerpendicularToWaterVerticalRadius = projectedOnPerpendicularToWaterVerticalRadius;
        RealHitPoints = realHitPoints;
        OnWaterHitPoints = onWaterHitPoints;
        PerpendicularToWaterHitPoints = onPerpendicularToWaterHitPoints;
        HorizontalRadiusHalfHitPoints = horizontalRadiusHalfHitPoints;
        VerticalRadiusHalfHitPoints = verticalRadiusHalfHitPoints;
        ProjectedOnWaterVerticalRadiusHalfHitPoints = projectedOnWaterVerticalRadiusHalfHitPoints;
        ProjectedOnPerpendicularToWaterVerticalRadiusHalfHitPoints = projectedOnPerpendicularToWaterVerticalRadiusHalfHitPoints;
        IsValid = true;
    }

    public bool IsValid { get; }

    public double HorizontalRadius { get; }

    public double VerticalRadius { get; }

    public double ProjectedOnWaterVerticalRadius { get; }

    public double ProjectedOnPerpendicularToWaterVerticalRadius { get; }

    public List<DispersionPlotHelper.Point> RealHitPoints { get; } = new();

    public List<DispersionPlotHelper.Point> OnWaterHitPoints { get; } = new();

    public List<DispersionPlotHelper.Point> PerpendicularToWaterHitPoints { get; } = new();

    public double HorizontalRadiusHalfHitPoints { get; }

    public double VerticalRadiusHalfHitPoints { get; }

    public double ProjectedOnWaterVerticalRadiusHalfHitPoints { get; }

    public double ProjectedOnPerpendicularToWaterVerticalRadiusHalfHitPoints { get; }
}

using System;
using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public class DispersionPlotHelper
    {
        /// <summary>
        /// Calulates the radii of the dispersion ellipse.
        /// </summary>
        /// <param name="dispersionData">Contains the parameters to calculate horizontal and vertial dispersions.</param>
        /// <param name="maxRange">Max range a ship can fire at.</param>
        /// <param name="aimingRange">Range the ship is currently aiming at.</param>
        /// <returns>The length of the horizontal and vertical radii of the dispersion ellipse.</returns>
        private static (double horizontalRadius, double verticalRadius) GetDispersionEllipse(Dispersion dispersionData, double maxRange, double aimingRange)
        {
            var verticalRadius = dispersionData.CalculateVerticalDispersion(maxRange, aimingRange);
            var horizontalRadius = dispersionData.CalculateHorizontalDispersion(aimingRange);

            return (horizontalRadius, verticalRadius);
        }

        /// <summary>
        /// Calculates the projection ratio and the vertical radius length of the dispersion ellipse projected on water.
        /// </summary>
        /// <param name="shell">Contains all the shell parameters.</param>
        /// <param name="maxRange">Max range a ship can fire at.</param>
        /// <param name="aimingRange">Range the ship is currently aiming at.</param>
        /// <param name="verticalRadius">The length of the dispersion ellipse vertical radius.</param>
        /// <returns>The projection ratio and the length of the vertical radius projected on water.</returns>
        private static (double waterLineProjection, double projectedVerticalRadius) GetProjectedEllipse(ArtilleryShell shell, double maxRange, double aimingRange, double verticalRadius)
        {
            double impactAngle;
            try
            {
                impactAngle = BallisticHelper.CalculateBallistic(shell, maxRange).First(x => x.Key >= aimingRange).Value.ImpactAngle;
            }
            catch (InvalidOperationException e)
            {
                return (0, 0);
            }

            var waterLineProjection = Math.Sin(Math.PI / 180 * impactAngle);
            verticalRadius /= waterLineProjection;

            return (waterLineProjection, verticalRadius);
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
        /// Calculates the coordinates of the shots hit points projected on water.
        /// </summary>
        /// <param name="sigma">The sigma of the ship.</param>
        /// <param name="horizontalRadius">The horizontal radius of the dispersion ellipse.</param>
        /// <param name="verticalRadius">The vertical radius of the dispersion ellipse.</param>
        /// <param name="shotsNumber">The number of shots to simulate.</param>
        /// <param name="waterLineProjection">The projection ratio to project the hitpoints on the waterline.</param>
        /// <returns>A list of hit points.</returns>
        private static List<(double x, double y)> GetHitPoints(double sigma, double horizontalRadius, double verticalRadius, int shotsNumber, double waterLineProjection)
        {
            Random random = new();
            List<(double x, double y)> hitPoints = new();
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

                y /= waterLineProjection;

                hitPoints.Add((x, y));
            }
            
            return hitPoints;
        }

        /// <summary>
        /// Calls all the necessary methods to get all the data for the dispersion plot.
        /// </summary>
        /// <param name="name">The name of the represented entity.</param>
        /// <param name="dispersionData">Contains the parameters to calculate horizontal and vertial dispersions.</param>
        /// <param name="shell">Contains all the shell parameters.</param>
        /// <param name="maxRange">Max range a ship can fire at.</param>
        /// <param name="aimingRange">Range the ship is currently aiming at.</param>
        /// <param name="sigma">The sigma of the ship.</param>
        /// <param name="shotsNumber">The number of shots to simulate.</param>
        /// <returns>A DispersionEllipse object containing all the informations.</returns>
        public static DispersionEllipse CalculateDispersionPlotParameters(string name, Dispersion dispersionData, ArtilleryShell shell, double maxRange, double aimingRange, double sigma, int shotsNumber)
        {
            var ellipse = GetDispersionEllipse(dispersionData, maxRange, aimingRange);
            var projectedEllipse = GetProjectedEllipse(shell, maxRange, aimingRange, ellipse.verticalRadius);
            var halfRatio = GetHalfHitsRatio(sigma);
            var hitPointsList = GetHitPoints(sigma, ellipse.horizontalRadius, ellipse.verticalRadius, shotsNumber, projectedEllipse.waterLineProjection);

            return new DispersionEllipse(name, ellipse.horizontalRadius, ellipse.verticalRadius, projectedEllipse.projectedVerticalRadius, hitPointsList, ellipse.horizontalRadius * halfRatio, ellipse.verticalRadius * halfRatio, projectedEllipse.projectedVerticalRadius * halfRatio);
        }
    }

    public record DispersionEllipse
    {
        public DispersionEllipse(string name, double horizontalRadius, double verticalRadius, double projectedVerticalRadius, List<(double x, double y)> hitPoints, double horizontalRadiusHalfHitPoints, double verticalRadiusHalfHitPoints, double projectedVerticalRadiusHalfHitPoints)
        {
            Name = name;
            HorizontalRadius = horizontalRadius;
            VerticalRadius = verticalRadius;
            ProjectedVerticalRadius = projectedVerticalRadius;
            HitPoints = hitPoints;
            HorizontalRadiusHalfHitPoints = horizontalRadiusHalfHitPoints;
            VerticalRadiusHalfHitPoints = verticalRadiusHalfHitPoints;
            ProjectedVerticalRadiusHalfHitPoints = projectedVerticalRadiusHalfHitPoints;
        }

        public string Name { get; set; }

        public bool IsValid { get; set; } = true;

        public bool IsLast { get; set; } = true;

        public double HorizontalRadius { get; set; }

        public double VerticalRadius { get; set; }

        public double ProjectedVerticalRadius { get; set; }

        public List<(double x, double y)> HitPoints { get; set; }

        public double HorizontalRadiusHalfHitPoints { get; set; }

        public double VerticalRadiusHalfHitPoints { get; set; }

        public double ProjectedVerticalRadiusHalfHitPoints { get; set; }
    }
}

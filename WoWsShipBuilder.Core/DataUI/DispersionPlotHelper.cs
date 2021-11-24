using System;
using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public class DispersionPlotHelper
    {
        public static (double horizontalRadius, double verticalRadius) GetDispersionEllipse(Dispersion dispersionData, double maxRange, double aimingRange)
        {
            var verticalRadius = dispersionData.CalculateVerticalDispersion(maxRange, aimingRange);
            var horizontalRadius = dispersionData.CalculateHorizontalDispersion(aimingRange);

            return (horizontalRadius, verticalRadius);
        }

        public static (double waterLineProjection, double projectedVerticalRadius) GetProjectedEllipse(ArtilleryShell shell, double maxRange, double aimingRange, double verticalRadius)
        {
            var impactAngle = BallisticHelper.CalculateBallistic(shell, maxRange).First(x => x.Key >= aimingRange).Value.ImpactAngle;
            var waterLineProjection = Math.Sin(Math.PI / 180 * impactAngle);
            verticalRadius /= waterLineProjection;

            return (waterLineProjection, verticalRadius);
        }

        public static double GetHalfHitsRatio(double sigma)
        {
            double left = -sigma;
            double right = sigma;
            double z = MathHelper.Cdf(right) - MathHelper.Cdf(left);
            double halfRatio = MathHelper.InvCdf((0.25 * z) + MathHelper.Cdf(left)) / left;

            return halfRatio;
        }

        public static List<(double x, double y)> GetHitPoints(double sigma, double horizontalRadius, double verticalRadius, int shotsNumber, double waterLineProjection)
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

        public static DispersionEllipse CalculateDispersionPlotParameters(Dispersion dispersionData, ArtilleryShell shell, double maxRange, double aimingRange, double sigma, int shotsNumber)
        {
            var ellipse = GetDispersionEllipse(dispersionData, maxRange, aimingRange);
            var projectedEllipse = GetProjectedEllipse(shell, maxRange, aimingRange, ellipse.verticalRadius);
            var halfRatio = GetHalfHitsRatio(sigma);
            var hitPointsList = GetHitPoints(sigma, ellipse.horizontalRadius, ellipse.verticalRadius, shotsNumber, projectedEllipse.waterLineProjection);

            return new DispersionEllipse(ellipse.horizontalRadius, ellipse.verticalRadius, projectedEllipse.projectedVerticalRadius, hitPointsList, ellipse.horizontalRadius * halfRatio, ellipse.verticalRadius * halfRatio, projectedEllipse.projectedVerticalRadius * halfRatio);
        }
    }

    public record DispersionEllipse
    {
        public DispersionEllipse(double horizontalRadius, double verticalRadius, double projectedVerticalRadius, List<(double x, double y)> hitPoints, double horizontalRadiusHalfHitPoints, double verticalRadiusHalfHitPoints, double projectedVerticalRadiusHalfHitPoints)
        {
            HorizontalRadius = horizontalRadius;
            VerticalRadius = verticalRadius;
            ProjectedVerticalRadius = projectedVerticalRadius;
            HitPoints = hitPoints;
            HorizontalRadiusHalfHitPoints = horizontalRadiusHalfHitPoints;
            VerticalRadiusHalfHitPoints = verticalRadiusHalfHitPoints;
            ProjectedVerticalRadiusHalfHitPoints = projectedVerticalRadiusHalfHitPoints;
        }

        public double HorizontalRadius { get; set; }

        public double VerticalRadius { get; set; }

        public double ProjectedVerticalRadius { get; set; }

        public List<(double x, double y)> HitPoints { get; set; }

        public double HorizontalRadiusHalfHitPoints { get; set; }

        public double VerticalRadiusHalfHitPoints { get; set; }

        public double ProjectedVerticalRadiusHalfHitPoints { get; set; }
    }
}

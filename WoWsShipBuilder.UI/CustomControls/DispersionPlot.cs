using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using DynamicData;
using WoWsShipBuilder.Core.DataUI;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.CustomControls
{
    public class DispersionPlot : TemplatedControl
    {
        /// <summary>
        /// Styled Property for the sigma.
        /// </summary>
        public static readonly StyledProperty<decimal> SigmaProperty =
            AvaloniaProperty.Register<DispersionPlot, decimal>(nameof(Sigma));

        /// <summary>
        /// Styled Property for the dispersion data.
        /// </summary>
        public static readonly StyledProperty<Dispersion> DispersionDataProperty =
            AvaloniaProperty.Register<DispersionPlot, Dispersion>(nameof(DispersionData));

        /// <summary>
        /// Styled Property for the aiming rage.
        /// </summary>
        public static readonly StyledProperty<decimal> AimingRangeProperty =
            AvaloniaProperty.Register<DispersionPlot, decimal>(nameof(AimingRange));

        /// <summary>
        /// Styled Property for the number of shots.
        /// </summary>
        public static readonly StyledProperty<decimal> ShotsNumberProperty =
            AvaloniaProperty.Register<DispersionPlot, decimal>(nameof(ShotsNumber), 180);

        /// <summary>
        /// Styled Property for the max range of the ship.
        /// </summary>
        public static readonly StyledProperty<decimal> MaxRangeProperty =
            AvaloniaProperty.Register<DispersionPlot, decimal>(nameof(MaxRange));

        /// <summary>
        /// Styled Property for the shell.
        /// </summary>
        public static readonly StyledProperty<ArtilleryShell> ShellProperty =
            AvaloniaProperty.Register<DispersionPlot, ArtilleryShell>(nameof(Shell));

        /// <summary>
        /// Gets or sets the sigma.
        /// </summary>
        public decimal Sigma
        {
            get => GetValue(SigmaProperty);
            set => SetValue(SigmaProperty, value);
        }

        /// <summary>
        /// Gets or sets the dispersion data.
        /// </summary>
        public Dispersion DispersionData
        {
            get => GetValue(DispersionDataProperty);
            set => SetValue(DispersionDataProperty, value);
        }

        /// <summary>
        /// Gets or sets the aiming range.
        /// </summary>
        public decimal AimingRange
        {
            get => GetValue(AimingRangeProperty);
            set => SetValue(AimingRangeProperty, value);
        }

        /// <summary>
        /// Gets or sets the number of shots to simulate.
        /// </summary>
        public decimal ShotsNumber
        {
            get => GetValue(ShotsNumberProperty);
            set => SetValue(ShotsNumberProperty, value);
        }

        /// <summary>
        /// Gets or sets the max range of the ship.
        /// </summary>
        public decimal MaxRange
        {
            get => GetValue(MaxRangeProperty);
            set => SetValue(MaxRangeProperty, value);
        }

        /// <summary>
        /// Gets or sets the shell.
        /// </summary>
        public ArtilleryShell Shell
        {
            get => GetValue(ShellProperty);
            set => SetValue(ShellProperty, value);
        }

        /// <summary>
        /// Renders the content of the control, drawing the ellipse as well as the shots fired.
        /// For more details, see <see cref="Visual.Render"/>.
        /// </summary>
        /// <param name="context">The <see cref="DrawingContext"/> used to draw the visualization.</param>
        public override void Render(DrawingContext context)
        {
            var compressionFactor = 1.5;

            // vertical and horizontal radiuses have been inverted in order to rotate the final graph by 90°
            var horizontalRadius = DispersionData.CalculateVerticalDispersion((double)MaxRange, (double)AimingRange) / compressionFactor;
            var verticalRadius = DispersionData.CalculateHorizontalDispersion((double)AimingRange) / compressionFactor;
            var impactAngle = BallisticHelper.CalculateBallistic(Shell, (double)MaxRange).First(x => x.Key >= (double)AimingRange).Value.ImpactAngle;
            horizontalRadius /= Math.Sin(Math.PI / 180 * impactAngle);

            DrawHitPointsArea(horizontalRadius, verticalRadius, context);
            DrawHitPoints(context, horizontalRadius, verticalRadius);
            DrawHalfHitPointsArea((double)Sigma, horizontalRadius, verticalRadius, context);
            DrawAdditionalElements(context, horizontalRadius, verticalRadius);
        }

        private void DrawHitPointsArea(double horizontalRadius, double verticalRadius, DrawingContext context)
        {
            Point center = new(Bounds.Center.X - horizontalRadius, Bounds.Center.Y - verticalRadius);
            Rect ellipseRectangle = new(center, new Size(horizontalRadius * 2, verticalRadius * 2));
            EllipseGeometry dispersioEllipse = new(ellipseRectangle);
            context.DrawGeometry(Brushes.Transparent, new Pen(Foreground, 4), dispersioEllipse);
        }

        private void DrawHitPoints(DrawingContext context, double horizontalRadius, double verticalRadius)
        {
            var pointRadius = 4;
            var pointColor = Colors.Goldenrod;            
            var pointStyle = new Pen(Brushes.DarkGoldenrod, 1);
            var gradientEnd = new GradientStop(new Color(50, pointColor.R, pointColor.G, pointColor.B), 1);
            var filling = new RadialGradientBrush();
            filling.GradientStops.Add(gradientEnd);
            filling.GradientOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
            filling.Radius = pointRadius;
            var random = new Random();

            for (int i = 0; i < ShotsNumber; i++)
            {
                Point hitPoint = GetHitPoint(random, horizontalRadius, verticalRadius);
                Point center = new(Bounds.Center.X - hitPoint.X - pointRadius, Bounds.Center.Y - hitPoint.Y - pointRadius);
                Rect ellipseRectangle = new(center, new Size(pointRadius * 2, pointRadius * 2));
                EllipseGeometry dispersioEllipse = new(ellipseRectangle);
                context.DrawGeometry(filling, pointStyle, dispersioEllipse);
            }
        }

        private void DrawHalfHitPointsArea(double sigma, double horizontalRadius, double verticalRadius, DrawingContext context)
        {
            double left = -sigma;
            double right = sigma;
            double z = MathHelper.Cdf(right) - MathHelper.Cdf(left);
            double halfRatio = MathHelper.InvCdf((0.25 * z) + MathHelper.Cdf(left)) / left;

            horizontalRadius *= halfRatio;
            verticalRadius *= halfRatio;

            var filling = new SolidColorBrush(Colors.OrangeRed, 0.1);
            Point center = new(Bounds.Center.X - horizontalRadius, Bounds.Center.Y - verticalRadius);
            Rect ellipseRectangle = new(center, new Size(horizontalRadius * 2, verticalRadius * 2));
            EllipseGeometry dispersioEllipse = new(ellipseRectangle);
            context.DrawGeometry(filling, new Pen(Brushes.OrangeRed, 2), dispersioEllipse);
        }

        private Point GetHitPoint(Random random, double horizontalRadius, double verticalRadius)
        {
            var randomRad = 2 * Math.PI * random.NextDouble();
            var randomLen = MathHelper.AdjustedGaussian(random, 0.0, 1 / (double)Sigma, -1, 1);
            var x = randomLen * (horizontalRadius * Math.Cos(randomRad));
            var y = randomLen * (verticalRadius * Math.Sin(randomRad));

            if (y > 0)
            {
                y = 300 * Math.Log((0.1 * y / 30) + 1.0);
            }

            return new Point(x, y);
        }

        private void DrawAdditionalElements(DrawingContext context, double horizontalRadius, double verticalRadius)
        {
            context.DrawLine(new Pen(Brushes.White, 1), new Point(Bounds.Center.X - horizontalRadius, Bounds.Center.Y), new Point(Bounds.Center.X + horizontalRadius, Bounds.Center.Y));
            context.DrawLine(new Pen(Brushes.White, 1), new Point(Bounds.Center.X, Bounds.Center.Y - verticalRadius), new Point(Bounds.Center.X, Bounds.Center.Y + verticalRadius));
        }
    }
}

using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using WoWsShipBuilder.Core.DataUI;
using WoWsShipBuilder.Core.DataUI.UnitTranslations;
using WoWsShipBuilder.UI.Extensions;
using WoWsShipBuilder.UI.Translations;

namespace WoWsShipBuilder.UI.CustomControls
{
    public class DispersionPlot : TemplatedControl
    {
        /// <summary>
        /// Styled Property for the dispersion plot parameters.
        /// </summary>
        public static readonly StyledProperty<DispersionEllipse> DispersionPlotParametersProperty =
            AvaloniaProperty.Register<DispersionPlot, DispersionEllipse>(nameof(DispersionPlotParameters));

        /// <summary>
        /// Styled Property for the dispersion plot scaling.
        /// </summary>
        public static readonly StyledProperty<double> PlotScalingProperty =
            AvaloniaProperty.Register<DispersionPlot, double>(nameof(PlotScaling), 1);

        static DispersionPlot()
        {
            DispersionPlotParametersProperty.Changed.AddClassHandler<DispersionPlot>((x, e) => x.InvalidateVisual());
        }

        /// <summary>
        /// Gets or sets the dispersion plot parameters.
        /// </summary>
        public DispersionEllipse DispersionPlotParameters
        {
            get => GetValue(DispersionPlotParametersProperty);
            set => SetValue(DispersionPlotParametersProperty, value);
        }

        /// <summary>
        /// Gets or sets the dispersion plot scaling.
        /// </summary>
        public double PlotScaling
        {
            get => GetValue(PlotScalingProperty);
            set => SetValue(PlotScalingProperty, value);
        }

        /// <summary>
        /// Renders the content of the control, drawing the ellipse as well as the shots fired.
        /// For more details, see <see cref="Visual.Render"/>.
        /// </summary>
        /// <param name="context">The <see cref="DrawingContext"/> used to draw the visualization.</param>
        public override void Render(DrawingContext context)
        {
            // DrawFusoReference(context);
            DrawHalfHitPointsArea(context);
            DrawAdditionalElements(context);
            DrawHitPointsArea(context);
            DrawHitPoints(context);
        }

        /// <summary>
        /// Draws the dispersion ellipse.
        /// </summary>
        /// <param name="context">The <see cref="DrawingContext"/> used to draw the visualization.</param>
        private void DrawHitPointsArea(DrawingContext context)
        {
            var xRadius = DispersionPlotParameters.ProjectedVerticalRadius * PlotScaling;
            var yRadius = DispersionPlotParameters.HorizontalRadius * PlotScaling;
            var filling = new SolidColorBrush(Colors.Gray, 0.1);
            Point center = new(Bounds.Center.X - xRadius, Bounds.Center.Y - yRadius);
            Rect ellipseRectangle = new(center, new Size(xRadius * 2, yRadius * 2));
            EllipseGeometry dispersionEllipse = new(ellipseRectangle);
            context.DrawGeometry(filling, new Pen(Foreground, 4), dispersionEllipse);
        }

        /// <summary>
        /// Draws shells hit points inside the dispersion ellipse.
        /// </summary>
        /// <param name="context">The <see cref="DrawingContext"/> used to draw the visualization.</param>
        private void DrawHitPoints(DrawingContext context)
        {
            var pointRadius = 6 * PlotScaling;
            var pointStyle = new Pen(Brushes.Transparent);
            var filling = new SolidColorBrush(Colors.Goldenrod, 0.33);

            foreach ((var x, var y) in DispersionPlotParameters.HitPoints)
            {
                Point center = new(Bounds.Center.X - (y * PlotScaling) - pointRadius, Bounds.Center.Y - (x * PlotScaling) - pointRadius);
                Rect pointRectangle = new(center, new Size(pointRadius * 2, pointRadius * 2));
                EllipseGeometry pointEllipse = new(pointRectangle);
                context.DrawGeometry(filling, pointStyle, pointEllipse);
            }
        }

        /// <summary>
        /// Draws the area where 50% of the shots lends on average.
        /// </summary>
        /// <param name="context">The <see cref="DrawingContext"/> used to draw the visualization.</param>
        private void DrawHalfHitPointsArea(DrawingContext context)
        {
            var xRadius = DispersionPlotParameters.ProjectedVerticalRadiusHalfHitPoints * PlotScaling;
            var yRadius = DispersionPlotParameters.HorizontalRadiusHalfHitPoints * PlotScaling;
            var filling = new SolidColorBrush(Colors.Red, 0.1);
            Point center = new(Bounds.Center.X - xRadius, Bounds.Center.Y - yRadius);
            Rect ellipseRectangle = new(center, new Size(xRadius * 2, yRadius * 2));
            EllipseGeometry dispersioEllipse = new(ellipseRectangle);
            context.DrawGeometry(filling, new Pen(Brushes.DarkRed, 2), dispersioEllipse);
        }

        /// <summary>
        /// Draws plot rulers and texts.
        /// </summary>
        /// <param name="context">The <see cref="DrawingContext"/> used to draw the visualization.</param>
        private void DrawAdditionalElements(DrawingContext context)
        {
            double offsetFromEllipse = 25;
            double textOffset = 5;
            double segmentEndHeight = 20;
            double plotMargin = 5;

            string unit = UnitLocalization.Unit_M;

            Point center = Bounds.Center;
            var xOuterRadius = DispersionPlotParameters.ProjectedVerticalRadius * PlotScaling;
            var yOuterRadius = DispersionPlotParameters.HorizontalRadius * PlotScaling;
            var xInnerRadius = DispersionPlotParameters.ProjectedVerticalRadiusHalfHitPoints * PlotScaling;
            var yInnerRadius = DispersionPlotParameters.HorizontalRadiusHalfHitPoints * PlotScaling;

            // text
            var verticalDiameter = new FormattedText($"{Translation.DispersionPlot_Vertical} {Math.Round(DispersionPlotParameters.ProjectedVerticalRadius * 2)} {unit}", Typeface.Default, FontSize, TextAlignment.Center, TextWrapping.NoWrap, Size.Infinity);
            var verticalDiameterHalfHitPoints = new FormattedText($"{Translation.DispersionPlot_InnerVertical} {Math.Round(DispersionPlotParameters.ProjectedVerticalRadiusHalfHitPoints * 2)} {unit}", Typeface.Default, FontSize, TextAlignment.Center, TextWrapping.NoWrap, Size.Infinity);
            var horizontalDiameter = new FormattedText($"{Translation.DispersionPlot_Horizontal} {Math.Round(DispersionPlotParameters.HorizontalRadius * 2)} {unit}", Typeface.Default, FontSize, TextAlignment.Center, TextWrapping.NoWrap, Size.Infinity);
            var horizontalDiameterHalfHitPoints = new FormattedText($"{Translation.DispersionPlot_InnerHorizontal} {Math.Round(DispersionPlotParameters.HorizontalRadiusHalfHitPoints * 2)} {unit}", Typeface.Default, FontSize, TextAlignment.Center, TextWrapping.NoWrap, Size.Infinity);

            // X axis
            context.DrawLine(new Pen(Brushes.Gray, 1), center.AddX(-xOuterRadius), center.AddX(xOuterRadius));

            // Y axis
            context.DrawLine(new Pen(Brushes.Gray, 1), center.AddY(-yOuterRadius), center.AddY(yOuterRadius));

            // verticalDiameter
            context.DrawLine(new Pen(Brushes.White, 1), center.AddX(-xOuterRadius).AddY(yOuterRadius + offsetFromEllipse), center.AddX(-xOuterRadius).AddY(yOuterRadius + offsetFromEllipse + segmentEndHeight));
            context.DrawLine(new Pen(Brushes.White, 1), center.AddX(xOuterRadius).AddY(yOuterRadius + offsetFromEllipse), center.AddX(xOuterRadius).AddY(yOuterRadius + offsetFromEllipse + segmentEndHeight));
            context.DrawLine(new Pen(Brushes.White, 1), center.AddX(-xOuterRadius).AddY(yOuterRadius + offsetFromEllipse + (segmentEndHeight / 2)), center.AddX(xOuterRadius).AddY(yOuterRadius + offsetFromEllipse + (segmentEndHeight / 2)));

            context.DrawText(Foreground, center.AddX(-verticalDiameter.Bounds.Width / 2).AddY(yOuterRadius + offsetFromEllipse + textOffset + verticalDiameter.Bounds.Height), verticalDiameter);

            // horizontalDiameter
            context.DrawLine(new Pen(Brushes.White, 1), center.AddX(-xOuterRadius - offsetFromEllipse).AddY(-yOuterRadius), center.AddX(-xOuterRadius - offsetFromEllipse - segmentEndHeight).AddY(-yOuterRadius));
            context.DrawLine(new Pen(Brushes.White, 1), center.AddX(-xOuterRadius - offsetFromEllipse).AddY(yOuterRadius), center.AddX(-xOuterRadius - offsetFromEllipse - segmentEndHeight).AddY(yOuterRadius));
            context.DrawLine(new Pen(Brushes.White, 1), center.AddX(-xOuterRadius - offsetFromEllipse - (segmentEndHeight / 2)).AddY(yOuterRadius), center.AddX(-xOuterRadius - offsetFromEllipse - (segmentEndHeight / 2)).AddY(-yOuterRadius));

            var rotation1 = context.PushSetTransform(Matrix.CreateRotation(-Math.PI / 2));
            context.DrawText(Foreground, center.SwapXY().MultiplyX(-1).AddX(-horizontalDiameter.Bounds.Width / 2).AddY(-xOuterRadius - offsetFromEllipse - textOffset - (2 * horizontalDiameter.Bounds.Height)), horizontalDiameter);
            rotation1.Dispose();

            // verticalDiameterHalfHitPoints
            context.DrawLine(new Pen(Brushes.Red, 1), center.AddX(-xInnerRadius).AddY(-yOuterRadius - offsetFromEllipse), center.AddX(-xInnerRadius).AddY(-yOuterRadius - offsetFromEllipse - segmentEndHeight));
            context.DrawLine(new Pen(Brushes.Red, 1), center.AddX(xInnerRadius).AddY(-yOuterRadius - offsetFromEllipse), center.AddX(xInnerRadius).AddY(-yOuterRadius - offsetFromEllipse - segmentEndHeight));
            context.DrawLine(new Pen(Brushes.Red, 1), center.AddX(-xInnerRadius).AddY(-yOuterRadius - offsetFromEllipse - (segmentEndHeight / 2)), center.AddX(xInnerRadius).AddY(-yOuterRadius - offsetFromEllipse - (segmentEndHeight / 2)));

            context.DrawText(Foreground, center.AddX(-verticalDiameterHalfHitPoints.Bounds.Width / 2).AddY(-yOuterRadius - offsetFromEllipse - textOffset - (2 * verticalDiameterHalfHitPoints.Bounds.Height)), verticalDiameterHalfHitPoints);

            // horizontalDiameterhalfHitPoints
            context.DrawLine(new Pen(Brushes.Red, 1), center.AddX(xOuterRadius + offsetFromEllipse).AddY(-yInnerRadius), center.AddX(xOuterRadius + offsetFromEllipse + segmentEndHeight).AddY(-yInnerRadius));
            context.DrawLine(new Pen(Brushes.Red, 1), center.AddX(xOuterRadius + offsetFromEllipse).AddY(yInnerRadius), center.AddX(xOuterRadius + offsetFromEllipse + segmentEndHeight).AddY(yInnerRadius));
            context.DrawLine(new Pen(Brushes.Red, 1), center.AddX(xOuterRadius + offsetFromEllipse + (segmentEndHeight / 2)).AddY(yInnerRadius), center.AddX(xOuterRadius + offsetFromEllipse + (segmentEndHeight / 2)).AddY(-yInnerRadius));

            var rotation2 = context.PushSetTransform(Matrix.CreateRotation(Math.PI / 2));
            context.DrawText(Foreground, center.SwapXY().AddX(-horizontalDiameterHalfHitPoints.Bounds.Width / 2).MultiplyY(-1).AddY(-xOuterRadius - offsetFromEllipse - textOffset - (2 * horizontalDiameterHalfHitPoints.Bounds.Height)), horizontalDiameterHalfHitPoints);
            rotation2.Dispose();

            // min plot size
            MinHeight = plotMargin + (2 * (yOuterRadius + offsetFromEllipse + textOffset + verticalDiameterHalfHitPoints.Bounds.Height + verticalDiameter.Bounds.Height));
            MinWidth = plotMargin + (2 * (xOuterRadius + offsetFromEllipse + textOffset + horizontalDiameter.Bounds.Height + horizontalDiameterHalfHitPoints.Bounds.Height));
        }

        private void DrawFusoReference(DrawingContext context)
        {
            var xRadius = 129.8 * PlotScaling;
            var yRadius = 11.2 * PlotScaling;
            var filling = new SolidColorBrush(Colors.Gray, 1);
            Point center = new(Bounds.Center.X - xRadius, Bounds.Center.Y - yRadius);
            Rect ellipseRectangle = new(center, new Size(xRadius * 2, yRadius * 2));
            EllipseGeometry dispersionEllipse = new(ellipseRectangle);
            context.DrawGeometry(filling, new Pen(Foreground, 4), dispersionEllipse);
        }
    }
}

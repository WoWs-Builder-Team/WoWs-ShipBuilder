using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using WoWsShipBuilder.Core.DataProvider;
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
            AvaloniaProperty.Register<DispersionPlot, double>(nameof(PlotScaling), 0.7);

        /// <summary>
        /// Styled Property for the dispersion ellipse plane.
        /// </summary>
        public static readonly StyledProperty<EllipsePlanes> EllipsePlaneProperty =
            AvaloniaProperty.Register<DispersionPlot, EllipsePlanes>(nameof(EllipsePlane), EllipsePlanes.HorizontalPlane);

        /// <summary>
        /// Styled Property for the Fuso reference.
        /// </summary>
        public static readonly StyledProperty<FusoPositions> FusoPositionProperty =
            AvaloniaProperty.Register<DispersionPlot, FusoPositions>(nameof(FusoPosition), FusoPositions.DontShow);

        /// <summary>
        /// Styled Property for the plot rotation.
        /// </summary>
        public static readonly StyledProperty<bool> IsVerticalProperty =
            AvaloniaProperty.Register<DispersionPlot, bool>(nameof(FusoPosition), true);

        static DispersionPlot()
        {
            DispersionPlotParametersProperty.Changed.AddClassHandler<DispersionPlot>((x, e) => x.InvalidateVisual());
            PlotScalingProperty.Changed.AddClassHandler<DispersionPlot>((x, e) => x.InvalidateVisual());
            EllipsePlaneProperty.Changed.AddClassHandler<DispersionPlot>((x, e) => x.InvalidateVisual());
            FusoPositionProperty.Changed.AddClassHandler<DispersionPlot>((x, e) => x.InvalidateVisual());
            IsVerticalProperty.Changed.AddClassHandler<DispersionPlot>((x, e) => x.InvalidateVisual());
        }

        public enum EllipsePlanes
        {
            RealPlane,
            HorizontalPlane,
            VerticalPlane,
        }

        public enum FusoPositions
        {
            DontShow,
            Broadside,
            BowIn,
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
        /// Gets or sets the ellipse plane.
        /// </summary>
        public EllipsePlanes EllipsePlane
        {
            get => GetValue(EllipsePlaneProperty);
            set => SetValue(EllipsePlaneProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Fuso reference has to be drawn or not.
        /// </summary>
        public FusoPositions FusoPosition
        {
            get => GetValue(FusoPositionProperty);
            set => SetValue(FusoPositionProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the plot orientation.
        /// </summary>
        public bool IsVertical
        {
            get => GetValue(IsVerticalProperty);
            set => SetValue(IsVerticalProperty, value);
        }

        /// <summary>
        /// Renders the content of the control, drawing the ellipse as well as the shots fired.
        /// For more details, see <see cref="Visual.Render"/>.
        /// </summary>
        /// <param name="context">The <see cref="DrawingContext"/> used to draw the visualization.</param>
        public override void Render(DrawingContext context)
        {
            var rect = new Rect(0, 0, Bounds.Width, Bounds.Height);
            Point center = rect.Center;

            DrawFusoReference(context, center);
            DrawHalfHitPointsArea(context, center);
            DrawAdditionalElements(context, center);
            DrawHitPointsArea(context, center);
            DrawHitPoints(context, center);
        }

        /// <summary>
        /// Draws the dispersion ellipse.
        /// </summary>
        /// <param name="context">The <see cref="DrawingContext"/> used to draw the visualization.</param>
        /// <param name="center">The center of the area.</param>
        private void DrawHitPointsArea(DrawingContext context, Point center)
        {
            double xRadius;
            if (EllipsePlane == EllipsePlanes.HorizontalPlane)
            {
                xRadius = DispersionPlotParameters.ProjectedOnWaterVerticalRadius;
            }
            else if (EllipsePlane == EllipsePlanes.RealPlane)
            {
                xRadius = DispersionPlotParameters.VerticalRadius;
            }
            else
            {
                xRadius = DispersionPlotParameters.ProjectedOnPerpendicularToWaterVerticalRadius;
            }

            xRadius *= PlotScaling;
            var yRadius = DispersionPlotParameters.HorizontalRadius * PlotScaling;

            if (IsVertical)
            {
                var tmp = xRadius;
                xRadius = yRadius;
                yRadius = tmp;
            }

            var filling = new SolidColorBrush(Colors.Gray, 0.1);
            Point ellipseCenter = new(center.X - xRadius, center.Y - yRadius);
            Rect ellipseRectangle = new(ellipseCenter, new Size(xRadius * 2, yRadius * 2));
            EllipseGeometry dispersionEllipse = new(ellipseRectangle);
            context.DrawGeometry(filling, new Pen(Foreground, 4), dispersionEllipse);
        }

        /// <summary>
        /// Draws shells hit points inside the dispersion ellipse.
        /// </summary>
        /// <param name="context">The <see cref="DrawingContext"/> used to draw the visualization.</param>
        /// <param name="center">The center of the area.</param>
        private void DrawHitPoints(DrawingContext context, Point center)
        {
            var pointRadius = 6d;

            if (EllipsePlane == EllipsePlanes.HorizontalPlane || PlotScaling < 1)
            {
                pointRadius *= PlotScaling;
            }

            var pointStyle = new Pen(Brushes.Transparent);
            var filling = new SolidColorBrush(Colors.Goldenrod, 0.33);

            List<(double x, double y)> hitPoints = new();

            if (EllipsePlane == EllipsePlanes.HorizontalPlane)
            {
                hitPoints = DispersionPlotParameters.OnWaterHitPoints;
            }
            else if (EllipsePlane == EllipsePlanes.RealPlane)
            {
                hitPoints = DispersionPlotParameters.RealHitPoints;
            }
            else
            {
                hitPoints = DispersionPlotParameters.PerpendicularToWaterHitPoints;
            }

            foreach ((var x, var y) in hitPoints)
            {
                Point pointCenter;

                if (IsVertical)
                {
                    pointCenter = new(center.X - (x * PlotScaling) - pointRadius, center.Y - (y * PlotScaling) - pointRadius);
                }
                else
                {
                    pointCenter = new(center.X - (y * PlotScaling) - pointRadius, center.Y - (x * PlotScaling) - pointRadius);
                }

                Rect pointRectangle = new(pointCenter, new Size(pointRadius * 2, pointRadius * 2));
                EllipseGeometry pointEllipse = new(pointRectangle);
                context.DrawGeometry(filling, pointStyle, pointEllipse);
            }
        }

        /// <summary>
        /// Draws the area where 50% of the shots lends on average.
        /// </summary>
        /// <param name="context">The <see cref="DrawingContext"/> used to draw the visualization.</param>
        /// <param name="center">The center of the area.</param>
        private void DrawHalfHitPointsArea(DrawingContext context, Point center)
        {
            double xRadius;
            if (EllipsePlane == EllipsePlanes.HorizontalPlane)
            {
                xRadius = DispersionPlotParameters.ProjectedOnWaterVerticalRadiusHalfHitPoints;
            }
            else if (EllipsePlane == EllipsePlanes.RealPlane)
            {
                xRadius = DispersionPlotParameters.VerticalRadiusHalfHitPoints;
            }
            else
            {
                xRadius = DispersionPlotParameters.ProjectedOnPerpendicularToWaterVerticalRadiusHalfHitPoints;
            }

            xRadius *= PlotScaling;
            var yRadius = DispersionPlotParameters.HorizontalRadiusHalfHitPoints * PlotScaling;

            if (IsVertical)
            {
                var tmp = xRadius;
                xRadius = yRadius;
                yRadius = tmp;
            }

            var filling = new SolidColorBrush(Colors.Red, 0.1);
            Point ellipseCenter = new(center.X - xRadius, center.Y - yRadius);
            Rect ellipseRectangle = new(ellipseCenter, new Size(xRadius * 2, yRadius * 2));
            EllipseGeometry dispersioEllipse = new(ellipseRectangle);
            context.DrawGeometry(filling, new Pen(Brushes.DarkRed, 2), dispersioEllipse);
        }

        /// <summary>
        /// Draws an ellipse with the same sizes of a Fuso to use as reference.
        /// </summary>
        /// <param name="context">The <see cref="DrawingContext"/> used to draw the visualization.</param>
        /// <param name="center">The center of the plot.</param>
        private void DrawFusoReference(DrawingContext context, Point center)
        {
            var length = 212.7;
            var width = 32.2;
            var height = 45;

            var plotMargin = 5;

            var yRadius = width;
            var xRadius = length;

            if (FusoPosition == FusoPositions.DontShow)
            {
                MinHeight = 0;
                MinWidth = 0;
                return;
            }
            else if (FusoPosition == FusoPositions.Broadside)
            {
                xRadius = width;
                yRadius = length;
            }

            if (EllipsePlane == EllipsePlanes.VerticalPlane)
            {
                xRadius = height;
            }

            xRadius *= PlotScaling;
            yRadius *= PlotScaling;

            if (IsVertical)
            {
                var tmp = xRadius;
                xRadius = yRadius;
                yRadius = tmp;
            }

            var filling = new SolidColorBrush(Colors.Black, 0.25);
            Point newCenter = new(center.X - xRadius, center.Y - yRadius);
            Rect ellipseRectangle = new(newCenter, new Size(xRadius * 2, yRadius * 2));
            EllipseGeometry dispersionEllipse = new(ellipseRectangle);
            if (EllipsePlane != EllipsePlanes.VerticalPlane)
            {
                context.DrawGeometry(filling, new Pen(Brushes.Black, 1), dispersionEllipse);
            }
            else
            {
                context.FillRectangle(filling, ellipseRectangle, 50);
                context.DrawRectangle(new Pen(Brushes.Black, 1), ellipseRectangle, 50);
            }

            MinHeight = (2 * yRadius) + plotMargin;
            MinWidth = (2 * xRadius) + plotMargin;
        }

        /// <summary>
        /// Draws plot rulers and texts.
        /// </summary>
        /// <param name="context">The <see cref="DrawingContext"/> used to draw the visualization.</param>
        /// <param name="center">The center of the plot.</param>
        private void DrawAdditionalElements(DrawingContext context, Point center)
        {
            double offsetFromEllipse = 25;
            double textOffset = 5;
            double segmentEndHeight = 20;
            double plotMargin = 5;

            string unit = UnitLocalization.Unit_M;

            double xOuterRadius;
            double yOuterRadius;
            double xInnerRadius;
            double yInnerRadius;
            if (EllipsePlane == EllipsePlanes.HorizontalPlane)
            {
                xOuterRadius = DispersionPlotParameters.ProjectedOnWaterVerticalRadius;
                xInnerRadius = DispersionPlotParameters.ProjectedOnWaterVerticalRadiusHalfHitPoints;
            }
            else if (EllipsePlane == EllipsePlanes.RealPlane)
            {
                xOuterRadius = DispersionPlotParameters.VerticalRadius;
                xInnerRadius = DispersionPlotParameters.VerticalRadiusHalfHitPoints;
            }
            else
            {
                xOuterRadius = DispersionPlotParameters.ProjectedOnPerpendicularToWaterVerticalRadius;
                xInnerRadius = DispersionPlotParameters.ProjectedOnPerpendicularToWaterVerticalRadiusHalfHitPoints;
            }

            xOuterRadius *= PlotScaling;
            xInnerRadius *= PlotScaling;
            yOuterRadius = DispersionPlotParameters.HorizontalRadius * PlotScaling;
            yInnerRadius = DispersionPlotParameters.HorizontalRadiusHalfHitPoints * PlotScaling;

            // text
            var typeface = Typeface.Default;
            if(AppData.Settings.SelectedLanguage.LocalizationFileName == "ja")
            {
                typeface = new Typeface("Yu Gothic UI", Typeface.Default.Style, Typeface.Default.Weight);
            }

            var vertical = Translation.DispersionPlot_Vertical;
            var innverVertical = Translation.DispersionPlot_InnerVertical;
            var horizontal = Translation.DispersionPlot_Horizontal;
            var innerHorizontal = Translation.DispersionPlot_InnerHorizontal;

            if (IsVertical)
            {
                var tmp1 = xOuterRadius;
                var tmp2 = xInnerRadius;
                xOuterRadius = yOuterRadius;
                xInnerRadius = yInnerRadius;
                yOuterRadius = tmp1;
                yInnerRadius = tmp2;

                var tmp3 = horizontal;
                var tmp4 = innerHorizontal;
                horizontal = vertical;
                innerHorizontal = innverVertical;
                vertical = tmp3;
                innverVertical = tmp4;
            }

            var verticalDiameter = new FormattedText($"{vertical} {Math.Round(xOuterRadius * 2 / PlotScaling)} {unit}", typeface, FontSize, TextAlignment.Left, TextWrapping.NoWrap, Size.Infinity);
            var verticalDiameterHalfHitPoints = new FormattedText($"{innverVertical} {Math.Round(xInnerRadius * 2 / PlotScaling)} {unit}", typeface, FontSize, TextAlignment.Left, TextWrapping.NoWrap, Size.Infinity);
            var horizontalDiameter = new FormattedText($"{horizontal} {Math.Round(yOuterRadius * 2 / PlotScaling)} {unit}", typeface, FontSize, TextAlignment.Left, TextWrapping.NoWrap, Size.Infinity);
            var horizontalDiameterHalfHitPoints = new FormattedText($"{innerHorizontal} {Math.Round(yInnerRadius * 2 / PlotScaling)} {unit}", typeface, FontSize, TextAlignment.Left, TextWrapping.NoWrap, Size.Infinity);

            // X axis
            context.DrawLine(new Pen(Brushes.Gray, 1), center.AddX(-xOuterRadius), center.AddX(xOuterRadius));

            // Y axis
            context.DrawLine(new Pen(Brushes.Gray, 1), center.AddY(-yOuterRadius), center.AddY(yOuterRadius));

            // verticalDiameter
            context.DrawLine(new Pen(Brushes.White, 1), center.AddX(-xOuterRadius).AddY(yOuterRadius + offsetFromEllipse), center.AddX(-xOuterRadius).AddY(yOuterRadius + offsetFromEllipse + segmentEndHeight));
            context.DrawLine(new Pen(Brushes.White, 1), center.AddX(xOuterRadius).AddY(yOuterRadius + offsetFromEllipse), center.AddX(xOuterRadius).AddY(yOuterRadius + offsetFromEllipse + segmentEndHeight));
            context.DrawLine(new Pen(Brushes.White, 1), center.AddX(-xOuterRadius).AddY(yOuterRadius + offsetFromEllipse + (segmentEndHeight / 2)), center.AddX(xOuterRadius).AddY(yOuterRadius + offsetFromEllipse + (segmentEndHeight / 2)));

            using (context.PushPostTransform(Matrix.CreateTranslation(center.X - (verticalDiameter.Bounds.Width / 2), center.Y + yOuterRadius + offsetFromEllipse + textOffset + verticalDiameter.Bounds.Height)))
            {
                context.DrawText(Foreground, default, verticalDiameter);
            }

            // horizontalDiameter
            context.DrawLine(new Pen(Brushes.White, 1), center.AddX(-xOuterRadius - offsetFromEllipse).AddY(-yOuterRadius), center.AddX(-xOuterRadius - offsetFromEllipse - segmentEndHeight).AddY(-yOuterRadius));
            context.DrawLine(new Pen(Brushes.White, 1), center.AddX(-xOuterRadius - offsetFromEllipse).AddY(yOuterRadius), center.AddX(-xOuterRadius - offsetFromEllipse - segmentEndHeight).AddY(yOuterRadius));
            context.DrawLine(new Pen(Brushes.White, 1), center.AddX(-xOuterRadius - offsetFromEllipse - (segmentEndHeight / 2)).AddY(yOuterRadius), center.AddX(-xOuterRadius - offsetFromEllipse - (segmentEndHeight / 2)).AddY(-yOuterRadius));

            using(context.PushPostTransform(Matrix.CreateRotation(-Math.PI / 2) * Matrix.CreateTranslation(center.X - xOuterRadius - offsetFromEllipse - textOffset - (2 * horizontalDiameter.Bounds.Height), center.Y + (horizontalDiameter.Bounds.Width / 2))))
            {
                context.DrawText(Foreground, default, horizontalDiameter);
            }

            // verticalDiameterHalfHitPoints
            context.DrawLine(new Pen(Brushes.Red, 1), center.AddX(-xInnerRadius).AddY(-yOuterRadius - offsetFromEllipse), center.AddX(-xInnerRadius).AddY(-yOuterRadius - offsetFromEllipse - segmentEndHeight));
            context.DrawLine(new Pen(Brushes.Red, 1), center.AddX(xInnerRadius).AddY(-yOuterRadius - offsetFromEllipse), center.AddX(xInnerRadius).AddY(-yOuterRadius - offsetFromEllipse - segmentEndHeight));
            context.DrawLine(new Pen(Brushes.Red, 1), center.AddX(-xInnerRadius).AddY(-yOuterRadius - offsetFromEllipse - (segmentEndHeight / 2)), center.AddX(xInnerRadius).AddY(-yOuterRadius - offsetFromEllipse - (segmentEndHeight / 2)));

            using (context.PushPostTransform(Matrix.CreateTranslation(center.X - (verticalDiameterHalfHitPoints.Bounds.Width / 2), center.Y - yOuterRadius - offsetFromEllipse - textOffset - (2 * verticalDiameterHalfHitPoints.Bounds.Height))))
            {
                context.DrawText(Foreground, default, verticalDiameterHalfHitPoints);
            }

            // horizontalDiameterHalfHitPoints
            context.DrawLine(new Pen(Brushes.Red, 1), center.AddX(xOuterRadius + offsetFromEllipse).AddY(-yInnerRadius), center.AddX(xOuterRadius + offsetFromEllipse + segmentEndHeight).AddY(-yInnerRadius));
            context.DrawLine(new Pen(Brushes.Red, 1), center.AddX(xOuterRadius + offsetFromEllipse).AddY(yInnerRadius), center.AddX(xOuterRadius + offsetFromEllipse + segmentEndHeight).AddY(yInnerRadius));
            context.DrawLine(new Pen(Brushes.Red, 1), center.AddX(xOuterRadius + offsetFromEllipse + (segmentEndHeight / 2)).AddY(yInnerRadius), center.AddX(xOuterRadius + offsetFromEllipse + (segmentEndHeight / 2)).AddY(-yInnerRadius));

            using (context.PushPostTransform(Matrix.CreateRotation(Math.PI / 2) * Matrix.CreateTranslation(center.X + xOuterRadius + offsetFromEllipse + textOffset + (2 * horizontalDiameterHalfHitPoints.Bounds.Height), center.Y - (horizontalDiameterHalfHitPoints.Bounds.Width / 2))))
            {
                context.DrawText(Foreground, default, horizontalDiameterHalfHitPoints);
            }

            // min plot size
            var height = 2 * (yOuterRadius + offsetFromEllipse + textOffset + verticalDiameterHalfHitPoints.Bounds.Height + verticalDiameter.Bounds.Height);
            var width = 2 * (xOuterRadius + offsetFromEllipse + textOffset + horizontalDiameter.Bounds.Height + horizontalDiameterHalfHitPoints.Bounds.Height);

            if (height > MinHeight)
            {
                MinHeight = height + plotMargin;
            }

            if (width > MinWidth)
            {
                MinWidth = width + plotMargin;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using WoWsShipBuilder.UI.Extensions;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.CustomControls
{
    /// <summary>
    /// Custom control for visualizing the firing angles of a ship's turrets.
    /// </summary>
    public class FiringAngleVisualizer : Control
    {
        private const double VerticalPadding = 50.0;

        /// <summary>
        /// StyledProperty for the <see cref="TurretModule"/> of the ship.
        /// </summary>
        public static readonly StyledProperty<TurretModule> TurretsProperty =
            AvaloniaProperty.Register<FiringAngleVisualizer, TurretModule>(nameof(Turrets));

        /// <summary>
        /// StyledProperty for the color of the ship.
        /// </summary>
        public static readonly StyledProperty<SolidColorBrush> ShipBackgroundProperty =
            AvaloniaProperty.Register<FiringAngleVisualizer, SolidColorBrush>(nameof(ShipBackground), new SolidColorBrush(Colors.Blue, 0.7));

        /// <summary>
        /// StyledProperty for the color of ship turrets.
        /// </summary>
        public static readonly StyledProperty<SolidColorBrush> TurretColorProperty =
            AvaloniaProperty.Register<FiringAngleVisualizer, SolidColorBrush>(nameof(TurretColor), new SolidColorBrush(Colors.Gray, 0.9));

        public static readonly StyledProperty<bool> ShowAllAnglesProperty =
            AvaloniaProperty.Register<FiringAngleVisualizer, bool>(nameof(ShowAllAngles), notifying: OnShowAllAnglesChanged);

        private (Gun gun, Point center, Geometry geometry)? selectedPosition;
        private List<(Gun gun, Geometry geometry, Point center)> turretGeometries = new();

        static FiringAngleVisualizer()
        {
            FocusableProperty.OverrideDefaultValue(typeof(FiringAngleVisualizer), true);
        }

        /// <summary>
        /// Gets or sets the <see cref="TurretModule"/> of the ship that has the turrets that should be displayed.
        /// </summary>
        public TurretModule Turrets
        {
            get => GetValue(TurretsProperty);
            set => SetValue(TurretsProperty, value);
        }

        /// <summary>
        /// Gets or sets the background color for the ship ellipse.
        /// </summary>
        public SolidColorBrush ShipBackground
        {
            get => GetValue(ShipBackgroundProperty);
            set => SetValue(ShipBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the color for the turret visualization on the ship.
        /// </summary>
        public SolidColorBrush TurretColor
        {
            get => GetValue(TurretColorProperty);
            set => SetValue(TurretColorProperty, value);
        }

        public bool ShowAllAngles
        {
            get => GetValue(ShowAllAnglesProperty);
            set => SetValue(ShowAllAnglesProperty, value);
        }

        public override void Render(DrawingContext context)
        {
            turretGeometries.Clear();
            context.FillRectangle(new SolidColorBrush(Colors.LightBlue, 0.5), new Rect(Bounds.Size));
            double maxHeight = Bounds.Height - VerticalPadding;
            Size shipRectSize = CalculateSize(maxHeight);
            double rectWidth = shipRectSize.Width;
            double horizontalOffset = (Bounds.Width - rectWidth) / 2;
            double verticalOffset = (Bounds.Height - maxHeight) / 2;
            var point = new Point(horizontalOffset, verticalOffset);

            var pen = new Pen();

            var shipRect = new Rect(point, shipRectSize);
            var shipBase = new EllipseGeometry(shipRect);
            context.DrawGeometry(ShipBackground, pen, shipBase);

            DrawTurrets(context, shipRect);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (ShowAllAngles)
            {
                return;
            }

            foreach ((Gun gun, Geometry geometry, Point center) entry in turretGeometries)
            {
                if (!entry.geometry.FillContains(e.GetPosition(this)))
                {
                    continue;
                }

                (Gun gun, Point center, Geometry geometry) thisPosition = (entry.gun, entry.center, entry.geometry);

                selectedPosition = thisPosition;

                InvalidateVisual();
                return;
            }

            selectedPosition = null;
            InvalidateVisual();
        }

        protected override void OnPointerLeave(PointerEventArgs e)
        {
            if (ShowAllAngles)
            {
                return;
            }

            e.Handled = true;
            if (selectedPosition != null)
            {
                selectedPosition = null;
                InvalidateVisual();
            }
        }

        private static void OnShowAllAnglesChanged(IAvaloniaObject target, bool beforeNotify)
        {
            if (!beforeNotify)
            {
                var visualizer = (FiringAngleVisualizer)target;
                visualizer.InvalidateVisual();
            }
        }

        /// <summary>
        /// Helper method to convert a polar coordinate to a cartesian coordinate.
        /// </summary>
        /// <param name="angle">The angle of the polar coordinate.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="center">The center of the circle.</param>
        /// <returns>A Point representing the matching cartesian coordinate.</returns>
        private static Point PolarToCartesian(double angle, double radius, Point center)
        {
            // Polar angle is relative to horizontal but we need vertical angle.
            angle -= 90;

            double radiant = (Math.PI / 180) * angle;
            double x = center.X + (radius * Math.Cos(radiant));
            double y = center.Y + (radius * Math.Sin(radiant));
            return new Point(x, y);
        }

        private static bool IsLargeArc(double startAngle, double endAngle)
        {
            return startAngle switch
            {
                < 0 when endAngle < 0 => Math.Abs(startAngle + endAngle) > 180,
                < 0 => Math.Abs(startAngle) + endAngle > 180,
                _ => endAngle - startAngle > 180
            };
        }

        private static double CalculateRadius(Rect shipRect)
        {
            return shipRect.Width * 0.25;
        }

        private static double GetHorizontalBorderCoordinate(double y, Point ellipseCenter, Size ellipseSize, double horizontalTurretPosition)
        {
            // Use variable names to match the standard ellipse equation.
            // Standard equation: (x-h)^2/b^2 + (y-k)^2/a^2 = 1
            // With (h,k) being the origin of the ellipse, length of major axis = 2a, length of minor axis = 2b
            (double h, double k) = ellipseCenter;

            double a = ellipseSize.Height / 2;
            double b = ellipseSize.Width / 2;

            double tmp = Math.Sqrt(Math.Pow(b, 2) - ((Math.Pow(y - k, 2) * Math.Pow(b, 2)) / Math.Pow(a, 2)));

            double result;
            if (horizontalTurretPosition > 1)
            {
                result = h - tmp;
            }
            else
            {
                result = h + tmp;
            }

            return result;
        }

        /// <summary>
        /// Helper method to calculate the size of the ship rectangle.
        /// </summary>
        /// <param name="maxHeight">The maximum height of the rectangle.</param>
        /// <returns>The size of the rectangle enclosing the ship ellipse.</returns>
        private Size CalculateSize(double maxHeight)
        {
            var result = new Size(maxHeight / 3.5, maxHeight);
            if (result.Width > Bounds.Width)
            {
                result = new Size(Bounds.Width, Bounds.Width * 4);
            }

            return result;
        }

        /// <summary>
        /// Draws the turrets of the current turret module.
        /// </summary>
        /// <param name="context">The drawing context used to draw.</param>
        /// <param name="shipRect">The rectangle enclosing the ship ellipse.</param>
        private void DrawTurrets(DrawingContext context, Rect shipRect)
        {
            foreach (var gun in Turrets.Guns)
            {
                DrawTurret(context, gun, shipRect);
            }

            if (!ShowAllAngles && selectedPosition != null)
            {
                DrawTurretAngles(context, selectedPosition.Value.gun, selectedPosition.Value.center, shipRect);
            }
        }

        /// <summary>
        /// Draws a single turret of a ship.
        /// </summary>
        /// <param name="context">The drawing context used to draw.</param>
        /// <param name="shipTurret">The <see cref="Gun"/> object containing the data of the turret.</param>
        /// <param name="shipRect">The rectangle enclosing the ship ellipse.</param>
        private void DrawTurret(DrawingContext context, Gun shipTurret, Rect shipRect)
        {
            double radius = CalculateRadius(shipRect);
            double maxHeight = shipRect.Height - (2 * radius);

            // Position of turrets ranges from about -1.5 to 6.5.
            // Offset first turret by value of radius to make sure that even a turret at -1.5 is rendered within the bounds of the ship.
            double verticalPosition = ((maxHeight / 8) * (shipTurret.VerticalPosition + 1.5)) + (VerticalPadding / 2) + radius;

            double horizontalOffset = GetHorizontalBorderCoordinate(verticalPosition, shipRect.Center, shipRect.Size, shipTurret.HorizontalPosition);
            double horizontalPosition = shipTurret.HorizontalPosition switch
            {
                < 1 => horizontalOffset - 10,
                > 1 => horizontalOffset + 10,
                _ => shipRect.Left + (shipRect.Width / 2),
            };

            var center = new Point(horizontalPosition, verticalPosition);
            var pen = new Pen();

            float startAngle = shipTurret.HorizontalSector[0];
            float endAngle = shipTurret.HorizontalSector[1];

            if (shipTurret.VerticalPosition >= 3)
            {
                startAngle += 180;
                endAngle += 180;
            }

            Point start = PolarToCartesian(startAngle, radius, center);
            Point end = PolarToCartesian(endAngle, radius, center);

            var geometry = new StreamGeometry();
            StreamGeometryContext geometryContext = geometry.Open();
            geometryContext.SetFillRule(FillRule.EvenOdd);
            geometryContext.BeginFigureFluent(center, true)
                .LineToFluent(start)
                .ArcToFluent(end, new Size(radius, radius), 0.0, IsLargeArc(startAngle, endAngle), SweepDirection.Clockwise)
                .LineToFluent(center)
                .EndFigure(true);

            context.DrawGeometry(TurretColor, pen, geometry);
            turretGeometries.Add((shipTurret, geometry, center));
            if (ShowAllAngles)
            {
                DrawTurretAngles(context, shipTurret, center, shipRect);
            }
        }

        private void DrawTurretAngles(DrawingContext context, Gun shipTurret, Point center, Rect shipRect)
        {
            double radius = CalculateRadius(shipRect) * 3;
            float startAngle = shipTurret.HorizontalSector[0];
            float endAngle = shipTurret.HorizontalSector[1];
            var facingBackwards = false;

            if (shipTurret.VerticalPosition >= 3)
            {
                startAngle += 180;
                endAngle += 180;
                facingBackwards = true;
            }

            var geometry = new StreamGeometry();
            StreamGeometryContext geometryContext = geometry.Open();
            geometryContext.SetFillRule(FillRule.EvenOdd);

            Point startPoint = PolarToCartesian(startAngle, radius, center);
            Point endPoint = PolarToCartesian(endAngle, radius, center);

            geometryContext.BeginFigureFluent(center, true)
                .LineToFluent(startPoint)
                .ArcToFluent(endPoint, new Size(radius, radius), 0.0, IsLargeArc(startAngle, endAngle), SweepDirection.Clockwise)
                .LineToFluent(center)
                .EndFigure(true);

            context.DrawGeometry(new SolidColorBrush(Colors.LightGray, 0.7), new Pen(), geometry);

            var text = new FormattedText($"{startAngle}° - {endAngle}°", Typeface.Default, 14, TextAlignment.Center, TextWrapping.NoWrap, Size.Infinity);

            double textY = center.Y;
            if (!facingBackwards)
            {
                textY -= text.Bounds.Height;
            }

            double textX = shipTurret.HorizontalPosition switch
            {
                > 1 => center.X - text.Bounds.Width,
                < 1 => center.X,
                _ => center.X - (text.Bounds.Width / 2)
            };

            var textOrigin = new Point(textX, textY);
            context.DrawText(new SolidColorBrush(Colors.Black), textOrigin, text);
        }
    }
}

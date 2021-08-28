using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.CustomControls
{
    /// <summary>
    /// Custom control for visualizing the firing angles of a ship's turrets.
    /// </summary>
    public class FiringAngleVisualizer : Control
    {
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

        public override void Render(DrawingContext context)
        {
            const double verticalPadding = 20.0;
            double maxHeight = Bounds.Height - verticalPadding;
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
        }

        /// <summary>
        /// Draws a single turret of a ship.
        /// </summary>
        /// <param name="context">The drawing context used to draw.</param>
        /// <param name="shipTurret">The <see cref="Gun"/> object containing the data of the turret.</param>
        /// <param name="shipRect">The rectangle enclosing the ship ellipse.</param>
        private void DrawTurret(DrawingContext context, Gun shipTurret, Rect shipRect)
        {
            double radius = shipRect.Width * 0.25;
            double maxHeight = shipRect.Height - (2 * radius);
            double offsetTolerance = shipRect.Width / 20;

            double horizontalPosition = shipTurret.HorizontalPosition switch
            {
                < 1 => shipRect.Right - offsetTolerance,
                > 1 => shipRect.Left + offsetTolerance,
                _ => shipRect.Left + (shipRect.Width / 2),
            };

            double verticalPosition = ((maxHeight / 8) * (shipTurret.VerticalPosition + 1)) + 20;

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

            var startLine = new LineSegment
            {
                Point = start,
            };
            var endLine = new LineSegment
            {
                Point = center,
            };
            var arcSegment = new ArcSegment
            {
                Point = end,
                IsLargeArc = true,
                Size = new Size(radius, radius),
                SweepDirection = SweepDirection.Clockwise,
            };

            var figures = new PathFigures
            {
                new()
                {
                    StartPoint = center,
                    IsClosed = true,
                    IsFilled = true,
                    Segments = new PathSegments { startLine, arcSegment, endLine },
                },
            };

            var geometry = new PathGeometry
            {
                Figures = figures,
                FillRule = FillRule.EvenOdd,
                Transform = null,
            };
            context.DrawGeometry(TurretColor, pen, geometry);
        }
    }
}

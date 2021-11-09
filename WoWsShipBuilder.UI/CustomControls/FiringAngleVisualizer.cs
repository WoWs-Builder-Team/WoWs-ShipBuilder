using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using WoWsShipBuilder.UI.Extensions;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.CustomControls
{
    /// <summary>
    /// Custom control for visualizing the firing angles of a ship's turrets.
    /// </summary>
    [PseudoClasses(":extended")]
    public class FiringAngleVisualizer : TemplatedControl
    {
        #region Static Fields and Constants

        private const double VerticalPadding = 50.0;

        /// <summary>
        /// Styled Property for the background color of the control's area.
        /// </summary>
        public static readonly StyledProperty<SolidColorBrush> ShipBackgroundProperty =
            AvaloniaProperty.Register<FiringAngleVisualizer, SolidColorBrush>(nameof(ShipBackground), new SolidColorBrush(Colors.LightBlue, 0.5));

        /// <summary>
        /// StyledProperty for the color of the ship.
        /// </summary>
        public static readonly StyledProperty<SolidColorBrush> ShipColorProperty =
            AvaloniaProperty.Register<FiringAngleVisualizer, SolidColorBrush>(nameof(ShipColor), new SolidColorBrush(Colors.Blue, 0.95));

        /// <summary>
        /// StyledProperty for the color of ship turrets.
        /// </summary>
        public static readonly StyledProperty<SolidColorBrush> TurretColorProperty =
            AvaloniaProperty.Register<FiringAngleVisualizer, SolidColorBrush>(nameof(TurretColor), new SolidColorBrush(Colors.Gray));

        /// <summary>
        /// Styled property for the color of the turret angle visualization.
        /// </summary>
        public static readonly StyledProperty<SolidColorBrush> TurretAngleColorProperty =
            AvaloniaProperty.Register<FiringAngleVisualizer, SolidColorBrush>(nameof(TurretAngleColor), new SolidColorBrush(Colors.LightGray, 0.7));

        /// <summary>
        /// Styled property for the color of the turret dead zones in the firing angle visualization.
        /// </summary>
        public static readonly StyledProperty<SolidColorBrush> DeadZoneColorProperty =
            AvaloniaProperty.Register<FiringAngleVisualizer, SolidColorBrush>(nameof(DeadZoneColor), new SolidColorBrush(Colors.Red, 0.6));

        /// <summary>
        /// Styled property for the color of the turret dead zones.
        /// </summary>
        public static readonly StyledProperty<SolidColorBrush> TurretDeadZoneColorProperty =
            AvaloniaProperty.Register<FiringAngleVisualizer, SolidColorBrush>(nameof(TurretDeadZoneColor), new SolidColorBrush(Colors.Red, 0.4));

        /// <summary>
        /// StyledProperty for the <see cref="TurretModule"/> of the ship.
        /// </summary>
        public static readonly StyledProperty<TurretModule> TurretsProperty = AvaloniaProperty.Register<FiringAngleVisualizer, TurretModule>(nameof(Turrets));

        /// <summary>
        /// Styled properties determining whether all turret angles are shown at once.
        /// Trigger an update of the pseudo classes to make sure the radius factor is increased properly.
        /// </summary>
        public static readonly StyledProperty<bool> ShowAllAnglesProperty = AvaloniaProperty.Register<FiringAngleVisualizer, bool>(
            nameof(ShowAllAngles),
            notifying:
            (target, beforeNotifying) =>
            {
                if (!beforeNotifying)
                {
                    ((FiringAngleVisualizer)target).UpdatePseudoClasses();
                }
            });

        /// <summary>
        /// Styled property determining whether all turret angles texts are shown at once.
        /// </summary>
        public static readonly StyledProperty<bool> PermanentAngleTextProperty =
            AvaloniaProperty.Register<FiringAngleVisualizer, bool>(nameof(PermanentAngleText));

        /// <summary>
        /// Styled property that specifies the factor applied to the turret radius when showing turret firing sectors.
        /// </summary>
        public static readonly StyledProperty<double> RadiusFactorProperty = AvaloniaProperty.Register<FiringAngleVisualizer, double>(nameof(RadiusFactor), 1);

        #endregion

        private readonly List<(Point textOrigin, FormattedText text)> angleTexts = new();

        private readonly List<(Gun gun, Geometry geometry, Point center)> turretGeometries = new();

        private (Gun gun, Point center, Geometry geometry)? selectedPosition;

        /// <summary>
        /// Static constructor to override default values.
        /// </summary>
        static FiringAngleVisualizer()
        {
            FontSizeProperty.OverrideDefaultValue(typeof(FiringAngleVisualizer), 14);
            FontWeightProperty.OverrideDefaultValue(typeof(FiringAngleVisualizer), FontWeight.Bold);
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
        /// Gets or sets the background color of the area behind the ship ellipse.
        /// </summary>
        public SolidColorBrush ShipBackground
        {
            get => GetValue(ShipBackgroundProperty);
            set => SetValue(ShipBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the background color for the ship ellipse.
        /// </summary>
        public SolidColorBrush ShipColor
        {
            get => GetValue(ShipColorProperty);
            set => SetValue(ShipColorProperty, value);
        }

        /// <summary>
        /// Gets or sets the color for the turret visualization on the ship.
        /// </summary>
        public SolidColorBrush TurretColor
        {
            get => GetValue(TurretColorProperty);
            set => SetValue(TurretColorProperty, value);
        }

        /// <summary>
        /// Gets or sets the fill color of the turret angle visualization.
        /// </summary>
        public SolidColorBrush TurretAngleColor
        {
            get => GetValue(TurretAngleColorProperty);
            set => SetValue(TurretAngleColorProperty, value);
        }

        /// <summary>
        /// Gets or sets the color of the dead zone angle visualization.
        /// </summary>
        public SolidColorBrush DeadZoneColor
        {
            get => GetValue(DeadZoneColorProperty);
            set => SetValue(DeadZoneColorProperty, value);
        }

        /// <summary>
        /// Gets or sets the color of the dead zone turret visualization.
        /// </summary>
        public SolidColorBrush TurretDeadZoneColor
        {
            get => GetValue(TurretDeadZoneColorProperty);
            set => SetValue(TurretDeadZoneColorProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether all angles should be shown regardless of mouse position/click.
        /// </summary>
        public bool ShowAllAngles
        {
            get => GetValue(ShowAllAnglesProperty);
            set
            {
                UpdatePseudoClasses(value);
                SetValue(ShowAllAnglesProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the turret angle texts should be permanently visible.
        /// </summary>
        public bool PermanentAngleText
        {
            get => GetValue(PermanentAngleTextProperty);
            set => SetValue(PermanentAngleTextProperty, value);
        }

        /// <summary>
        /// Gets or sets the radius factor that is used to determine the size of the firing sector visualization.
        /// </summary>
        public double RadiusFactor
        {
            get => GetValue(RadiusFactorProperty);
            set => SetValue(RadiusFactorProperty, value);
        }

        /// <summary>
        /// Renders the content of the control, drawing the turrets as well as their firing angles.
        /// For more details, see <see cref="Visual.Render"/>.
        /// </summary>
        /// <param name="context">The <see cref="DrawingContext"/> used to draw the visualization.</param>
        public override void Render(DrawingContext context)
        {
            turretGeometries.Clear();
            context.FillRectangle(ShipBackground, new Rect(Bounds.Size));
            double maxHeight = Bounds.Height - VerticalPadding;
            Size shipRectSize = CalculateSize(maxHeight);
            double rectWidth = shipRectSize.Width;
            double horizontalOffset = (Bounds.Width - rectWidth) / 2;
            double verticalOffset = (Bounds.Height - maxHeight) / 2;
            var point = new Point(horizontalOffset, verticalOffset);

            var pen = new Pen();

            var shipRect = new Rect(point, shipRectSize);
            var shipBase = new EllipseGeometry(shipRect);
            context.DrawGeometry(ShipColor, pen, shipBase);

            DrawTurrets(context, shipRect);
        }

        /// <summary>
        /// Processes a pointer event. If the event location is on a turret, the firing angles of the turret are rendered.
        /// A click anywhere else in the control hides the angle visualization.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (ShowAllAngles)
            {
                return;
            }

            foreach ((var gun, var geometry, Point center) in turretGeometries)
            {
                if (!geometry.FillContains(e.GetPosition(this)))
                {
                    continue;
                }

                (Gun gun, Point center, Geometry geometry) thisPosition = (gun, center, geometry);
                Debug.WriteLine(gun.WgGunIndex);

                if (selectedPosition?.center != thisPosition.center)
                {
                    UpdatePseudoClasses(false);
                    selectedPosition = thisPosition;
                    UpdatePseudoClasses(true);
                }
                else
                {
                    selectedPosition = null;
                    UpdatePseudoClasses();
                }

                return;
            }

            if (selectedPosition != null)
            {
                selectedPosition = null;
                UpdatePseudoClasses(false);
            }
        }

        /// <summary>
        /// Processes a property change and requests a redraw of the control.
        /// </summary>
        /// <param name="change">The changed property.</param>
        /// <typeparam name="T">The type of the property.</typeparam>
        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
#pragma warning disable 8631
            base.OnPropertyChanged(change);
#pragma warning restore 8631

            if (change.Property != IsPointerOverProperty)
            {
                InvalidateVisual();
            }
        }

        /// <summary>
        /// Hides the firing angles if the pointer leaves the control except when <see cref="ShowAllAngles"/> is true.
        /// </summary>
        /// <param name="e">The event args.</param>
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
                UpdatePseudoClasses(false);
                InvalidateVisual();
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

            double radian = (Math.PI / 180) * angle;
            double x = center.X + (radius * Math.Cos(radian));
            double y = center.Y + (radius * Math.Sin(radian));
            return new Point(x, y);
        }

        /// <summary>
        /// Helper method to check if the angle between the two values is a large or a small angle.
        /// </summary>
        /// <param name="startAngle">The start of the firing sector in degrees. Can be negative.</param>
        /// <param name="endAngle">The end of the firing sector in degrees. Should not be negative.</param>
        /// <returns><see langword="true"/> if the difference is greater than 180 degrees, <see langword="false"/> otherwise.</returns>
        private static bool IsLargeArc(double startAngle, double endAngle)
        {
            return startAngle switch
            {
                <= 0 when endAngle < 0 => Math.Abs(startAngle) + (endAngle + 360) > 180,
                < 0 when endAngle > 0 => Math.Abs(startAngle) + endAngle > 180,
                _ => endAngle - startAngle > 180,
            };
        }

        /// <summary>
        /// Helper method to calculate the proper radius of the turrets.
        /// </summary>
        /// <param name="shipRect">The rectangle enclosing the ship ellipse.</param>
        /// <returns>A quarter of the width of the ship rectangle.</returns>
        private static double CalculateRadius(Rect shipRect)
        {
            return shipRect.Width * 0.25;
        }

        /// <summary>
        /// Helper method to calculate the x coordinate of the border of the ship ellipse for a given y coordinate.
        /// </summary>
        /// <param name="y">The y coordinate of the point.</param>
        /// <param name="ellipseCenter">The center of the ellipse.</param>
        /// <param name="ellipseSize">The size of the ellipse.</param>
        /// <param name="horizontalTurretPosition">The horizontal turret position of the ship turret, see <see cref="Gun.HorizontalPosition"/>.</param>
        /// <returns>The x coordinate for the requested point.</returns>
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
        /// Helper method to draw an arc on a StreamGeometryContext. Modifies the provided geometryContext.
        /// </summary>
        /// <param name="geometryContext">The context to draw on.</param>
        /// <param name="startAngle">The start angle of the arc.</param>
        /// <param name="endAngle">The end angle of the arc.</param>
        /// <param name="radius">The radius of the arc.</param>
        /// <param name="center">The center of the arc.</param>
        private static void DrawArcFigure(StreamGeometryContext geometryContext, double startAngle, double endAngle, double radius, Point center)
        {
            Point startPoint = PolarToCartesian(startAngle, radius, center);
            Point endPoint = PolarToCartesian(endAngle, radius, center);

            geometryContext.BeginFigureFluent(center, true)
                .LineToFluent(startPoint)
                .ArcToFluent(endPoint, new Size(radius, radius), 0.0, IsLargeArc(startAngle, endAngle), SweepDirection.Clockwise)
                .LineToFluent(center)
                .EndFigure(true);
        }

        /// <summary>
        /// Helper method to draw positive and negative angle indicators.
        /// </summary>
        /// <param name="context">The drawing context used to draw the indicators.</param>
        private void DrawIndicators(DrawingContext context)
        {
            var typeface = new Typeface(FontFamily, FontStyle, FontWeight.Bold);
            var fontSize = 28;
            var plusText = new FormattedText("+", typeface, fontSize, TextAlignment.Center, TextWrapping.NoWrap, Size.Infinity);
            var minusText = new FormattedText("-", typeface, fontSize, TextAlignment.Center, TextWrapping.NoWrap, Size.Infinity);
            var leftPoint = new Point(20, (Bounds.Size.Height / 2) - (minusText.Bounds.Height / 2));
            context.DrawText(Foreground, leftPoint, minusText);
            var rightPoint = new Point(Bounds.Size.Width - 20 - plusText.Bounds.Width, (Bounds.Size.Height / 2) - (plusText.Bounds.Height / 2));
            context.DrawText(Foreground, rightPoint, plusText);
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
            angleTexts.Clear();
            foreach (var gun in Turrets.Guns)
            {
                DrawTurret(context, gun, shipRect);
            }

            if (!ShowAllAngles && selectedPosition != null)
            {
                DrawTurretAngles(context, selectedPosition.Value.gun, selectedPosition.Value.center, shipRect);
            }
            else if (ShowAllAngles)
            {
                foreach ((Point center, Gun shipTurret) in turretGeometries.Select(entry => (entry.center, entry.gun)))
                {
                    DrawTurretAngles(context, shipTurret, center, shipRect);
                }
            }

            foreach ((Point textOrigin, FormattedText text) in angleTexts)
            {
                DrawAngleText(context, textOrigin, text);
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

            double startAngle = shipTurret.HorizontalSector[0];
            double endAngle = shipTurret.HorizontalSector[1];
            var angleOffset = 0.0;

            if (endAngle < 0)
            {
                endAngle += 360;
            }

            if (shipTurret.TurretOrientation == TurretOrientation.Backward)
            {
                angleOffset = 180;
                startAngle += 180;
                endAngle += 180;
            }

            var drawingGroup = new DrawingGroup();
            PathGeometry turretGeometry = PathGeometry.Parse(
                "M 10.451042,11.377083 9.525,12.170833 H 3.4395833 l -0.79375,-0.79375 h -1.5875 v -1.322916 h 0.79375 V 8.4666667 c 0,-0.5291667 " +
                "0.3002661,-1.3683176 0.9634748,-1.6052961 0.449291,-0.1888714 0.8886086,-0.2467873 1.4177752,-0.2467873 v -5.55625 h 1.5875 v 5.55625 h " +
                "1.5875 v -5.55625 h 1.5875 v 5.55625 c 0.5291667,0 1.2930047,0.095838 1.8008907,0.4810755 0.402385,0.3919374 0.580359,1.1064245 " +
                "0.580359,1.5319045 v 1.4266037 h 0.79375 v 1.322916 z");

            PathGeometry turretOutlines = PathGeometry.Parse("m 7.4083332,6.6145832 v 1.0583334 h 1.5875 V 6.6145832 m -4.7624999,0 v 1.0583334 h 1.5875 V 6.6145832");
            turretGeometry.FillRule = FillRule.NonZero;
            double turretScale = radius * 2 / turretGeometry.Bounds.Width;
            double xOffset = turretGeometry.Bounds.Left * turretScale;
            double yOffset = turretGeometry.Bounds.Top * turretScale;
            var matrix = Matrix.CreateScale(turretScale, turretScale);
            if (shipTurret.TurretOrientation == TurretOrientation.Backward)
            {
                matrix *= Matrix.CreateRotation(Math.PI);
                matrix *= Matrix.CreateTranslation(center.X + ((turretGeometry.Bounds.Width * turretScale) / 2) + xOffset, center.Y + (turretGeometry.Bounds.Width * turretScale * 0.75) + yOffset);
            }
            else
            {
                matrix *= Matrix.CreateTranslation(center.X - ((turretGeometry.Bounds.Width * turretScale) / 2) - xOffset, center.Y - (turretGeometry.Bounds.Width * turretScale * 0.75) - yOffset);
            }

            turretGeometry.Transform = new MatrixTransform(matrix);
            turretOutlines.Transform = new MatrixTransform(matrix);

            drawingGroup.AddChild(turretGeometry, TurretColor, Brushes.Black);
            drawingGroup.AddChild(turretOutlines, Brushes.Transparent, Brushes.Black);

            drawingGroup.Draw(context);

            turretGeometries.Add((shipTurret, turretGeometry, center));

            if (PermanentAngleText)
            {
                angleTexts.Add(CalculateTextPosition(startAngle, endAngle, center, shipTurret, angleOffset > 0));
            }
        }

        /// <summary>
        /// Renders the firing sectors of a turret.
        /// </summary>
        /// <param name="context">The drawing context for the rendering.</param>
        /// <param name="shipTurret">The ship turret to draw angles for.</param>
        /// <param name="center">The center of the turret visualization.</param>
        /// <param name="shipRect">The rectangle enclosing the ship ellipse.</param>
        private void DrawTurretAngles(DrawingContext context, Gun shipTurret, Point center, Rect shipRect)
        {
            double radius = CalculateRadius(shipRect) * RadiusFactor;
            double startAngle = shipTurret.HorizontalSector[0];
            double endAngle = shipTurret.HorizontalSector[1];
            var facingBackwards = false;

            if (endAngle < 0)
            {
                endAngle += 360;
            }

            if (shipTurret.VerticalPosition >= 3)
            {
                startAngle += 180;
                endAngle += 180;
                facingBackwards = true;
            }

            var drawingGroup = new DrawingGroup();
            drawingGroup.AddChild(CreateArcs(startAngle, endAngle, radius, center), TurretAngleColor, new SolidColorBrush(Colors.DarkGray));

            if (shipTurret.HorizontalDeadZones != null)
            {
                foreach (double[] deadZone in shipTurret.HorizontalDeadZones)
                {
                    double zoneStart = deadZone[0];
                    double zoneEnd = deadZone[1];
                    if (facingBackwards)
                    {
                        zoneStart += 180;
                        zoneEnd += 180;
                    }

                    Geometry arcGeometry = CreateArcs(zoneStart, zoneEnd, radius, center);
                    drawingGroup.AddChild(arcGeometry, DeadZoneColor, DeadZoneColor);
                }
            }

            drawingGroup.Draw(context);

            angleTexts.Add(CalculateTextPosition(startAngle, endAngle, center, shipTurret, facingBackwards));
        }

        /// <summary>
        /// Renders the angle text using the drawing context.
        /// </summary>
        /// <param name="context">The Drawing context.</param>
        /// <param name="textOrigin">The point of origin for the text.</param>
        /// <param name="text">The formatted text.</param>
        private void DrawAngleText(DrawingContext context, Point textOrigin, FormattedText text)
        {
            context.DrawText(Foreground, textOrigin, text);
            DrawIndicators(context);
        }

        /// <summary>
        /// Calculate the point of origin for a formatted text.
        /// </summary>
        /// <param name="startAngle">The start angle.</param>
        /// <param name="endAngle">The end angle.</param>
        /// <param name="center">The center of the associated turret.</param>
        /// <param name="shipTurret">The associated turret.</param>
        /// <param name="facingBackwards">Determines whether the turret is facing backwards.</param>
        /// <returns>The point of origin as well as the formatted text for the provided parameters.</returns>
        private (Point origin, FormattedText text) CalculateTextPosition(double startAngle, double endAngle, Point center, Gun shipTurret, bool facingBackwards)
        {
            double effectiveStartAngle = startAngle % 360;
            double effectiveEndAngle = endAngle % 360;

            if (effectiveStartAngle >= 180)
            {
                effectiveStartAngle -= 360;
            }

            if (effectiveEndAngle >= 180)
            {
                effectiveEndAngle -= 360;
            }

            var text = new FormattedText(
                $"{effectiveStartAngle}° to {effectiveEndAngle}°",
                new Typeface(FontFamily, FontStyle, FontWeight),
                FontSize,
                TextAlignment.Center,
                TextWrapping.NoWrap,
                Size.Infinity);

            double textY = center.Y;
            if (!facingBackwards)
            {
                textY -= text.Bounds.Height * 0.2;
            }
            else
            {
                textY -= text.Bounds.Height * 0.5;
            }

            double textX = shipTurret.HorizontalPosition switch
            {
                > 1 => center.X - text.Bounds.Width,
                < 1 => center.X,
                _ => center.X - (text.Bounds.Width / 2),
            };

            var textOrigin = new Point(textX, textY);
            return (textOrigin, text);
        }

        /// <summary>
        /// Helper method that draws a firing sector between two angles.
        /// </summary>
        /// <param name="startAngle">The start angle.</param>
        /// <param name="endAngle">The end angle.</param>
        /// <param name="radius">The radius of the arc.</param>
        /// <param name="center">The center of the turret visualization.</param>
        /// <returns>A <see cref="Geometry"/> representing the generated structure.</returns>
        private Geometry CreateArcs(double startAngle, double endAngle, double radius, Point center)
        {
            return CreateArcs(new List<double> { startAngle, endAngle }, radius, center);
        }

        /// <summary>
        /// Helper method that draws a firing sector between two angles.
        /// </summary>
        /// <param name="angles">A list of angles of the firing sectors of the turret.</param>
        /// <param name="radius">The radius of the arc.</param>
        /// <param name="center">The center of the turret visualization.</param>
        /// <param name="onlyDeadZones">Indicates whether only the dead zones should be drawn.</param>
        /// <returns>A <see cref="Geometry"/> representing the generated structure.</returns>
        private Geometry CreateArcs(IReadOnlyList<double> angles, double radius, Point center, bool onlyDeadZones = false)
        {
            var geometry = new StreamGeometry();
            StreamGeometryContext geometryContext = geometry.Open();
            geometryContext.SetFillRule(FillRule.EvenOdd);

            var loopStart = 0;
            if (onlyDeadZones)
            {
                loopStart = 1;
            }

            for (int i = loopStart; i < angles.Count - 1; i += 2)
            {
                double start = angles[i];
                double end = angles[i + 1];
                DrawArcFigure(geometryContext, start, end, radius, center);
            }

            return geometry;
        }

        /// <summary>
        /// Update method to set the extended pseudo class.
        /// </summary>
        /// <param name="newValue">A bool indicating whether the class should be set or removed.</param>
        private void UpdatePseudoClasses(bool? newValue = null)
        {
            PseudoClasses.Set(":extended", newValue ?? selectedPosition != null || ShowAllAngles);
        }
    }
}

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
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Ship.Components;
using WoWsShipBuilder.UI.Extensions;

namespace WoWsShipBuilder.UI.CustomControls;

/// <summary>
/// Custom control for visualizing the firing angles of a ship's turrets.
/// </summary>
[PseudoClasses(":extended")]
public class FiringAngleVisualizer : TemplatedControl
{
    private const double VerticalPadding = 50.0;

    /// <summary>
    /// Styled Property for the background color of the control's area.
    /// </summary>
    public static readonly StyledProperty<SolidColorBrush> ShipBackgroundProperty =
        AvaloniaProperty.Register<FiringAngleVisualizer, SolidColorBrush>(nameof(ShipBackground), new(Colors.LightBlue, 0.5));

    /// <summary>
    /// StyledProperty for the color of the ship.
    /// </summary>
    public static readonly StyledProperty<SolidColorBrush> ShipColorProperty =
        AvaloniaProperty.Register<FiringAngleVisualizer, SolidColorBrush>(nameof(ShipColor), new(Colors.Navy, 0.95));

    /// <summary>
    /// StyledProperty for the color of ship turrets.
    /// </summary>
    public static readonly StyledProperty<SolidColorBrush> TurretColorProperty =
        AvaloniaProperty.Register<FiringAngleVisualizer, SolidColorBrush>(nameof(TurretColor), new(Colors.Gray));

    /// <summary>
    /// Styled property for the color of the turret angle visualization.
    /// </summary>
    public static readonly StyledProperty<SolidColorBrush> TurretAngleColorProperty =
        AvaloniaProperty.Register<FiringAngleVisualizer, SolidColorBrush>(nameof(TurretAngleColor), new(Colors.LightGray, 0.7));

    /// <summary>
    /// Styled property for the color of the turret dead zones in the firing angle visualization.
    /// </summary>
    public static readonly StyledProperty<SolidColorBrush> DeadZoneColorProperty =
        AvaloniaProperty.Register<FiringAngleVisualizer, SolidColorBrush>(nameof(DeadZoneColor), new(Colors.Red, 0.6));

    /// <summary>
    /// Styled property for the color of the turret dead zones.
    /// </summary>
    public static readonly StyledProperty<SolidColorBrush> TurretDeadZoneColorProperty =
        AvaloniaProperty.Register<FiringAngleVisualizer, SolidColorBrush>(nameof(TurretDeadZoneColor), new(Colors.Red, 0.4));

    /// <summary>
    /// StyledProperty for the <see cref="TurretModule"/> of the ship.
    /// </summary>
    public static readonly StyledProperty<IEnumerable<IGun>> TurretsProperty = AvaloniaProperty.Register<FiringAngleVisualizer, IEnumerable<IGun>>(nameof(Turrets));

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

    private static readonly IPen DefaultPen = new Pen();

    private readonly List<(Point textOrigin, FormattedText text)> angleTexts = new();

    private readonly List<(IGun gun, Geometry geometry, Point center)> turretGeometries = new();

    private (IGun gun, Point center, Geometry geometry)? selectedPosition;

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
    public IEnumerable<IGun> Turrets
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
        return new(x, y);
    }

    /// <summary>
    /// Helper method to check if the angle between the two values is a large or a small angle.
    /// </summary>
    /// <param name="startAngle">The start of the firing sector in degrees. Can be negative.</param>
    /// <param name="endAngle">The end of the firing sector in degrees. Should not be negative.</param>
    /// <returns><see langword="true"/> if the difference is greater than 180 degrees, <see langword="false"/> otherwise.</returns>
    private static bool IsLargeArc(double startAngle, double endAngle)
    {
        startAngle += startAngle < 0 ? 360 : 0;
        endAngle += endAngle <= 0 ? 360 : 0;
        double diff = endAngle - startAngle;
        return diff < 0 ? diff + 360 > 180 : endAngle - startAngle > 180;
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
    /// Helper method that draws a firing sector between two angles.
    /// </summary>
    /// <param name="startAngle">The start angle.</param>
    /// <param name="endAngle">The end angle.</param>
    /// <param name="radius">The radius of the arc.</param>
    /// <param name="center">The center of the turret visualization.</param>
    /// <returns>A <see cref="Geometry"/> representing the generated structure.</returns>
    private static Geometry CreateArcs(double startAngle, double endAngle, double radius, Point center)
    {
        return CreateArcs(radius, center, false, startAngle, endAngle);
    }

    /// <summary>
    /// Helper method that draws a firing sector between two angles.
    /// </summary>
    /// <param name="radius">The radius of the arc.</param>
    /// <param name="center">The center of the turret visualization.</param>
    /// <param name="onlyDeadZones">Indicates whether only the dead zones should be drawn.</param>
    /// <param name="angles">An array of angles of the firing sectors of the turret.</param>
    /// <returns>A <see cref="Geometry"/> representing the generated structure.</returns>
    private static Geometry CreateArcs(double radius, Point center, bool onlyDeadZones = false, params double[] angles)
    {
        var geometry = new StreamGeometry();
        var geometryContext = geometry.Open();
        geometryContext.SetFillRule(FillRule.EvenOdd);

        var loopStart = 0;
        if (onlyDeadZones)
        {
            loopStart = 1;
        }

        for (int i = loopStart; i < angles.Length - 1; i += 2)
        {
            double start = angles[i];
            double end = angles[i + 1];
            DrawArcFigure(geometryContext, start, end, radius, center);
        }

        return geometry;
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
        var startPoint = PolarToCartesian(startAngle, radius, center);
        if (Math.Abs(startAngle - endAngle) < 0.01)
        {
            // Required for 360 degree turrets, otherwise this will only draw a single line and not an arc
            double middleAngle = startAngle < 179 ? 180 : 0;
            var middlePoint = PolarToCartesian(middleAngle, radius, center);
            geometryContext.BeginFigureFluent(startPoint, true)
                .ArcToFluent(middlePoint, new(radius, radius), 0.0, IsLargeArc(startAngle, middleAngle), SweepDirection.Clockwise)
                .ArcToFluent(startPoint, new(radius, radius), 0.0, IsLargeArc(middleAngle, endAngle), SweepDirection.Clockwise)
                .EndFigure(true);
        }
        else
        {
            var endPoint = PolarToCartesian(endAngle, radius, center);
            geometryContext.BeginFigureFluent(center, true)
                .LineToFluent(startPoint)
                .ArcToFluent(endPoint, new(radius, radius), 0.0, IsLargeArc(startAngle, endAngle), SweepDirection.Clockwise)
                .LineToFluent(center)
                .EndFigure(true);
        }
    }

    private static double NormalizeAngle(double angle)
    {
        angle %= 360;
        if (angle > 180)
        {
            angle -= 360;
        }

        return angle;
    }

    /// <summary>
    /// Creates a transform group with scale, rotate and translate transforms to move turret or launcher icons to their target position.
    /// </summary>
    /// <param name="turretScale">The scaling to apply to the target of the transform.</param>
    /// <param name="degrees">The rotation angle in degrees.</param>
    /// <param name="center">The center for the rotation.</param>
    /// <param name="xOffset">The horizontal offset for the translation.</param>
    /// <param name="yOffset">The vertical offset for the translation.</param>
    /// <returns>A <see cref="TransformGroup"/> containing the transforms to move the geometry to its target position.</returns>
    private static TransformGroup CreateTransformGroup(double turretScale, double degrees, Point center, double xOffset, double yOffset)
    {
        var scaling = new ScaleTransform(turretScale, turretScale);
        var rotation = new RotateTransform(degrees, xOffset, yOffset);
        var translation = new TranslateTransform(center.X - xOffset, center.Y - yOffset);

        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(scaling);
        transformGroup.Children.Add(rotation);
        transformGroup.Children.Add(translation);

        return transformGroup;
    }

    /// <summary>
    /// Renders the content of the control, drawing the turrets as well as their firing angles.
    /// For more details, see <see cref="Visual.Render"/>.
    /// </summary>
    /// <param name="context">The <see cref="DrawingContext"/> used to draw the visualization.</param>
    public override void Render(DrawingContext context)
    {
        turretGeometries.Clear();
        var shipRect = CreateBasicStructure(context);
        angleTexts.Clear();
        DrawTurrets(context, shipRect);

        if (ShowAllAngles)
        {
            foreach (var (shipTurret, _, center) in turretGeometries)
            {
                DrawTurretAngles(context, shipTurret, center, shipRect);
            }
        }
        else if (selectedPosition != null)
        {
            DrawTurretAngles(context, selectedPosition.Value.gun, selectedPosition.Value.center, shipRect);
        }

        foreach (var (textOrigin, text) in angleTexts)
        {
            DrawAngleText(context, textOrigin, text);
        }
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

            var thisPosition = (gun, center, geometry);
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
        base.OnPropertyChanged(change);

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

    private Rect CreateBasicStructure(DrawingContext context)
    {
        context.FillRectangle(ShipBackground, new(Bounds.Size));
        double maxHeight = Bounds.Height - VerticalPadding;
        var shipRectSize = CalculateShipSize(maxHeight);
        double rectWidth = shipRectSize.Width;
        double horizontalOffset = (Bounds.Width - rectWidth) / 2;
        double verticalOffset = (Bounds.Height - maxHeight) / 2;
        var shipAnchorPoint = new Point(horizontalOffset, verticalOffset);

        var shipRect = new Rect(shipAnchorPoint, shipRectSize);
        var shipBase = new EllipseGeometry(shipRect);
        context.DrawGeometry(ShipColor, DefaultPen, shipBase);

        return shipRect;
    }

    /// <summary>
    /// Helper method to draw positive and negative angle indicators.
    /// </summary>
    /// <param name="context">The drawing context used to draw the indicators.</param>
    private void DrawIndicators(DrawingContext context)
    {
        var typeface = new Typeface(FontFamily, FontStyle, FontWeight.Bold);
        const int fontSize = 28;
        var plusText = new FormattedText("+", typeface, fontSize, TextAlignment.Left, TextWrapping.NoWrap, Size.Infinity);
        var minusText = new FormattedText("-", typeface, fontSize, TextAlignment.Left, TextWrapping.NoWrap, Size.Infinity);
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
    private Size CalculateShipSize(double maxHeight)
    {
        var result = new Size(maxHeight / 3.5, maxHeight);
        if (result.Width > Bounds.Width)
        {
            result = new(Bounds.Width, Bounds.Width * 4);
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
        foreach (var gun in Turrets.OrderBy(gun => gun, new TurretListComparer()))
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
    private void DrawTurret(DrawingContext context, IGun shipTurret, Rect shipRect)
    {
        double radius = CalculateRadius(shipRect);
        double maxHeight = shipRect.Height - (2 * radius);

        // Position of turrets ranges from about -1.5 to 6.5.
        // Offset first turret by value of radius to make sure that even a turret at -1.5 is rendered within the bounds of the ship.
        double verticalPosition = ((maxHeight / 8) * (decimal.ToDouble(shipTurret.VerticalPosition) + 1.5)) + (VerticalPadding / 2) + radius;

        double horizontalOffset = GetHorizontalBorderCoordinate(verticalPosition, shipRect.Center, shipRect.Size, decimal.ToDouble(shipTurret.HorizontalPosition));
        double horizontalPosition = shipTurret.HorizontalPosition switch
        {
            < 1 => horizontalOffset - 10,
            > 1 => horizontalOffset + 10,
            _ => shipRect.Left + (shipRect.Width / 2),
        };

        var center = new Point(horizontalPosition, verticalPosition);

        if (shipTurret is Gun)
        {
            DrawArtilleryTurretIcons(radius, shipTurret, center, context);
        }
        else if (shipTurret is TorpedoLauncher)
        {
            DrawTorpedoLauncherIcons(radius, shipTurret, center, context);
        }

        var startAngle = decimal.ToDouble(shipTurret.HorizontalSector[0]);
        var endAngle = decimal.ToDouble(shipTurret.HorizontalSector[1]);

        if (endAngle < 0)
        {
            endAngle += 360;
        }

        startAngle += decimal.ToDouble(shipTurret.BaseAngle);
        endAngle += decimal.ToDouble(shipTurret.BaseAngle);

        if (PermanentAngleText)
        {
            angleTexts.Add(CalculateAngleTextPosition(startAngle, endAngle, center, shipTurret, shipTurret.BaseAngle > 0));
        }
    }

    private void DrawTorpedoLauncherIcons(double radius, IGun shipTurret, Point center, DrawingContext context)
    {
        var launcherGeometry = PathGeometry.Parse("M 0,9.7895832 H 8.4666666 M 0,8.2020833 H 8.4666666 M 0,4.4979166 c 0.52916666,0 8.4666666,0 8.4666666,0 M " +
                                                  "0,1.5875 H 8.4666666 M 6.4771355,9.926714 V 1.6184604 c 0.2645833,-1.71052293 1.8520833,-1.71052293 " +
                                                  "2.1166667,0 V 9.926714 c -0.7804407,0.807197 -1.4665162,0.597218 -2.1166667,0 z m -2.1166667,0 V 1.6184604 c " +
                                                  "0.2645833,-1.71052293 1.8520833,-1.71052293 2.1166667,0 V 9.926714 c -0.6966683,0.689771 -1.4018189,0.721197 " +
                                                  "-2.1166667,0 z m -2.1166667,0 V 1.6184604 c 0.2645833,-1.71052293 1.8520833,-1.71052293 2.1166667,0 V " +
                                                  "9.926714 c -0.6925275,0.682249 -1.3971989,0.728549 -2.1166667,0 z m -2.1166666,0 V 1.6184604 c " +
                                                  "0.26458333,-1.71052296 1.8520833,-1.71052296 2.1166667,0 V 9.926714 c -0.6978745,0.691941 -1.40312901,0.719063 -2.1166667,0 z");
        launcherGeometry.FillRule = FillRule.NonZero;

        double turretScale = radius / launcherGeometry.Bounds.Width;
        double halfWidth = launcherGeometry.Bounds.Width * turretScale / 2;
        double halfHeight = launcherGeometry.Bounds.Height * turretScale / 2;

        launcherGeometry.Transform = CreateTransformGroup(turretScale, decimal.ToDouble(shipTurret.BaseAngle), center, halfWidth, halfHeight);
        turretGeometries.Add((shipTurret, launcherGeometry, center));

        var pen = new Pen
        {
            Brush = Brushes.DarkGray,
        };
        context.DrawGeometry(TurretColor, pen, launcherGeometry);
    }

    private void DrawArtilleryTurretIcons(double radius, IGun shipTurret, Point center, DrawingContext context)
    {
        var drawingGroup = new DrawingGroup();
        var turretGeometry = PathGeometry.Parse("M 9.5250002,9.6572912 8.5989582,10.451041 H 2.5135415 l -0.79375,-0.7937498 h -1.5875 v -1.322916 h " +
                                                "0.79375 V 6.7468749 c 0,-0.5291667 0.3002661,-1.3683176 0.9634748,-1.6052961 0.449291,-0.1888714 " +
                                                "0.8886086,-0.2467873 1.4177752,-0.2467873 v -4.7625 h 1.5875 v 4.7625 h 1.5875 v -4.7625 h 1.5875 v " +
                                                "4.7625 c 0.5291667,0 1.2930047,0.095838 1.8008907,0.4810755 0.4023848,0.3919374 0.5803588,1.1064245 " +
                                                "0.5803588,1.5319045 v 1.4266037 h 0.79375 v 1.322916 z");
        turretGeometry.FillRule = FillRule.NonZero;

        var turretOutlines = PathGeometry.Parse("m 6.4822914,4.8947915 v 1.0583334 h 1.5875 V 4.8947915 m -4.7624999,0 v 1.0583334 h 1.5875 V 4.8947915");
        double turretScale = radius * 2 / turretGeometry.Bounds.Width;
        double turretWidth = turretGeometry.Bounds.Width * turretScale;
        double turretHeight = turretGeometry.Bounds.Height * turretScale;

        var transformGroup = CreateTransformGroup(turretScale, decimal.ToDouble(shipTurret.BaseAngle), center, turretWidth * 0.5, turretHeight * 0.75);
        turretGeometry.Transform = transformGroup;
        turretOutlines.Transform = transformGroup;
        turretGeometries.Add((shipTurret, turretGeometry, center));

        drawingGroup.AddChild(turretGeometry, TurretColor, Brushes.DarkGray);
        drawingGroup.AddChild(turretOutlines, Brushes.Transparent, Brushes.DarkGray);
        drawingGroup.Draw(context);
    }

    /// <summary>
    /// Renders the firing sectors of a turret.
    /// </summary>
    /// <param name="context">The drawing context for the rendering.</param>
    /// <param name="shipTurret">The ship turret to draw angles for.</param>
    /// <param name="center">The center of the turret visualization.</param>
    /// <param name="shipRect">The rectangle enclosing the ship ellipse.</param>
    private void DrawTurretAngles(DrawingContext context, IGun shipTurret, Point center, Rect shipRect)
    {
        double radius = CalculateRadius(shipRect) * RadiusFactor;
        var startAngle = decimal.ToDouble(shipTurret.HorizontalSector[0] + shipTurret.BaseAngle);
        var endAngle = decimal.ToDouble(shipTurret.HorizontalSector[1] + shipTurret.BaseAngle);
        bool facingBackwards = shipTurret.BaseAngle > 0;

        startAngle = NormalizeAngle(startAngle);
        endAngle = NormalizeAngle(endAngle);

        var drawingGroup = new DrawingGroup();
        drawingGroup.AddChild(CreateArcs(startAngle, endAngle, radius, center), TurretAngleColor, new SolidColorBrush(Colors.DarkGray));

        foreach (decimal[] deadZone in shipTurret.HorizontalDeadZones ?? Array.Empty<decimal[]>())
        {
            var zoneStart = decimal.ToDouble(deadZone[0] + shipTurret.BaseAngle);
            var zoneEnd = decimal.ToDouble(deadZone[1] + shipTurret.BaseAngle);
            zoneStart = NormalizeAngle(zoneStart);
            zoneEnd = NormalizeAngle(zoneEnd);

            var arcGeometry = CreateArcs(zoneStart, zoneEnd, radius, center);
            drawingGroup.AddChild(arcGeometry, DeadZoneColor, DeadZoneColor);
        }

        drawingGroup.Draw(context);

        angleTexts.Add(CalculateAngleTextPosition(startAngle, endAngle, center, shipTurret, facingBackwards));
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
    private (Point origin, FormattedText text) CalculateAngleTextPosition(double startAngle, double endAngle, Point center, IGun shipTurret, bool facingBackwards)
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
            $"{effectiveStartAngle:0.#}° to {effectiveEndAngle:0.#}°",
            new(FontFamily, FontStyle, FontWeight),
            FontSize,
            TextAlignment.Left,
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
    /// Update method to set the extended pseudo class.
    /// </summary>
    /// <param name="newValue">A bool indicating whether the class should be set or removed.</param>
    private void UpdatePseudoClasses(bool? newValue = null)
    {
        PseudoClasses.Set(":extended", newValue ?? selectedPosition != null || ShowAllAngles);
    }
}

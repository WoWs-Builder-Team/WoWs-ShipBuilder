using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.UI.Extensions;

namespace WoWsShipBuilder.UI.CustomControls
{
    internal class DpmAngleVisualizer : TemplatedControl
    {
        /// <summary>
        /// StyledProperty for the <see cref="TurretModule"/> of the ship.
        /// </summary>
        public static readonly StyledProperty<TurretModule> TurretsProperty = AvaloniaProperty.Register<DpmAngleVisualizer, TurretModule>(nameof(Turrets));

        /// <summary>
        /// StyledProperty for the <see cref="TorpedoModule"/> of the ship.
        /// </summary>
        public static readonly StyledProperty<TorpedoModule> TorpedoProperty = AvaloniaProperty.Register<DpmAngleVisualizer, TorpedoModule>(nameof(TorpModule));

        // private readonly List<(Gun gun, Geometry geometry, Point center)> turretGeometries = new();
        static DpmAngleVisualizer()
        {
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
        /// Gets or sets the <see cref="TorpedoModule"/> of the ship that has the torpedo launchers that should be displayed.
        /// </summary>
        public TorpedoModule TorpModule
        {
            get => GetValue(TorpedoProperty);
            set => SetValue(TorpedoProperty, value);
        }

        private static EllipseGeometry GetEllipse(Point location, Size size, bool centered = true)
        {
            return centered ?
                new EllipseGeometry(new Rect(new Point(location.X - (size.Width / 2), location.Y - (size.Height / 2)), size)) :
                new EllipseGeometry(new Rect(location, size));
        }

        private static void DrawTextHelper(DrawingContext context, Point center, IBrush color, FormattedText text, int xOffset = 0, int yOffset = 0)
        {
            Point location = new Point(center.X + xOffset, center.Y + yOffset - (text.Bounds.Height / 2));
            context.DrawText(color, location, text);
        }

        private static bool IsLargeArc(double startAngle, double endAngle)
        {
            return startAngle switch
            {
                <= 0 when endAngle < 0 => Math.Abs(startAngle) + (endAngle + 360) > 180,
                < 0 when endAngle > 0 => Math.Abs(startAngle) + endAngle > 180,
                _ => endAngle - startAngle > 180,
            };
        }

        private static Point PolarToCartesian(double angle, double radius, Point center)
        {
            // Polar angle is relative to horizontal but we need vertical angle.
            angle -= 90;

            double radian = (Math.PI / 180) * angle;
            double x = center.X + (radius * Math.Cos(radian));
            double y = center.Y + (radius * Math.Sin(radian));
            return new Point(x, y);
        }

        public override void Render(DrawingContext context)
        {
            var rect = new Rect(0, 0, Bounds.Width, Bounds.Height);
            Point center = rect.Center;

            DrawBasicElements(context, center);
        }

        private void DrawBasicElements(DrawingContext context, Point center)
        {
            const int outerRingSize = 500;
            const int textOffset = 5;
            IBrush mainColor = Brushes.Chocolate;
            IBrush secondaryColor = Brushes.White;
            
            context.DrawGeometry(Brushes.Transparent, new Pen(secondaryColor, 1, DashStyle.DashDotDot), GetEllipse(center, new Size(outerRingSize, outerRingSize)));
            context.DrawGeometry(Brushes.Transparent, new Pen(secondaryColor, 1, DashStyle.DashDotDot), GetEllipse(center, new Size(outerRingSize * 0.8, outerRingSize * 0.8)));
            context.DrawGeometry(Brushes.Transparent, new Pen(secondaryColor, 1, DashStyle.DashDotDot), GetEllipse(center, new Size(outerRingSize * 0.6, outerRingSize * 0.6)));
            context.DrawGeometry(Brushes.Transparent, new Pen(secondaryColor, 1, DashStyle.DashDotDot), GetEllipse(center, new Size(outerRingSize * 0.4, outerRingSize * 0.4)));
            context.DrawGeometry(mainColor, new Pen(mainColor), GetEllipse(center, new Size(30, 100)));

            DrawTextHelper(context, center, mainColor, new FormattedText("100%", Typeface.Default, FontSize, TextAlignment.Left, TextWrapping.NoWrap, Size.Infinity), (outerRingSize / 2) + textOffset);
            DrawTextHelper(context, center, mainColor, new FormattedText("75%", Typeface.Default, FontSize, TextAlignment.Left, TextWrapping.NoWrap, Size.Infinity), (int)(outerRingSize * 0.8 / 2) + textOffset);
            DrawTextHelper(context, center, mainColor, new FormattedText("50%", Typeface.Default, FontSize, TextAlignment.Left, TextWrapping.NoWrap, Size.Infinity), (int)(outerRingSize * 0.6 / 2) + textOffset);
            DrawTextHelper(context, center, mainColor, new FormattedText("25%", Typeface.Default, FontSize, TextAlignment.Left, TextWrapping.NoWrap, Size.Infinity), (int)(outerRingSize * 0.4 / 2) + textOffset);

            var drawingGroup = new DrawingGroup();
            drawingGroup.AddChild(CreateArc(center), mainColor, secondaryColor);
            drawingGroup.Draw(context);
        }

        private Geometry CreateArc(Point center)
        {
            var geometry = new StreamGeometry();
            StreamGeometryContext geometryContext = geometry.Open();
            geometryContext.SetFillRule(FillRule.EvenOdd);
            double radius = 200;
            double startAngle = -150;
            double endAngle = 150;
            Point startPoint = PolarToCartesian(startAngle, radius, center);
            Point endPoint = PolarToCartesian(endAngle, radius, center);
            geometryContext.BeginFigureFluent(center, true)
                    .LineToFluent(startPoint)
                    .ArcToFluent(endPoint, new Size(radius, radius), 0.0, IsLargeArc(startAngle, endAngle), SweepDirection.Clockwise)
                    .LineToFluent(center)
                    .EndFigure(true);
            return geometry;
        }
    }
}

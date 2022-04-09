using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace WoWsShipBuilder.UI.CustomControls
{
    internal class DpmAnglesChart : TemplatedControl
    {
        static DpmAnglesChart()
        {
        }

        private static EllipseGeometry GibEllipse(Point location, Size size, bool centered = true)
        {
            return centered ?
                new EllipseGeometry(new Rect(new Point(location.X - (size.Width / 2), location.Y - (size.Height / 2)), size)) :
                new EllipseGeometry(new Rect(location, size));
        }

        private static void DrawTextHelper(DrawingContext context, Point center, IBrush color, FormattedText text, int xOffset = 0, int yOffset = 0)
        {
            Point location = new Point(center.X + xOffset, center.Y + yOffset);
            context.DrawText(color, location, text);
        }

        public override void Render(DrawingContext context)
        {
            var rect = new Rect(0, 0, Bounds.Width, Bounds.Height);
            Point center = rect.Center;

            DrawBasicElements(context, center);
        }

        private void DrawBasicElements(DrawingContext context, Point center)
        {
            const int firstRingSize = 200;
            const int secondRingSize = 300;
            const int thirdRingSize = 400;
            const int fourthRingSize = 500;
            IBrush mainColor = Brushes.Chocolate;
            IBrush secondaryColor = Brushes.White;

            context.DrawGeometry(Brushes.Transparent, new Pen(secondaryColor, 1, DashStyle.DashDotDot), GibEllipse(center, new Size(fourthRingSize, fourthRingSize)));
            context.DrawGeometry(Brushes.Transparent, new Pen(secondaryColor, 1, DashStyle.DashDotDot), GibEllipse(center, new Size(thirdRingSize, thirdRingSize)));
            context.DrawGeometry(Brushes.Transparent, new Pen(secondaryColor, 1, DashStyle.DashDotDot), GibEllipse(center, new Size(secondRingSize, secondRingSize)));
            context.DrawGeometry(Brushes.Transparent, new Pen(secondaryColor, 1, DashStyle.DashDotDot), GibEllipse(center, new Size(firstRingSize, firstRingSize)));
            context.DrawGeometry(mainColor, new Pen(mainColor), GibEllipse(center, new Size(30, 100)));

            DrawTextHelper(context, center, mainColor, new FormattedText("100%", Typeface.Default, FontSize, TextAlignment.Left, TextWrapping.NoWrap, Size.Infinity), fourthRingSize / 2);
            DrawTextHelper(context, center, mainColor, new FormattedText("75%", Typeface.Default, FontSize, TextAlignment.Left, TextWrapping.NoWrap, Size.Infinity), thirdRingSize / 2);
            DrawTextHelper(context, center, mainColor, new FormattedText("50%", Typeface.Default, FontSize, TextAlignment.Left, TextWrapping.NoWrap, Size.Infinity), secondRingSize / 2);
            DrawTextHelper(context, center, mainColor, new FormattedText("25%", Typeface.Default, FontSize, TextAlignment.Left, TextWrapping.NoWrap, Size.Infinity), firstRingSize / 2);
        }
    }
}

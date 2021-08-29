using Avalonia;
using Avalonia.Media;

namespace WoWsShipBuilder.UI.Extensions
{
    public static class GeometryExtensions
    {
        public static StreamGeometryContext BeginFigureFluent(this StreamGeometryContext context, Point start, bool isFilled)
        {
            context.BeginFigure(start, isFilled);
            return context;
        }

        public static StreamGeometryContext LineToFluent(this StreamGeometryContext context, Point target)
        {
            context.LineTo(target);
            return context;
        }

        public static StreamGeometryContext ArcToFluent(
            this StreamGeometryContext context,
            Point point,
            Size size,
            double rotationAngle,
            bool isLargeArc,
            SweepDirection sweepDirection)
        {
            context.ArcTo(point, size, rotationAngle, isLargeArc, sweepDirection);
            return context;
        }
    }
}

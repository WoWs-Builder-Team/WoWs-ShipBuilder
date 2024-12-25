using System.Diagnostics.Metrics;

namespace WoWsShipBuilder.Infrastructure.Metrics;

public static class MetricsExtensions
{
    public static IDisposable NewTimer(this Histogram<double> histogram)
    {
        return new Timer(histogram);
    }
}

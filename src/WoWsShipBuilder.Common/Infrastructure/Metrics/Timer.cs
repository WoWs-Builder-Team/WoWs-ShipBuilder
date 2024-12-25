using System.Diagnostics.Metrics;

namespace WoWsShipBuilder.Infrastructure.Metrics;

internal sealed class Timer : IDisposable
{
    private readonly ValueStopwatch stopwatch;

    private readonly Histogram<double> observer;

    public Timer(Histogram<double> observer)
    {
        this.stopwatch = ValueStopwatch.StartNew();
        this.observer = observer;
    }

    public void Dispose()
    {
        var elapsedTime = this.stopwatch.GetElapsedTime();
        this.observer.Record(elapsedTime.TotalSeconds);
    }
}

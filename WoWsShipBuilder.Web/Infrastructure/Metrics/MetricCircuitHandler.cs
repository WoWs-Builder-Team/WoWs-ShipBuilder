using Microsoft.AspNetCore.Components.Server.Circuits;
using Prometheus;

namespace WoWsShipBuilder.Web.Infrastructure.Metrics;

public class MetricCircuitHandler : CircuitHandler
{
    private static readonly Gauge ActiveConnectionGauge = Prometheus.Metrics.CreateGauge("active_connections", "Currently active connections.");

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        ActiveConnectionGauge.Inc();
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        ActiveConnectionGauge.Dec();
        ActiveConnectionGauge.IncTo(0); // Error-recovery in case this method gets invoked multiple times for a connection
        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }
}

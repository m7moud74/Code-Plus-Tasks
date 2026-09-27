using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace APP.Common.Diagnostics;

public static class DiagnosticsConfig
{
    public const string ServiceName = "OrderFlow.Api";

    public static readonly ActivitySource ActivitySource =
        new(ServiceName);

    public static readonly Meter Meter =
        new(ServiceName);
}
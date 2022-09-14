using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Rido.AzNorthBound;
using System.Diagnostics;

TelemetryDebugWriter.IsTracingDisabled = Debugger.IsAttached;
IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddApplicationInsightsTelemetryWorkerService();
    })
    .Build();

await host.RunAsync();

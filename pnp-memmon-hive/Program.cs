using pnp_memmon_hive;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.AddHostedService<Device>();
    })
    .Build();

await host.RunAsync();

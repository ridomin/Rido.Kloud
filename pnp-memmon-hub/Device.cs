using dtmi_rido_pnp;
using Humanizer;
using Rido.Mqtt.HubClient;
using Rido.MqttCore;
using Rido.MqttCore.PnP;

using System.Diagnostics;
using System.Text;

namespace pnp_memmon_hub;

public class Device : BackgroundService
{
    private readonly ILogger<Device> _logger;
    private readonly IConfiguration _configuration;

    private readonly Stopwatch clock = Stopwatch.StartNew();
    private int telemetryCounter = 0;
    private int commandCounter = 0;
    private int twinRecCounter = 0;
    private int reconnectCounter = 0;

    private double telemetryWorkingSet = 0;
    private const bool default_enabled = true;
    private const int default_interval = 8;

    private memmon client;

    private string lastDiscconectReason = string.Empty;

    Timer screenRefresher;
    private int uxRefresh = 1;

    public Device(ILogger<Device> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        if (_configuration["UxRefresh"] != null)
        {
            if (int.TryParse(_configuration["UxRefresh"], out uxRefresh))
            {
                _logger.LogInformation("UxRefresh: {uxRefresh}", uxRefresh);
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Connecting..");
        var cs = new ConnectionSettings(_configuration.GetConnectionString("cs"));

        if (cs.IdScope != null && _configuration["masterKey"] != null)
        {
            var deviceId = Environment.MachineName;
            var masterKey = _configuration.GetValue<string>("masterKey");
            var deviceKey = DpsKey.ComputeDeviceKey(masterKey, deviceId);
            var newCs = $"IdScope={cs.IdScope};DeviceId={deviceId};SharedAccessKey={deviceKey};SasMinutes={cs.SasMinutes}";
            client = await memmon.CreateClientAsync(newCs, stoppingToken);
        }
        else
        {
            client = await memmon.CreateClientAsync(_configuration.GetConnectionString("cs"), stoppingToken);
        }
        _logger.LogInformation("Connected");

        if (uxRefresh < 1) _logger.LogInformation(client.Connection.ConnectionSettings.ToString());

        client.Connection.OnMqttClientDisconnected += Connection_OnMqttClientDisconnected;


        client.Property_enabled.OnProperty_Updated = Property_enabled_UpdateHandler;
        client.Property_interval.OnProperty_Updated = Property_interval_UpdateHandler;
        client.Command_getRuntimeStats.OnCmdDelegate = Command_getRuntimeStats_Handler;

        await client.Property_enabled.InitPropertyAsync(client.InitialState, default_enabled, stoppingToken);
        await client.Property_interval.InitPropertyAsync(client.InitialState, default_interval, stoppingToken);

        client.Property_started.PropertyValue = DateTime.Now;
        await client.Property_started.ReportPropertyAsync(stoppingToken);

        RefreshScreen(this);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (client?.Property_enabled.PropertyValue.Value == true)
            {
                telemetryWorkingSet = Environment.WorkingSet;
                await client.Telemetry_workingSet.SendTelemetryAsync(telemetryWorkingSet, stoppingToken);
                telemetryCounter++;
            }
            var interval = client?.Property_interval.PropertyValue?.Value;
            await Task.Delay(interval.HasValue ? interval.Value * 1000 : 1000, stoppingToken);
        }
    }

    private void Connection_OnMqttClientDisconnected(object sender, Rido.MqttCore.DisconnectEventArgs e)
    {
        lastDiscconectReason = e.ReasonInfo;
        reconnectCounter++;
    }

    private async Task<PropertyAck<bool>> Property_enabled_UpdateHandler(PropertyAck<bool> p)
    {
        twinRecCounter++;
        var ack = new PropertyAck<bool>(p.Name)
        {
            Description = "desired notification accepted",
            Status = 200,
            Version = p.Version,
            Value = p.Value
        };
        client.Property_enabled.PropertyValue = ack;
        return await Task.FromResult(ack);
    }

    private async Task<PropertyAck<int>> Property_interval_UpdateHandler(PropertyAck<int> p)
    {
        ArgumentNullException.ThrowIfNull(client);
        twinRecCounter++;
        var ack = new PropertyAck<int>(p.Name);
        if (p.Value > 0)
        {
            ack.Description = "desired property accepted";
            ack.Version = p.Version;
            ack.Value = p.Value;
            ack.Status = 200;
        }
        else
        {
            ack.Description = "negative values not accepted";
            ack.Version = p.Version;
            ack.Value = p.LastReported>0 ? p.LastReported : default_interval;
            ack.Status = 406;
        }
        client.Property_interval.PropertyValue = ack;
        return await Task.FromResult(ack);
    }

    private async Task<Cmd_getRuntimeStats_Response> Command_getRuntimeStats_Handler(Cmd_getRuntimeStats_Request req)
    {
        commandCounter++;
        var result = new Cmd_getRuntimeStats_Response()
        {
            Status = 200
        };

        result.diagnosticResults.Add("machine name", Environment.MachineName);
        result.diagnosticResults.Add("os version", Environment.OSVersion.ToString());
        result.diagnosticResults.Add("started", TimeSpan.FromMilliseconds(clock.ElapsedMilliseconds).Humanize(3));
        if (req.DiagnosticsMode == DiagnosticsMode.complete)
        {
            result.diagnosticResults.Add("twin version", client.Property_started.Version.ToString());
            result.diagnosticResults.Add("this app:", System.Reflection.Assembly.GetExecutingAssembly()?.FullName ?? "");
        }
        if (req.DiagnosticsMode == DiagnosticsMode.full)
        {
            result.diagnosticResults.Add($"twin receive: ", twinRecCounter.ToString());
            result.diagnosticResults.Add($"twin sends: ", RidCounter.Current.ToString());
            result.diagnosticResults.Add("telemetry: ", telemetryCounter.ToString());
            result.diagnosticResults.Add("command: ", commandCounter.ToString());
            result.diagnosticResults.Add("reconnects: ", reconnectCounter.ToString());
        }
        return await Task.FromResult(result);
    }

    private void RefreshScreen(object state)
    {
        string RenderData()
        {
            void AppendLineWithPadRight(StringBuilder sb, string s) => sb.AppendLine(s?.PadRight(Console.BufferWidth > 1 ? Console.BufferWidth - 1 : 300));

            string enabled_value = client?.Property_enabled?.PropertyValue.Value.ToString();
            string interval_value = client?.Property_interval.PropertyValue?.Value.ToString();
            StringBuilder sb = new();
            AppendLineWithPadRight(sb, " ");
            AppendLineWithPadRight(sb, client?.Connection.ConnectionSettings?.HostName);
            AppendLineWithPadRight(sb, $"{client?.Connection.ConnectionSettings.ClientId} ({client.Connection.ConnectionSettings.Auth})");
            AppendLineWithPadRight(sb, " ");
            AppendLineWithPadRight(sb, String.Format("{0:8} | {1:15} | {2}", "Property", "Value".PadRight(15), "Version"));
            AppendLineWithPadRight(sb, String.Format("{0:8} | {1:15} | {2}", "--------", "-----".PadLeft(15, '-'), "------"));
            AppendLineWithPadRight(sb, String.Format("{0:8} | {1:15} | {2}", "enabled".PadRight(8), enabled_value?.PadLeft(15), client?.Property_enabled?.PropertyValue.Version));
            AppendLineWithPadRight(sb, String.Format("{0:8} | {1:15} | {2}", "interval".PadRight(8), interval_value?.PadLeft(15), client?.Property_interval.PropertyValue?.Version));
            AppendLineWithPadRight(sb, String.Format("{0:8} | {1:15} | {2}", "started".PadRight(8), client.Property_started.PropertyValue.ToShortTimeString().PadLeft(15), client?.Property_started?.Version));
            AppendLineWithPadRight(sb, " ");
            AppendLineWithPadRight(sb, $"Reconnects: {reconnectCounter}");
            AppendLineWithPadRight(sb, $"Telemetry: {telemetryCounter}");
            AppendLineWithPadRight(sb, $"Twin receive: {twinRecCounter}");
            AppendLineWithPadRight(sb, $"Twin send: {RidCounter.Current}");
            AppendLineWithPadRight(sb, $"Command messages: {commandCounter}");
            AppendLineWithPadRight(sb, " ");
            AppendLineWithPadRight(sb, $"WorkingSet: {telemetryWorkingSet.Bytes()}");
            AppendLineWithPadRight(sb, " ");
            AppendLineWithPadRight(sb, $"Time Running: {TimeSpan.FromMilliseconds(clock.ElapsedMilliseconds).Humanize(3)}");
            AppendLineWithPadRight(sb, $"ConnectionStatus: {client.Connection.IsConnected} [{lastDiscconectReason}]");
            AppendLineWithPadRight(sb, " ");
            return sb.ToString();
        }

        void RenderOneLiner()
        {
            _logger.LogInformation($"running for: {TimeSpan.FromMilliseconds(clock.ElapsedMilliseconds).Humanize(3)}. IsConnected: {client.Connection.IsConnected}. [{lastDiscconectReason}] ");
           _logger.LogInformation($"Reconnects: {reconnectCounter}. Telemetry: {telemetryCounter}. Twins: {twinRecCounter}. Commands: {commandCounter}");
        }

        if (uxRefresh>0)
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(RenderData());
            screenRefresher = new Timer(RefreshScreen, this, uxRefresh * 1000, 0);
        }
        else
        {
            RenderOneLiner();
            screenRefresher = new Timer(RefreshScreen, this, 5000, 0);
        }
    }
}
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Rido.Mqtt.MqttNet4Adapter;
using Rido.MqttCore;
using System.Text.Json;

namespace Rido.AzNorthBound
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private TelemetryClient _telemetryClient;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _configuration = configuration;
            _telemetryClient = telemetryClient;
        }

        int numMessages = 0;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var cs = new ConnectionSettings(_configuration.GetConnectionString("cs"));
            _telemetryClient.TrackTrace("Starting AZ Northbound connector");
            var cnx = await new MqttNetClientConnectionFactory().CreateBasicClientAsync(cs, stoppingToken);

            cnx.OnMessage += Cnx_OnMessage;

            _ = cnx.SubscribeAsync("pnp/+/telemetry", stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (cnx.IsConnected)
                {
                    _logger.LogInformation("Worker running at: {time} NumMsg {numMSg}", DateTimeOffset.Now, numMessages);
                    _telemetryClient.TrackTrace($"Worker running at: {DateTimeOffset.Now} NumMsg {numMessages}");
                }
                else
                {
                    _telemetryClient.TrackException(new ApplicationException("MQTT Client Not Connected"));
                }

                await Task.Delay(60000, stoppingToken);
            }
        }

        private async Task Cnx_OnMessage(MqttMessage m)
        {
            var segments = m.Topic.Split('/');
            var deviceId = segments[1];
            var msgType = segments[2];
            if (msgType == "telemetry")
            {
                _telemetryClient.TrackEvent("telemetry",
                    new Dictionary<string, string> { { "deviceId", deviceId } },
                    JsonSerializer.Deserialize<Dictionary<string,double>>(m.Payload));
                numMessages++;
            }
            await Task.Yield();
        }
    }
}
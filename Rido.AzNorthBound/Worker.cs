using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Humanizer;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Rido.Mqtt.MqttNet4Adapter;
using Rido.MqttCore;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Rido.AzNorthBound
{
    public class Worker : BackgroundService
    {
        Stopwatch started = Stopwatch.StartNew();

        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private TelemetryClient _telemetryClient;

        EventHubProducerClient producerClient;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _configuration = configuration;
            _telemetryClient = telemetryClient;

            producerClient = new EventHubProducerClient(configuration.GetConnectionString("eh"), configuration.GetValue<string>("eh-name"));
        }

        int numMessages = 0;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var cs = new ConnectionSettings(_configuration.GetConnectionString("cs"));
            
            _telemetryClient.TrackTrace($"Starting AZ Northbound connector, reading from broker {cs.HostName}");
            _logger.LogInformation($"Starting AZ Northbound connector, reading from broker {cs.HostName}");

            var cnx = await new MqttNetClientConnectionFactory().CreateBasicClientAsync(cs, false, stoppingToken);

            cnx.OnMessage += Cnx_OnMessage;

            _ = cnx.SubscribeAsync("pnp/+/telemetry", stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (cnx.IsConnected)
                {
                    _logger.LogWarning("Worker running for: {time} NumMsg {numMSg}", 
                        TimeSpan.FromMilliseconds(started.ElapsedMilliseconds).Humanize(3), numMessages);
                }
                else
                {
                    _telemetryClient.TrackException(new ApplicationException("MQTT Client Not Connected"));
                }

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task Cnx_OnMessage(MqttMessage m)
        {
            var segments = m.Topic.Split('/');
            var deviceId = segments[1];
            var msgType = segments[2];
            if (msgType == "telemetry")
            {
                await SendToEH(deviceId,m);
                _telemetryClient.TrackEvent("telemetry",
                    new Dictionary<string, string> { { "deviceId", deviceId } },
                    JsonSerializer.Deserialize<Dictionary<string,double>>(m.Payload));
                numMessages++;
            }
            await Task.Yield();
        }

        async Task SendToEH(string did, MqttMessage m)
        {
            var batch = await producerClient.CreateBatchAsync();
            var ed = new EventData(Encoding.UTF8.GetBytes(m.Payload));
            ed.Properties.Add("deviceId", did);
            ed.Properties.Add("modelId", "dtmi:1");
            if (batch.TryAdd(ed))
            {
                await producerClient.SendAsync(batch);
            }
        }
    }
}
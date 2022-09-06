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
            producerClient = new EventHubProducerClient(_configuration.GetConnectionString("eh"), _configuration.GetValue<string>("eh-name"));

            var cs = new ConnectionSettings(_configuration.GetConnectionString("cs"));
            
            _logger.LogWarning($"Starting AZ Northbound connector, reading from broker {cs.HostName}, writing to {producerClient.EventHubName}");

            var cnx = await new MqttNetClientConnectionFactory().CreateBasicClientAsync(cs, false, stoppingToken);

            cnx.OnMessage += Cnx_OnMessage;

            var connAck = await cnx.SubscribeAsync("pnp/+/telemetry", stoppingToken);
            if (connAck == 0)
            {
                _logger.LogWarning($"Subscribed for telemetry at broker {cnx.ConnectionSettings.HostName}");
            }

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
            numMessages++;
            var segments = m.Topic.Split('/');
            var deviceId = segments[1];
            var msgType = segments[2];
            _logger.LogInformation("New message from {0}", deviceId);
            if (msgType == "telemetry")
            {
                await SendToEH(deviceId,m);
                _telemetryClient.TrackEvent("telemetry",
                    new Dictionary<string, string> { { "deviceId", deviceId } },
                    JsonSerializer.Deserialize<Dictionary<string,double>>(m.Payload));
                await SendToEHAsync(deviceId,m);
            }
            //return Task.FromResult(0);
        }

        async Task SendToEHAsync(string did, MqttMessage m)
        {
            var batch = await producerClient.CreateBatchAsync();
            var ed = new EventData(Encoding.UTF8.GetBytes(m.Payload));
            ed.Properties.Add("deviceId", did);
            if (batch.TryAdd(ed))
            {
                await producerClient.SendAsync(batch);
            }
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
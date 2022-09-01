using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Humanizer;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Rido.Mqtt.MqttNet4Adapter;
using Rido.MqttCore;
using Rido.MqttCore.Birth;
using System.ComponentModel;
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

        Dictionary<string, string> devices = new Dictionary<string, string>();

        public Worker(ILogger<Worker> logger, IConfiguration configuration, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _configuration = configuration;
            _telemetryClient = telemetryClient;

        }

        int numMessages = 0;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            producerClient = new EventHubProducerClient(_configuration.GetConnectionString("eh"), _configuration.GetValue<string>("eh-name"));

            var cs = new ConnectionSettings(_configuration.GetConnectionString("cs"));
            
            _logger.LogWarning($"Starting AZ Northbound connector, reading from broker {cs.HostName}, writing to {producerClient.EventHubName}");

            var cnx = await new MqttNetClientConnectionFactory().CreateBasicClientAsync(cs, false, stoppingToken);

            cnx.OnMessage += Cnx_OnMessage;

            await cnx.SubscribeAsync("pnp/+/telemetry", stoppingToken);
            await cnx.SubscribeAsync("pnp/+/birth", stoppingToken);

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

                await Task.Delay(30000, stoppingToken);
            }
        }

        private async Task Cnx_OnMessage(MqttMessage m)
        {
            numMessages++;
            var segments = m.Topic.Split('/');
            var deviceId = segments[1];
            var msgType = segments[2];
            _logger.LogInformation("New message from {0}", deviceId);

            if (msgType == "birth")
            {
                var  birthMsg = JsonDocument.Parse(m.Payload);

                if (birthMsg != null)
                {
                    if (!devices.ContainsKey(deviceId))
                    {
                        string? mid = birthMsg.RootElement.GetProperty("model-id").GetString();
                        devices.Add(deviceId, mid ?? "");
                    }
                }
            }

            if (msgType == "telemetry")
            {
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
            var mid = devices[did];
            if (mid != null)
            {
                ed.Properties.Add("modelId", mid);
            }
            if (batch.TryAdd(ed))
            {
                await producerClient.SendAsync(batch);
            }
        }
    }
}
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Humanizer;
using Microsoft.ApplicationInsights;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.Connections;
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

            producerClient = new EventHubProducerClient(configuration.GetConnectionString("eh"), configuration.GetValue<string>("eh-name"));
        }

        int numMessages = 0;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            producerClient = new EventHubProducerClient(_configuration.GetConnectionString("eh"), _configuration.GetValue<string>("eh-name"));

            var cs = new ConnectionSettings(_configuration.GetConnectionString("cs"));
            
            string header = $"AZ Northbound connector, reading from broker {cs.HostName}, writing to {producerClient.EventHubName}";

            MqttClient? cnx = new MQTTnet.MqttFactory().CreateMqttClient() as MqttClient;
            await cnx!.ConnectAsync(new MqttClientOptionsBuilder().WithConnectionSettings(cs, false).Build());


            cnx.ApplicationMessageReceivedAsync += Cnx_ApplicationMessageReceivedAsync;

            await cnx.SubscribeAsync(new MqttClientSubscribeOptionsBuilder().WithTopicFilter("pnp/+/telemetry").Build(), stoppingToken);
            await cnx.SubscribeAsync(new MqttClientSubscribeOptionsBuilder().WithTopicFilter("pnp/+/birth").Build(), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (cnx.IsConnected)
                {
                    _logger.LogWarning(header);
                    _logger.LogWarning("Worker running for: {time} NumMsg {numMSg} from {deviceCount} devices", 
                        TimeSpan.FromMilliseconds(started.ElapsedMilliseconds).Humanize(3), numMessages, devices.Count);
                }
                else
                {
                    _telemetryClient.TrackException(new ApplicationException("MQTT Client Not Connected"));
                }

                await Task.Delay(5000, stoppingToken);
            }
        }

        private async Task Cnx_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            numMessages++;
            var segments = arg.ApplicationMessage.Topic.Split('/');
            var deviceId = segments[1];
            var msgType = segments[2];
            _logger.LogInformation("New message from {0}", deviceId);

            if (msgType == "birth")
            {
                //var  birthMsg = JsonDocument.Parse(arg.ApplicationMessage.Payload);
                string birthMsgJson = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
                var birthMsg = Json.FromString<BirthConvention.BirthMessage>(birthMsgJson);
                if (birthMsg?.ConnectionStatus == BirthConvention.ConnectionStatus.online)
                {
                    if (!devices.ContainsKey(deviceId))
                    {
                        string? mid = birthMsg.ModelId;
                        devices.Add(deviceId, mid ?? "");
                    }
                }
            }

            if (msgType == "telemetry")
            {
                //_telemetryClient.TrackEvent("telemetry",
                //    new Dictionary<string, string> { { "deviceId", deviceId } },
                //    JsonSerializer.Deserialize<Dictionary<string,double>>(m.Payload));
                await SendToEHAsync(deviceId,arg.ApplicationMessage);
            }
            //return Task.FromResult(0);
        }

        async Task SendToEHAsync(string did, MqttApplicationMessage m)
        {
            var jsonMsg = Encoding.UTF8.GetString(m.Payload);
            var modelId = devices[did];
            string ehJsonMsg = string.Empty;

            MemmonSchema? memmonTelemetry;
            if (modelId == "dtmi:rido:pnp:memmon;1")
            {
                memmonTelemetry = JsonSerializer.Deserialize<MemmonSchema>(jsonMsg);

                _telemetryClient.TrackEvent("telemetry",
                    new Dictionary<string, string> { { "deviceId", did} }, 
                    new Dictionary<string, double> { { "workingSet", memmonTelemetry!.workingSet } }
                );

                memmonTelemetry!.DeviceId = did;
                memmonTelemetry!.ModelId = modelId;
                ehJsonMsg = JsonSerializer.Serialize(memmonTelemetry);
            }

            PiSenseHatSchema? senseHatSchema;
            if (modelId == "dtmi:rido:pnp:sensehat;1")
            {
                senseHatSchema = JsonSerializer.Deserialize<PiSenseHatSchema>(jsonMsg);

                _telemetryClient.TrackEvent("telemetry",
                    new Dictionary<string, string> { { "deviceId", did } },
                    new Dictionary<string, double> { 
                        { "t1", senseHatSchema!.t1},
                        { "t2", senseHatSchema!.t2},
                        { "h", senseHatSchema!.h},
                        { "m", senseHatSchema!.m}
                    }
                );

                senseHatSchema!.DeviceId = did;
                senseHatSchema!.ModelId = modelId;
                ehJsonMsg = JsonSerializer.Serialize(senseHatSchema);
            }
                       

            var batch = await producerClient.CreateBatchAsync();
            var ed = new EventData(ehJsonMsg);
            
            if (batch.TryAdd(ed))
            {
                await producerClient.SendAsync(batch);
            }
        }
    }
}
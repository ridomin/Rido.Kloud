using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient.TopicBindings;
using MQTTnet.Extensions.MultiCloud;
using smart_lightbulb_winforms;
using System.Text.Json;

namespace smart_lightbulb_winforms_hive
{
 

    internal class smartlightbulb : PnPMqttClient, Ismartlightbulb
    {
        const string modelId = "dtmi:pnd:demo:smartlightbulb;1";
        public ITelemetry<int> Telemetry_batteryLife { get; set; }
        public IReadOnlyProperty<DateTime> Property_lastBatteryReplacement { get; set; }
        public IWritableProperty<int> Property_lightState { get; set; }

        public smartlightbulb(IMqttClient connection) : base(connection)
        {
            Telemetry_batteryLife = new Telemetry<int>(connection, "batteryLife");
            Property_lastBatteryReplacement = new ReadOnlyProperty<DateTime>(connection, "lastBatteryReplacement");
            Property_lightState = new WritableProperty<int>(connection, "lightState");
        }

        public static async Task<smartlightbulb> CreateClientAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            var cs = connectionString + ";ModelId=" + modelId;
            var hive = await BrokerClientFactory.CreateFromConnectionSettingsAsync(connectionString, true, cancellationToken);
            var client = new smartlightbulb(hive);
            client.InitialState = JsonSerializer.Serialize(new { desired = "", reported = ""});// await client.GetTwinAsync(cancellationToken);
            return client;
        }

    }
}

using MQTTnet.Client;
using Rido.Mqtt.Client;
using Rido.Mqtt.Client.TopicBindings;
using Rido.MqttCore;
using Rido.MqttCore.PnP;
using smart_lightbulb_winforms;
using System.Text.Json;

namespace smart_lightbulb_winforms_hive
{
 

    internal class smartlightbulb : PnPClient, Ismartlightbulb
    {
        const string modelId = "dtmi:pnd:demo:smartlightbulb;1";
        public string InitialState { get; set; }
        public ITelemetry<int> Telemetry_batteryLife { get; set; }
        public IReadOnlyProperty<DateTime> Property_lastBatteryReplacement { get; set; }
        public IWritableProperty<int> Property_lightState { get; set; }
        IMqttBaseClient Ismartlightbulb.Connection { get => base.Connection;  }

        public smartlightbulb(IMqttBaseClient connection) : base(connection)
        {
            Telemetry_batteryLife = new Telemetry<int>(connection, "batteryLife");
            Property_lastBatteryReplacement = new ReadOnlyProperty<DateTime>(connection, "lastBatteryReplacement");
            Property_lightState = new WritableProperty<int>(connection, "lightState");
        }

        public static async Task<smartlightbulb> CreateClientAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            var cs = connectionString + ";ModelId=" + modelId;
            var hive = await new Rido.Mqtt.MqttNet4Adapter.MqttNetClientConnectionFactory().CreateBasicClientAsync(ConnectionSettings.FromConnectionString(cs));
            var client = new smartlightbulb(hive);
            client.InitialState = JsonSerializer.Serialize(new { desired = "", reported = ""});// await client.GetTwinAsync(cancellationToken);
            return client;
        }

    }
}

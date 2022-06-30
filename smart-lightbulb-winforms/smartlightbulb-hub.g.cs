using MQTTnet.Client;
using Rido.Mqtt.AzIoTClient;
using Rido.Mqtt.HubClient;
using Rido.Mqtt.HubClient.TopicBindings;
using Rido.MqttCore;
using Rido.MqttCore.PnP;
using smart_lightbulb_winforms;

namespace smart_lightbulb_winforms_hub
{

    internal class smartlightbulb : HubMqttClient, Ismartlightbulb
    {
        const string modelId = "dtmi:pnd:demo:smartlightbulb;1";
        public string InitialState { get; set; }

        public ITelemetry<int> Telemetry_batteryLife { get; set; }
        public IReadOnlyProperty<DateTime> Property_lastBatteryReplacement { get; set; }
        public IWritableProperty<int> Property_lightState { get; set; }

        public smartlightbulb(IMqttBaseClient connection) : base(connection)
        {
            Telemetry_batteryLife = new Telemetry<int>(connection, "batteryLife");
            Property_lastBatteryReplacement = new ReadOnlyProperty<DateTime>(connection, "lastBatteryReplacement");
            Property_lightState = new WritableProperty<int>(connection, "lightState");
        }

        public static async Task<smartlightbulb> CreateClientAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            var cs = connectionString + ";ModelId=" + modelId;
            var hub = await HubDpsFactory.CreateFromConnectionStringAsync(cs);
            var client = new smartlightbulb(hub.Connection);
            client.InitialState = await client.GetTwinAsync(cancellationToken);
            return client;
        }

    }
}

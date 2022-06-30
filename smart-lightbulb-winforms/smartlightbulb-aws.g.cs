using MQTTnet.Client;
using Rido.Mqtt.AwsClient;
using Rido.Mqtt.AwsClient.TopicBindings;
using Rido.Mqtt.MqttNet4Adapter;
using Rido.MqttCore;
using Rido.MqttCore.PnP;
using smart_lightbulb_winforms;

namespace smart_lightbulb_winforms_aws
{

    internal class smartlightbulb : AwsMqttClient, Ismartlightbulb
    {
        const string modelId = "dtmi:pnd:demo:smartlightbulb;1";

        public ITelemetry<int> Telemetry_batteryLife { get; set; }
        public IReadOnlyProperty<DateTime> Property_lastBatteryReplacement { get; set; }
        public IWritableProperty<int> Property_lightState { get; set; }
        public string InitialState { get; set; }

        public smartlightbulb(IMqttBaseClient connection) : base(connection)
        {
            Telemetry_batteryLife = new Telemetry<int>(connection, "batteryLife");
            Property_lastBatteryReplacement = new ReadOnlyProperty<DateTime>(connection, "lastBatteryReplacement");
            Property_lightState = new WritableProperty<int>(connection, "lightState");
        }

        public static async Task<smartlightbulb> CreateClientAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            
            var connection = await new MqttNetClientConnectionFactory().CreateAwsClientAsync(ConnectionSettings.FromConnectionString(connectionString), cancellationToken);
            var client = new smartlightbulb(connection);
            client.InitialState = await client.GetShadowAsync(cancellationToken);
            return client;
        }

    }
}

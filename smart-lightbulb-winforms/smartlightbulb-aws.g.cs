using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.AwsIoTClient;
using MQTTnet.Extensions.MultiCloud.AwsIoTClient.TopicBindings;
using MQTTnet.Extensions.MultiCloud.Clients;
using smart_lightbulb_winforms;

namespace smart_lightbulb_winforms_aws
{

    internal class smartlightbulb : AwsMqttClient, Ismartlightbulb
    {
        

        public ITelemetry<int> Telemetry_batteryLife { get; set; }
        public IReadOnlyProperty<DateTime> Property_lastBatteryReplacement { get; set; }
        public IWritableProperty<int> Property_lightState { get; set; }
        public string InitialState { get; set; }

        public smartlightbulb(IMqttClient connection) : base(connection)
        {
            Telemetry_batteryLife = new Telemetry<int>(connection, "batteryLife");
            Property_lastBatteryReplacement = new ReadOnlyProperty<DateTime>(connection, "lastBatteryReplacement");
            Property_lightState = new WritableProperty<int>(connection, "lightState");
        }

        public static async Task<smartlightbulb> CreateClientAsync(string connectionString, CancellationToken cancellationToken = default)
        {

            var connection = await AwsClientFactory.CreateFromConnectionSettingsAsync(connectionString, cancellationToken);
            var client = new smartlightbulb(connection);
            client.InitialState = await client.GetShadowAsync(cancellationToken);
            return client;
        }

    }
}

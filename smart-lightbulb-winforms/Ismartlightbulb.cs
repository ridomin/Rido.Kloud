
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Clients;

namespace smart_lightbulb_winforms
{
    internal enum LightStateEnum
    {
        On = 1,
        Off = 0
    }

    internal interface Ismartlightbulb 
    {
        const string modelId = "dtmi:pnd:demo:smartlightbulb;1";
        public string InitialState { get; set; }
        public IMqttClient Connection { get; }
        IReadOnlyProperty<DateTime> Property_lastBatteryReplacement { get; set; }
        IWritableProperty<int> Property_lightState { get; set; }
        ITelemetry<int> Telemetry_batteryLife { get; set; }
    }
}
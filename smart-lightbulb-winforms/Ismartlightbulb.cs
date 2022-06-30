using Rido.MqttCore;
using Rido.MqttCore.PnP;

namespace smart_lightbulb_winforms
{
    internal enum LightStateEnum
    {
        On = 1,
        Off = 0
    }

    internal interface Ismartlightbulb 
    {
        public string InitialState { get; set; }
        public IMqttBaseClient Connection { get; }
        IReadOnlyProperty<DateTime> Property_lastBatteryReplacement { get; set; }
        IWritableProperty<int> Property_lightState { get; set; }
        ITelemetry<int> Telemetry_batteryLife { get; set; }
    }
}
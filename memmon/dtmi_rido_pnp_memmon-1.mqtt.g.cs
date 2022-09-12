﻿//  <auto-generated/> 


using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient.TopicBindings;
using MQTTnet.Extensions.MultiCloud;

namespace dtmi_rido_pnp_memmon.mqtt;

public class memmon : PnPMqttClient, Imemmon
{
    public IReadOnlyProperty<DateTime> Property_started { get; set; }
    public IWritableProperty<bool> Property_enabled { get; set; }
    public IWritableProperty<int> Property_interval { get; set; }
    public ITelemetry<double> Telemetry_workingSet { get; set; }
    public ICommand<Cmd_getRuntimeStats_Request, Cmd_getRuntimeStats_Response> Command_getRuntimeStats { get; set; }

    internal memmon(IMqttClient c) : base(c, Imemmon.ModelId)
    {
        Property_started = new ReadOnlyProperty<DateTime>(c, "started");
        Property_interval = new WritableProperty<int>(c, "interval");
        Property_enabled = new WritableProperty<bool>(c, "enabled");
        Telemetry_workingSet = new Telemetry<double>(c, "workingSet");
        Command_getRuntimeStats = new Command<Cmd_getRuntimeStats_Request, Cmd_getRuntimeStats_Response>(c, "getRuntimeStats");
    }
}
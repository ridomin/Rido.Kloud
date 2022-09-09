﻿using dtmi_rido_pnp_memmon;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.Connections;
using MQTTnet.Extensions.IoTHubClient;
using MQTTnet.Extensions.PnPClient;

namespace memmon;

internal class MemMonFactory
{
    internal static string ComputeDeviceKey(string masterKey, string deviceId) =>
            Convert.ToBase64String(new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(masterKey)).ComputeHash(System.Text.Encoding.UTF8.GetBytes(deviceId)));

    IConfiguration _configuration;

    internal static ConnectionSettings connectionSettings;

    public MemMonFactory(IConfiguration configuration)
    {
        this._configuration = configuration;
    }

    public async Task<Imemmon> CreateMemMonClientAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connectionString, nameof(connectionString));
        connectionSettings = new ConnectionSettings(_configuration.GetConnectionString("cs"));
        if (connectionString.Contains("IdScope") || connectionString.Contains("SharedAccessKey"))
        {
            if (connectionSettings.IdScope != null && _configuration["masterKey"] != null)
            {
                var deviceId = Environment.MachineName;
                var masterKey = _configuration.GetValue<string>("masterKey");
                var deviceKey = ComputeDeviceKey(masterKey, deviceId);
                var newCs = $"IdScope={connectionSettings.IdScope};DeviceId={deviceId};SharedAccessKey={deviceKey};SasMinutes={connectionSettings.SasMinutes}";
                return await CreateHubClientAsync(newCs, cancellationToken);
            }
            else
            {
                return await CreateHubClientAsync(connectionString, cancellationToken);
            }
        }
        else if (connectionSettings.HostName.Contains("amazonaws.com"))
        {
            return await CreateAwsClientAsync(connectionString, cancellationToken);
        }
        else
        {
            return await CreateBrokerClientAsync(connectionString, cancellationToken);
        }    
    }

    static async Task<dtmi_rido_pnp_memmon.mqtt.memmon> CreateBrokerClientAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var cs = new ConnectionSettings(connectionString) { ModelId = Imemmon.ModelId };
        //var mqtt = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger()) as MqttClient;
        //await mqtt.ConnectAsync(new MqttClientOptionsBuilder().WithConnectionSettings(cs, true).Build());
        var mqtt = await BrokerClientFactory.CreatePnPBrokerClientAsync(cs, true, cancellationToken);
        var client = new dtmi_rido_pnp_memmon.mqtt.memmon(mqtt.Connection);
        return client;
    }

    static async Task<dtmi_rido_pnp_memmon.hub.memmon> CreateHubClientAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var cs = connectionString + ";ModelId=" + Imemmon.ModelId;
        var hub = await HubDpsFactory.CreateFromConnectionStringAsync(cs);
        connectionSettings = HubDpsFactory.ConnectionSettings;
        var client = new dtmi_rido_pnp_memmon.hub.memmon(hub.Connection);
        client.InitialState = await client.GetTwinAsync(cancellationToken);
        return client;
    }

    static async Task<dtmi_rido_pnp_memmon.aws.memmon> CreateAwsClientAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var cs = new ConnectionSettings(connectionString) { ModelId = Imemmon.ModelId };
        var mqtt = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger()) as MqttClient;
        await mqtt.ConnectAsync(new MqttClientOptionsBuilder().WithConnectionSettings(cs).Build());
        var client = new dtmi_rido_pnp_memmon.aws.memmon(mqtt);
        return client;
    }
}

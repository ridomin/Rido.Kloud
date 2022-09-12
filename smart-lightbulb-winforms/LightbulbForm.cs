using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.Connections;
using System.Drawing.Design;

namespace smart_lightbulb_winforms;

public partial class LightbulbForm : Form
{
    //const string cs = "IdScope=0ne003861C6;Auth=X509;X509key=cert.pfx|1234";
    //const string cs = "HostName=a38jrw6jte2l2x-ats.iot.us-west-1.amazonaws.com;ClientId=bulb1;Auth=X509;X509Key=cert.pfx|1234";
    //const string cs = "IdScope=0ne004CB66B;Auth=X509;X509key=cert.pfx|1234";

    ConnectionSettings connectionSettings;

    CloudSelecterForm cloudSelecterForm;

    Ismartlightbulb? client;
    int currentBattery = 100;

    public LightbulbForm()
    {
        InitializeComponent();
    }

    private async void Form1_Load(object sender, EventArgs e)
    {
        
        cloudSelecterForm = new CloudSelecterForm();
        if (cloudSelecterForm.ShowDialog() == DialogResult.OK)
        {
            await RunDevice(cloudSelecterForm.ConnectionString, cloudSelecterForm.CloudType);
        }
        else
        {
            MessageBox.Show("Invalid Connection Settings", "InvalidConnectionSettings");
        }    


    }

    private async Task RunDevice(string connectionString, CloudType cloud)
    {
        connectionSettings = new ConnectionSettings(connectionString);
        switch (cloud)
        {
            case CloudType.IoTHubDps:
                client = await smart_lightbulb_winforms_hub.smartlightbulb.CreateClientAsync(connectionString) ;
                break;
            case CloudType.AWS:
                client = await smart_lightbulb_winforms_aws.smartlightbulb.CreateClientAsync(connectionString);
                break;
            case CloudType.IoTHub:
                client = await smart_lightbulb_winforms_hub.smartlightbulb.CreateClientAsync(connectionString);
                break;
            case CloudType.Hive:
                client = await smart_lightbulb_winforms_hive.smartlightbulb.CreateClientAsync(connectionString);
                 break;
        }

        client.Connection.DisconnectedAsync += d => UpdateUI();

        buttonConnectDisconnect.Text = "Disconnect";

        if (Properties.Settings.Default.battery > 0)
        {
            currentBattery = Properties.Settings.Default.battery;
        }

        //labelStatus.Text = $"{client.Connection.Options.ClientId} connected to {client.Connection}";
        client.Property_lightState.OnProperty_Updated = Property_lightState_UpdateHandler;

        await client.Property_lightState.InitPropertyAsync(client.InitialState,1);

        client.Property_lastBatteryReplacement.PropertyValue = DateTime.Now;
        await client.Property_lastBatteryReplacement.ReportPropertyAsync();

        await UpdateUI();

        while (client.Connection.IsConnected)
        {
            if (currentBattery < 1)
            {
                client.Property_lightState.PropertyValue = new PropertyAck<int>(client.Property_lightState.PropertyValue.Name)
                {
                    Value = 0, //LightStateEnum.Off,
                    Description = "battery ended",
                    Status = 203,
                    Version = 0
                };
                await client.Property_lightState.ReportPropertyAsync();
                _ = UpdateUI().ConfigureAwait(false);

            }

            if (client.Property_lightState.PropertyValue.Value.Equals(1))
            {
                await client.Telemetry_batteryLife.SendTelemetryAsync(currentBattery--);
                progressBar1.Value = currentBattery;
            }
            await Task.Delay(1000);
        }
    }

    

    private async void ButtonOnOff_Click(object sender, EventArgs e)
    {
        ArgumentNullException.ThrowIfNull(client);
        Toggle();
        await UpdateUI();
        var ack = new PropertyAck<int>(client.Property_lightState.PropertyValue.Name)
        {
            Description = "Changed by user",
            Status = 203,
            Value = client.Property_lightState.PropertyValue.Value,
            Version = 0
        };
        client.Property_lightState.PropertyValue = ack;
        await client.Property_lightState.ReportPropertyAsync();
    }

    private void Toggle()
    {
        ArgumentNullException.ThrowIfNull(client);
        if (client.Property_lightState.PropertyValue.Value == 0 /*LightStateEnum.Off */)
        {
            client.Property_lightState.PropertyValue.Value = 1;
        } 
        else
        {
            client.Property_lightState.PropertyValue.Value = 0;
        }
         
    }

    private async Task UpdateUI()
    {
        ArgumentNullException.ThrowIfNull(client);
        string selectedImg = string.Empty;
        string selectedText = string.Empty; 
        string connectedText = string.Empty;
        string buttonConnectText = string.Empty;
        if (client.Connection.IsConnected)
        {
            buttonConnectText = "Disconnect";
            connectedText = $"{connectionSettings}";
        } 
        else
        {
            buttonConnectText = "Connect";
            connectedText = "Disconnected";
        }

        if (client.Property_lightState.PropertyValue.Value == 0)
        {
            selectedText = "Turn On";
            selectedImg =  "Off.jpg";
            

        }
        else
        {
            selectedText = "Turn Off";
            selectedImg = "On.jpg";
        }

        if (this.InvokeRequired)
        {
            this.Invoke(() =>
            {
                labelStatus.Text = connectedText;
                buttonConnectDisconnect.Text = buttonConnectText;
                buttonOnOff.Text = selectedText;
                pictureBoxLightbulb.ImageLocation = selectedImg;
                buttonOnOff.Enabled = client.Connection.IsConnected;
                buttonReplaceBateries.Enabled = client.Connection.IsConnected;
                progressBar1.Enabled = client.Connection.IsConnected;
            });
        }
        else
        {
            labelStatus.Text = connectedText;
            buttonConnectDisconnect.Text = buttonConnectText;
            buttonOnOff.Text = selectedText;
            pictureBoxLightbulb.ImageLocation = selectedImg;
            buttonOnOff.Enabled = client.Connection.IsConnected;
            buttonReplaceBateries.Enabled = client.Connection.IsConnected;
            progressBar1.Enabled = client.Connection.IsConnected;
        }
        await Task.Yield();
    }

    private async Task<PropertyAck<int>> Property_lightState_UpdateHandler(PropertyAck<int> p)
    {
        ArgumentNullException.ThrowIfNull(client);
        client.Property_lightState.PropertyValue.Value = p.Value;
        
        await UpdateUI();
        var ack = new PropertyAck<int>(p.Name)
        {
            Description = "light state Accepted",
            Status = 200,
            Value = p.Value,
            Version = p.Version
        };
        return await Task.FromResult(ack);
    }

    private async void buttonReplaceBateries_Click(object sender, EventArgs e)
    {
        ArgumentNullException.ThrowIfNull(client);
        currentBattery = 100;
        progressBar1.Value = currentBattery;
        client.Property_lastBatteryReplacement.PropertyValue = DateTime.Now;
        await client.Property_lastBatteryReplacement.ReportPropertyAsync();
    }

    private void LightbulbForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        Properties.Settings.Default.battery = currentBattery;
        Properties.Settings.Default.Save();
    }

    private void labelStatus_Click(object sender, EventArgs e)
    {
        Properties.Settings.Default.hostname = "";
        Properties.Settings.Default.battery = 100;
        currentBattery = 100;
        Properties.Settings.Default.Save();
    }

    private async void buttonConnectDisconnect_Click(object sender, EventArgs e)
    {
        if (client.Connection.IsConnected)
        {
            await client.Connection.DisconnectAsync();
            await UpdateUI();
        }
        else
        {
            await RunDevice(cloudSelecterForm.ConnectionString, cloudSelecterForm.CloudType);
            await UpdateUI();
        }
    }

    private async void buttonChangeCloud_Click(object sender, EventArgs e)
    {
        if (client.Connection.IsConnected)
        {
            await client.Connection.DisconnectAsync();
        }
        await UpdateUI();

        if (cloudSelecterForm.ShowDialog() == DialogResult.OK)
        {
            await RunDevice(cloudSelecterForm.ConnectionString, cloudSelecterForm.CloudType);
        }
        else
        {
            MessageBox.Show("Invalid Connection Settings", "InvalidConnectionSettings");
        }

    }
}

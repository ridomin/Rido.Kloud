using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace IoTUX.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        readonly ServiceClient sc;
        readonly DigitalTwinClient dc;
        public CommandController(IConfiguration config)
        {
            _configuration = config;
            var cs = config.GetConnectionString("hub");
            if (config["hubName"] != null)
            {
                var host = config.GetValue<string>("hubName");
                dc = DigitalTwinClient.Create(host, new DefaultAzureCredential());
            }
            else if (!string.IsNullOrEmpty(cs))
            {
                dc = DigitalTwinClient.CreateFromConnectionString(config.GetConnectionString("hub"));
            }
            else
            {
                throw new ApplicationException("connectionsString_hub neither hostName found in configuration.");
            }
            
        }

        [HttpPost("{deviceId}")]
        public async Task<string> Invoke(string deviceId, string cmdName, [FromBody] object value)
        {
            var res = await dc.InvokeCommandAsync(deviceId, cmdName, JsonSerializer.Serialize(value));
            return res.Body.Payload;
            //CloudToDeviceMethod c2d = new CloudToDeviceMethod(cmdName);
            //c2d.SetPayloadJson(JsonSerializer.Serialize(value));
            
            //var c2dRes = await sc.InvokeDeviceMethodAsync(deviceId, c2d);
            //var resJson = c2dRes.GetPayloadAsJson();
            //return c2dRes.GetPayloadAsJson();
        }
    }
}

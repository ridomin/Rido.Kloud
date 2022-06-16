using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
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
            //sc = ServiceClient.CreateFromConnectionString(config.GetConnectionString("hub"));
            dc = DigitalTwinClient.CreateFromConnectionString(config.GetConnectionString("hub"));
            //rm = RegistryManager.Create(config.GetValue<string>("hubName"), new DefaultAzureCredential());
        }

        [HttpPost("{deviceId}")]
        public async Task<string> Invoke(string deviceId, string cmdName, [FromBody] string value)
        {
            var res = await dc.InvokeCommandAsync(deviceId, cmdName, value);
            return res.Body.Payload;
            //CloudToDeviceMethod c2d = new CloudToDeviceMethod(cmdName);
            //c2d.SetPayloadJson(JsonSerializer.Serialize(value));
            
            //var c2dRes = await sc.InvokeDeviceMethodAsync(deviceId, c2d);
            //var resJson = c2dRes.GetPayloadAsJson();
            //return c2dRes.GetPayloadAsJson();
        }
    }
}

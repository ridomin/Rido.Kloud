using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IoTUX.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        readonly RegistryManager rm;

        public DevicesController(IConfiguration config)
        {
            _configuration = config;
            rm = RegistryManager.CreateFromConnectionString(config.GetConnectionString("hub"));
            //rm = RegistryManager.Create(config.GetValue<string>("hubName"), new DefaultAzureCredential());
        }

        // GET: api/<DevicesController>
        [HttpGet]
        public async Task<IEnumerable<DeviceInfo>> Get()
        {
            var q = rm.CreateQuery("SELECT * FROM devices", 100);
            var twinsRaw = await q.GetNextAsTwinAsync();
            return twinsRaw
                .OrderByDescending(t => t.LastActivityTime)
                .Select(t => new DeviceInfo
                {  
                    DeviceId = t.DeviceId, 
                    ModelId = t.ModelId, 
                    Status = t.Status.Value, 
                    AuthenticationType =  t.AuthenticationType.Value, 
                    LastActivityTime =  t.LastActivityTime.Value, 
                    Version = t.Version.Value,
                    ReportedVersion = t.Properties.Reported.Version,
                    DesiredVersion = t.Properties.Desired.Version
                });
            //return new string[] { "value1", "value2" };
        }

        // GET api/<DevicesController>/5
        //[HttpGet("{id}")]
        //public async Task<Device> Get(string id)
        //{
        //    return await rm.GetDeviceAsync(id);
        //}

        [HttpGet("{id}")]
        public async Task<string> GetTwin(string id)
        {
            return (await rm.GetTwinAsync(id)).ToJson();
        }

        // DELETE api/<DevicesController>/5
        [HttpDelete("{id}")]
        public async Task Delete(string id)
        {
            await rm.RemoveDeviceAsync(await rm.GetDeviceAsync(id));
        }
    }

    public class DeviceInfo
    {
        public string DeviceId { get; set; }
        public string ModelId { get; set; }
        public DateTime LastActivityTime{ get; set; }
        public long Version { get; set; }
        public long ReportedVersion { get; set; }
        public long DesiredVersion { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DeviceStatus Status { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AuthenticationType AuthenticationType { get; set; }
    }
}

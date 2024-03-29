﻿using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
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
            var cs = config.GetConnectionString("hub");
            if (config["hubName"] != null)
            {
                var host = config.GetValue<string>("hubName");
                rm = RegistryManager.Create(host, new DefaultAzureCredential());
            }
            else if (!string.IsNullOrEmpty(cs))
            {
                rm = RegistryManager.CreateFromConnectionString(config.GetConnectionString("hub"));
            }
            else
            {
                throw new ApplicationException("connectionsString_hub neither hostName found in configuration.");
            }    
        }

        // GET: api/<DevicesController>
        [HttpGet]
        public async Task<IEnumerable<DeviceInfo>> Get()
        {
            var q = rm.CreateQuery("SELECT * FROM devices", 100);
            var twinsRaw = await q.GetNextAsTwinAsync();
            return twinsRaw
                .Select(t => new DeviceInfo
                {
                    DeviceId = t.DeviceId,
                    ModelId = t.ModelId,
                    Status = t.Status.Value,
                    State = t.ConnectionState.Value.ToString(),
                    AuthenticationType = t.AuthenticationType.Value,
                    LastActivityTime = t.LastActivityTime.Value,
                    Version = t.Version.Value,
                    ReportedVersion = t.Properties.Reported.Version,
                    DesiredVersion = t.Properties.Desired.Version
                })
                .OrderByDescending(t => t.LastActivityTime)
                .OrderBy(t => t.State);
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

        [HttpPost("{id}")]
        public async Task UpdateTwin(string id, [FromBody]object twinValue)
        {
            Console.WriteLine(twinValue);
            //var twinPatch = new Twin();
            //twinPatch.Properties.Desired[propName] = JsonSerializer.Deserialize<int>(twinValue);
            //var twinPatch = await twinValue.Content.ReadAsStringAsync();
            var twin = await rm.GetTwinAsync(id);
            await rm.UpdateTwinAsync(id, twinValue.ToString(), twin.ETag);
        }
    }

    public class DeviceInfo
    {
        public string DeviceId { get; set; }
        public string ModelId { get; set; }
        public string State { get; set; }
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

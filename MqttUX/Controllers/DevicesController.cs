using Microsoft.AspNetCore.Mvc;
using Rido.Mqtt.MqttNet4Adapter;
using Rido.MqttCore;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MqttUX.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        static Dictionary<string, Dictionary<string, object>> devices = new Dictionary<string, Dictionary<string, object>>();
        IMqttBaseClient mqtt;
        public DevicesController(IConfiguration config)
        {
            mqtt = new MqttNetClientConnectionFactory().CreateBasicClientAsync(new ConnectionSettings(config.GetConnectionString("broker"))).Result;
            mqtt.OnMessage += async m =>
            {
                var segments = m.Topic.Split('/');
                var did = segments[1];
                if (devices.ContainsKey(did))
                {
                    devices.Remove(did);
                }
                devices.Add(did, JsonSerializer.Deserialize<Dictionary<string, object>>(m.Payload));
                await Task.Yield();
            };
            mqtt.SubscribeAsync("pnp/+/birth").Wait();
        }
        // GET: api/<ValuesController>
        [HttpGet]
        public IDictionary<string, Dictionary<string, object>> Get()
        {
            return devices;
        }

        // GET api/<ValuesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ValuesController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ValuesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ValuesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

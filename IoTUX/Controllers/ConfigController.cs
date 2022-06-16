using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IoTUX.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public ConfigController(IConfiguration config)
        {
            _configuration = config;
        }
        [HttpGet]
        public string Get()
        {
            string host = string.Empty;
            var cs = _configuration.GetConnectionString("hub");
            if (_configuration["hubName"] != null)
            {
                host = _configuration.GetValue<string>("hubName");
            }
            else if (!string.IsNullOrEmpty(cs))
            {
                var segments = cs.Split(';');
                var hostname = segments[0].Split('=');
                host =  hostname[1];
                
            }
            else
            {
                throw new ApplicationException("connectionsString_hub neither hostName found in configuration.");
            }
            return host;
        }
    }
}

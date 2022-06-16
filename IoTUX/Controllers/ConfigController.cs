using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

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
            var cs = _configuration.GetConnectionString("hub");
            var segments = cs.Split(';');
            var hostname = segments[0].Split('=');
            return hostname[1];
        }
    }
}

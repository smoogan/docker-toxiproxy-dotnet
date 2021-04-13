using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Toxiproxy.Net;
using Toxiproxy.Net.Toxics;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RedisController : ControllerBase
    {
        private readonly ILogger<RedisController> _logger;

        public RedisController(ILogger<RedisController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public long RedisTest()
        {
            using (var connection = new Connection("proxy", true))
            {
                var client = connection.Client();
                var proxy = client.FindProxy("apiToRedis");

                var latencyToxic = new LatencyToxic();
                latencyToxic.Attributes.Latency = 50;
                latencyToxic.Attributes.Jitter = 5;
                latencyToxic.Toxicity = 1.0;

                proxy.Add(latencyToxic);
                proxy.Update();

                ConnectionMultiplexer muxer =
                    ConnectionMultiplexer.Connect("proxy:22220");
                IDatabase conn = muxer.GetDatabase();

                var testVal = conn.StringIncrement("test");
                return testVal;
            }
        }
    }
}

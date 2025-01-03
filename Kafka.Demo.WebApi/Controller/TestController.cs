using Kafka.Demo.Lib.Producer;
using Kafka.Demo.WebApi.Message;
using Microsoft.AspNetCore.Mvc;

namespace Kafka.Demo.WebApi.Controller;

[ApiController]
[Route("api/[controller]")]
public class TestController(IKafkaProducer kafkaProducer) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post(int count)
    {
        for (var i = 1; i <= count; i++)
        {
            await kafkaProducer.ProduceAsync("topic1", $"key-{i}", new TestMessage { Name = $"Name-{i}" }); 
        }
        return Ok();
    }
}
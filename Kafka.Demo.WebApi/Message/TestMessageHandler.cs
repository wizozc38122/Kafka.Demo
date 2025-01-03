using Kafka.Demo.Lib.MessageHandle;

namespace Kafka.Demo.WebApi.Message;

public class TestMessageHandler : IKafkaMessageHandler<TestMessage>
{
    public async Task HandleAsync(TestMessage message)
    {
        Console.WriteLine($"Received message: {message.Name}");
        if (new Random().Next(1, 3) == 2) throw new Exception("Random exception");
        await Task.CompletedTask;
    }
}
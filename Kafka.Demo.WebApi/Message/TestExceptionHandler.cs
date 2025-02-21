using Confluent.Kafka;
using Kafka.Demo.Lib.MessageHandleException;

namespace Kafka.Demo.WebApi.Message;

public class TestExceptionHandler : IMessageHandleExceptionHandler
{
    public async Task HandleAsync(Exception exception, KafkaErrorMessage errorMessage, string groupName)
    {
        Console.WriteLine("Exception occurred");
        Console.WriteLine($"Exception: {exception.Message}");
        Console.WriteLine($"Topic: {errorMessage.Topic}");
        Console.WriteLine($"Group: {groupName}");
        Console.WriteLine($"Key: {errorMessage.Key}");
        Console.WriteLine($"Value: {errorMessage.Value}");
        await Task.CompletedTask;
    }
}
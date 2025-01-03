using Confluent.Kafka;
using Kafka.Demo.Lib.MessageHandleException;

namespace Kafka.Demo.WebApi.Message;

public class TestExceptionHandler : IMessageHandleExceptionHandler
{
    public async Task HandleAsync<TKey, TValue>(Exception exception, ConsumeResult<TKey, TValue> consumeResult, string groupName)
    {
        Console.WriteLine("Exception occurred");
        Console.WriteLine($"Exception: {exception.Message}");
        Console.WriteLine($"Topic: {consumeResult.Topic}");
        Console.WriteLine($"Group: {groupName}");
        Console.WriteLine($"Key: {consumeResult.Message.Key}");
        await Task.CompletedTask;
    }
}
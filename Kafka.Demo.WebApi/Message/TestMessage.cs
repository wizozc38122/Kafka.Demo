using Kafka.Demo.Lib.MessageHandle;

namespace Kafka.Demo.WebApi.Message;

public record TestMessage : IKafkaMessage
{
    public string Name { get; set; }
}
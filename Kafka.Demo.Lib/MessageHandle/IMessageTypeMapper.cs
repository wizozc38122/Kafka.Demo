namespace Kafka.Demo.Lib.MessageHandle
{
    public interface IMessageTypeMapper
    {
        IKafkaMessage Get(string messageType, string message);
    }
}
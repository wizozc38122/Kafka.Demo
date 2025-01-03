using System.Threading.Tasks;

namespace Kafka.Demo.Lib.MessageHandle
{
    public interface IKafkaMessageHandlerInvoker
    {
        Task InvokeHandleAsync<TMessage>(TMessage message) where TMessage : IKafkaMessage;
    }
}
using System;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Kafka.Demo.Lib.MessageHandleException
{
    public interface IMessageHandleExceptionHandler
    {
        Task HandleAsync(Exception exception, KafkaErrorMessage errorMessage,
            string groupName);
    }
}
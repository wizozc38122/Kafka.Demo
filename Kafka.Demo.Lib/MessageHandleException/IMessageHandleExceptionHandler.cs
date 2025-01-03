using System;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Kafka.Demo.Lib.MessageHandleException
{
    public interface IMessageHandleExceptionHandler
    {
        Task HandleAsync<TKey, TValue>(Exception exception, ConsumeResult<TKey, TValue> consumeResult,
            string groupName);
    }
}
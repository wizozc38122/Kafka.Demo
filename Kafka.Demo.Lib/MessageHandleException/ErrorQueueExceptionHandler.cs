using System;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Kafka.Demo.Lib.MessageHandleException
{
    public class ErrorQueueExceptionHandler : IMessageHandleExceptionHandler
    {
        private readonly string _bootstrapServers;

        public ErrorQueueExceptionHandler(string bootstrapServers)
        {
            _bootstrapServers = bootstrapServers;
        }

        public async Task HandleAsync<TKey, TValue>(Exception exception, ConsumeResult<TKey, TValue> consumeResult,
            string groupName)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = _bootstrapServers,
                Acks = Acks.All,
                CompressionType = CompressionType.Lz4,
                AllowAutoCreateTopics = true,
            };
            using (
                var producer = new ProducerBuilder<TKey, TValue>(config).Build())
            {
                await producer.ProduceAsync($"{consumeResult.Topic}_{groupName}_Error", new Message<TKey, TValue>
                {
                    Key = consumeResult.Message.Key,
                    Value = consumeResult.Message.Value,
                    Headers = consumeResult.Message.Headers
                });
            }
        }
    }
}
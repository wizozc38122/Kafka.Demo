using System;
using System.Text;
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

        public async Task HandleAsync(Exception exception, KafkaErrorMessage errorMessage,
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
                var producer = new ProducerBuilder<string, string>(config).Build())
            {
                var headers = new Headers();
                foreach (var header in errorMessage.Headers)
                {
                    headers.Add(header.Key, Encoding.UTF8.GetBytes(header.Value));
                }

                await producer.ProduceAsync($"{errorMessage.Topic}_{groupName}_Error", new Message<string, string>
                {
                    Key = errorMessage.Key,
                    Value = errorMessage.Value,
                    Headers = headers
                });
            }
        }
    }
}
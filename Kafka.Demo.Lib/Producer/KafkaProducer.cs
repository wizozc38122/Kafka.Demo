using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using Kafka.Demo.Lib.Compression;
using Kafka.Demo.Lib.MessageHandle;

namespace Kafka.Demo.Lib.Producer
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly string _bootstrapServers;

        public KafkaProducer(string bootstrapServers)
        {
            _bootstrapServers = bootstrapServers;
        }

        public async Task ProduceAsync<TMessage>(string topic, string key, TMessage kafkaMessage,
            bool compression = false) where TMessage : IKafkaMessage
        {
            var config = new ProducerConfig
            {
                BootstrapServers = _bootstrapServers,
                Acks = Acks.All,
                CompressionType = CompressionType.Lz4,
                AllowAutoCreateTopics = true,
                MessageTimeoutMs = 5000
            };

            var headers = new Headers
            {
                { "v", Encoding.UTF8.GetBytes(Global.Version) },
                { "mt", Encoding.UTF8.GetBytes(typeof(TMessage).Name) },
                { "ct", Encoding.UTF8.GetBytes(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")) },
                { "c", compression ? Encoding.UTF8.GetBytes("t") : Encoding.UTF8.GetBytes("f") }
            };

            var value = JsonSerializer.Serialize(kafkaMessage);
            var message = new Message<string, string>
            {
                Key = key,
                Value = compression ? CompressionHelper.Compress(value) : value,
                Headers = headers
            };
            using (var producer = new ProducerBuilder<string, string>(config).Build())
            {
                await producer.ProduceAsync(topic, message);
            }
        }
    }
}
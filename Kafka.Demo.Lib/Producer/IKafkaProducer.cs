using System.Threading.Tasks;
using Kafka.Demo.Lib.MessageHandle;

namespace Kafka.Demo.Lib.Producer
{
    public interface IKafkaProducer
    {
        Task ProduceAsync<TKafkaMessage>(string topic, string key, TKafkaMessage kafkaMessage, bool compression = false)
            where TKafkaMessage : IKafkaMessage;
    }
}
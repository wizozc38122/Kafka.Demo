using System.Threading.Tasks;

namespace Kafka.Demo.Lib.MessageHandle
{
    public interface IKafkaMessageHandler <in TKafkaMessage> where TKafkaMessage : IKafkaMessage 
    {
        Task HandleAsync(TKafkaMessage message); 
    }
}
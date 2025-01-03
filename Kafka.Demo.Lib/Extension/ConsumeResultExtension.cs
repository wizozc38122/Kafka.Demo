using System.Linq;
using System.Text;
using Confluent.Kafka;

namespace Kafka.Demo.Lib.Extension
{
    public static class ConsumeResultExtension
    {
        public static string GetHeaderValue<TKey, TValue>(this ConsumeResult<TKey, TValue> consumeResult, string key)
        {
            if (consumeResult.Message.Headers == null) return null;
            var header = consumeResult.Message.Headers.FirstOrDefault(h => h.Key == key);
            return header == null ? null : Encoding.UTF8.GetString(header.GetValueBytes());
        }
    }
}
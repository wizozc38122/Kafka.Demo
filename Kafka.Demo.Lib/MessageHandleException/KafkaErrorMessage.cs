using System.Collections.Generic;

namespace Kafka.Demo.Lib.MessageHandleException
{
    public class KafkaErrorMessage
    {
        public string Topic { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public Dictionary<string, string> Headers { get; set; }
    }
}
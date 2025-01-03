using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Kafka.Demo.Lib.MessageHandle
{
    public class MessageTypeMapper : IMessageTypeMapper
    {
        private readonly Dictionary<string, Type> _mapDict = new Dictionary<string, Type>();

        public MessageTypeMapper Map<TEvent>(string eventType) where TEvent : IKafkaMessage
        {
            _mapDict[eventType] = typeof(TEvent);
            return this;
        }

        public IKafkaMessage Get(string messageType, string message)
        {
            if (!_mapDict.TryGetValue(messageType, out var map)) { throw new Exception("Not Mapping"); }
            return JsonSerializer.Deserialize(message, map) as IKafkaMessage;
        }
    }
}
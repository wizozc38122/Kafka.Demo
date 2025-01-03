using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;

namespace Kafka.Demo.Lib.MessageHandle
{
    public class KafkaMessageHandlerInvoker : IKafkaMessageHandlerInvoker
    {
        private readonly IServiceProvider _serviceProvider;

        private static readonly ConcurrentDictionary<Type, MethodInfo> MethodCache =
            new ConcurrentDictionary<Type, MethodInfo>();

        private static readonly ConcurrentDictionary<Type, Type> HandlerTypeCache =
            new ConcurrentDictionary<Type, Type>();

        public KafkaMessageHandlerInvoker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeHandleAsync<TMessage>(TMessage message) where TMessage : IKafkaMessage
        {
            var messageType = message.GetType();
            var handlerType = HandlerTypeCache.GetOrAdd(messageType, type =>
                typeof(IKafkaMessageHandler<>).MakeGenericType(type));
            var eventHandler = _serviceProvider.GetService(handlerType);
            if (eventHandler is null) throw new Exception("Message handler not found");
            var method = MethodCache.GetOrAdd(handlerType, type =>
                type.GetMethod(nameof(IKafkaMessageHandler<IKafkaMessage>.HandleAsync)));
            if (method == null) throw new Exception("Message handler method not found");
            var result = method.Invoke(eventHandler, new object[] { message });
            if (!(result is Task task))
            {
                throw new Exception("Message handler method invocation failed");
            }

            await task;
        }
    }
}
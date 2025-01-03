using System;
using System.Linq;
using System.Reflection;
using Kafka.Demo.Lib.BackgroundService;
using Kafka.Demo.Lib.MessageHandle;
using Kafka.Demo.Lib.MessageHandleException;
using Kafka.Demo.Lib.Producer;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.Demo.Lib.DependencyInjection
{
    public static class KafkaServiceCollectionExtensions
    {
        public static IServiceCollection AddKafka(this IServiceCollection services, string bootstrapServers,
            Action<KafkaConfig> config = null)
        {
            var eventTypeMapper = new MessageTypeMapper();
            var kafkaConfig = new KafkaConfig(bootstrapServers, services, eventTypeMapper);
            config?.Invoke(kafkaConfig);
            services.AddSingleton<IKafkaMessageHandlerInvoker, KafkaMessageHandlerInvoker>();
            services.AddSingleton<IMessageTypeMapper>(eventTypeMapper);
            services.AddSingleton<IKafkaProducer>(new KafkaProducer(bootstrapServers));

            return services;
        }
    }

    public class KafkaConfig
    {
        private readonly IServiceCollection _services;
        private readonly MessageTypeMapper _messageTypeMapper;
        private readonly string _bootstrapServers;

        internal KafkaConfig(string bootstrapServers, IServiceCollection services,
            MessageTypeMapper messageTypeMapper)
        {
            _bootstrapServers = bootstrapServers;
            _services = services;
            _messageTypeMapper = messageTypeMapper;
        }


        public KafkaSubscribe SetSubscribe(string groupName)
        {
            return new KafkaSubscribe(_services, _bootstrapServers, groupName);
        }

        public KafkaConfig RegisterMessageHandler<TKafkaMessage, TKafkaMessageHandler>()
            where TKafkaMessage : IKafkaMessage
            where TKafkaMessageHandler : class, IKafkaMessageHandler<TKafkaMessage>
        {
            _services.AddTransient<IKafkaMessageHandler<TKafkaMessage>, TKafkaMessageHandler>();
            _messageTypeMapper.Map<TKafkaMessage>(typeof(TKafkaMessage).Name);
            return this;
        }

        public KafkaConfig RegisterMessageFromAssembly()
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly is null) return this;
            var handlerTypes = assembly.GetTypes()
                .Where(type => !type.IsAbstract && !type.IsInterface)
                .SelectMany(type => type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IKafkaMessageHandler<>))
                    .Select(i => new { HandlerType = type, EventType = i.GenericTypeArguments[0] }));
            foreach (var handler in handlerTypes)
            {
                var eventType = handler.EventType.Name;
                var method = typeof(KafkaConfig)
                    .GetMethod(nameof(RegisterMessageHandler))
                    ?.MakeGenericMethod(handler.EventType, handler.HandlerType);
                if (method != null) method.Invoke(this, Array.Empty<object>());
            }

            return this;
        }
    }

    public class KafkaSubscribe
    {
        private readonly string _groupName;
        private readonly IServiceCollection _services;
        private readonly string _bootstrapServers;

        public KafkaSubscribe(IServiceCollection services, string bootstrapServers, string groupName)
        {
            _groupName = groupName;
            _services = services;
            _bootstrapServers = bootstrapServers;
        }

        public KafkaSubscribe SubscribeTopic(string topic, int workCount = 20, bool enableErrorQueue = false,
            IMessageHandleExceptionHandler exceptionHandler = null)
        {
            _services.AddHostedService(sp =>
                new KafkaSubscribeBackgroundService(sp, _bootstrapServers, _groupName, topic, workCount,
                    enableErrorQueue,
                    exceptionHandler));
            return this;
        }
    }
}
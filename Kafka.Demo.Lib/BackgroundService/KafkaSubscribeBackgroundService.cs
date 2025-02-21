using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Kafka.Demo.Lib.Compression;
using Kafka.Demo.Lib.Extension;
using Kafka.Demo.Lib.MessageHandle;
using Kafka.Demo.Lib.MessageHandleException;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kafka.Demo.Lib.BackgroundService
{
    public class KafkaSubscribeBackgroundService : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly IKafkaMessageHandlerInvoker _kafkaMessageHandlerInvoker;
        private readonly IMessageTypeMapper _messageTypeMapper;
        private readonly IConsumer<string, string> _consumer;
        private readonly SemaphoreSlim _semaphore;
        private readonly string _groupName;
        private readonly IMessageHandleExceptionHandler _errorQueueMessageHandleExceptionHandler;
        private readonly IMessageHandleExceptionHandler _messageHandleExceptionHandler;
        private readonly ILogger<KafkaSubscribeBackgroundService> _logger;

        public KafkaSubscribeBackgroundService(IServiceProvider serviceProvider, string bootstrapServers,
            string groupName,
            string topic, int workCount, bool enableErrorQueue,
            IMessageHandleExceptionHandler messageHandleExceptionHandler = null)
        {
            _kafkaMessageHandlerInvoker = serviceProvider.GetRequiredService<IKafkaMessageHandlerInvoker>();
            _messageTypeMapper = serviceProvider.GetRequiredService<IMessageTypeMapper>();
            if (enableErrorQueue)
                _errorQueueMessageHandleExceptionHandler = new ErrorQueueExceptionHandler(bootstrapServers);
            _logger = serviceProvider.GetRequiredService<ILogger<KafkaSubscribeBackgroundService>>();

            _consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
            {
                GroupId = groupName,
                BootstrapServers = bootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            }).Build();
            _consumer.Subscribe(topic);
            _groupName = groupName;
            _messageHandleExceptionHandler = messageHandleExceptionHandler;
            _semaphore = new SemaphoreSlim(workCount);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var retryDelay = TimeSpan.FromSeconds(5);
            var consumeTimeout = TimeSpan.FromSeconds(5);

            await Task.Run(async () =>
            {
                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            _logger.LogDebug("Polling Message");
                            var consumeResult = _consumer.Consume(consumeTimeout);
                            if (consumeResult == null) continue;
                            _logger.LogDebug("Committing Message");
                            _consumer.Commit(consumeResult);

                            _logger.LogDebug("Waiting Semaphore");
                            await _semaphore.WaitAsync(stoppingToken);

                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    _logger.LogDebug(
                                        $"Processing Message {consumeResult.Message.Key} {consumeResult.Message.Value} {consumeResult.Message.Headers}");
                                    await HandleAsync(consumeResult);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Error handling message");
                                    if (_messageHandleExceptionHandler != null)
                                        await _messageHandleExceptionHandler.HandleAsync(ex, consumeResult, _groupName);
                                    if (_errorQueueMessageHandleExceptionHandler != null)
                                        await _errorQueueMessageHandleExceptionHandler.HandleAsync(ex, consumeResult,
                                            _groupName);
                                }
                                finally
                                {
                                    _logger.LogDebug("Releasing Semaphore");
                                    _semaphore.Release();
                                }
                            }, stoppingToken);
                        }
                        catch (ConsumeException ex)
                        {
                            _logger.LogError(ex, "Consume Exception");
                            await Task.Delay(retryDelay, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Unexpected Exception in Consume Loop");
                            await Task.Delay(retryDelay, stoppingToken);
                        }
                    }
                }
                finally
                {
                    _consumer.Close();
                }
            }, stoppingToken);
        }


        private async Task HandleAsync(ConsumeResult<string, string> result)
        {
            _logger.LogDebug("Handle Message");
            var messageType = result.GetHeaderValue("mt");
            var compression = result.GetHeaderValue("c");
            if (messageType is null || compression is null)
                throw new Exception("Message Type or Compression is null, Not Created by Producer");
            var message = _messageTypeMapper.Get(messageType,
                compression == "t"
                    ? CompressionHelper.Decompress(result.Message.Value)
                    : result.Message.Value);
            if (message is null) return;
            await _kafkaMessageHandlerInvoker.InvokeHandleAsync(message);
        }
    }
}
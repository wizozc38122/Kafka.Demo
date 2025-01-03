# Overview

### 消費端

- 繼承 IKafkaMessage 宣告為消息
```csharp
public record TestMessage : IKafkaMessage
{
}
```

- 繼承 IKafkaMessageHandler 消息處理

```csharp
public class TestMessageHandler : IKafkaMessageHandler<TestMessage>
{
    public async Task HandleAsync(TestMessage message)
    {
    }
}
```

- 注入

```csharp
builder.Services.AddKafka("localhost:9092", config =>
{
    // 註冊消息處理 逐筆/掃描Assembly
    config.RegisterMessageHandler<TestMessage, TestMessageHandler>();
    config.RegisterMessageFromAssembly();

    // 訂閱 Topic
    config.SetSubscribe("Group1") // 訂閱群組名稱
        .SubscribeTopic("topic1"); // 要訂閱的Topic
        
        // workCount: 同時間並行可處理的消息數量
        // enableErrorQueue: 啟動時當Handler有錯誤會將消息發送到 ErrorTopic
        // exceptionHandler: 可繼承IMessageHandleExceptionHandler自訂Handler錯誤執行
        .SubscribeTopic("topic1", workCount: 20, enableErrorQueue: true, exceptionHandler: new TestExceptionHandler());
});
```

- IMessageHandleExceptionHandler

```csharp
public class TestExceptionHandler : IMessageHandleExceptionHandler
{
    public async Task HandleAsync<TKey, TValue>(Exception exception, ConsumeResult<TKey, TValue> consumeResult, string groupName)
    {
        Console.WriteLine("Exception occurred");
        Console.WriteLine($"Exception: {exception.Message}");
        Console.WriteLine($"Topic: {consumeResult.Topic}");
        Console.WriteLine($"Group: {groupName}");
        Console.WriteLine($"Key: {consumeResult.Message.Key}");
        await Task.CompletedTask;
    }
}
```

### 發送端

```csharp
builder.Services.AddKafka("localhost:9092")
```

```csharp
// 發送
await kafkaProducer.ProduceAsync("topic", "key", new { Message: "value" });

// 資料量大可選擇壓縮，太小會反效果
await kafkaProducer.ProduceAsync("topic", "key", new { Message: "value" }, compression: true); 
```
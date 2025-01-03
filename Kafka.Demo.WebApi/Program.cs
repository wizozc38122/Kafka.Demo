using Kafka.Demo.Lib.DependencyInjection;
using Kafka.Demo.WebApi.Message;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddKafka("localhost:9092", config =>
{
    // Register message handler
    config.RegisterMessageHandler<TestMessage, TestMessageHandler>();
    config.RegisterMessageFromAssembly();

    // Subscribe topic
    config.SetSubscribe("group1")
        .SubscribeTopic("topic1", workCount: 20, enableErrorQueue: true, exceptionHandler: new TestExceptionHandler());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
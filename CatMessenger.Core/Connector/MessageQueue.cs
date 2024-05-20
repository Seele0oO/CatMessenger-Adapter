using System.Text;
using CatMessenger.Core.Config;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace CatMessenger.Core.Connector;

public class MessageQueue(IConfigProvider config, ILogger<RabbitMqConnector> logger, IConnection connection) : IDisposable
{
    public static string ExchangeName { get; } = "catmessenger";
    public static string ExchangeType { get; } = "fanout";
    public static string RoutingKey { get; } = "";
    
    public delegate void Handle(ConnectorMessage message);

    public event Handle OnMessage;

    private IChannel? Channel { get; set; }
    
    private string QueueName { get; set; }
    
    public bool IsClosing { get; private set; } = false;
    
    public async Task Connect()
    {
        Channel = await connection.CreateChannelAsync();
        
        await Channel.ExchangeDeclareAsync(ExchangeName, ExchangeType, true, false, null);
        var queue = await Channel.QueueDeclareAsync(); QueueName = queue.QueueName;
        await Channel.QueueBindAsync(QueueName, ExchangeName, RoutingKey);

        await Channel.BasicConsumeAsync(QueueName, true, new MessageConsumer(config, logger, this));
    }

    public async Task Disconnect()
    {
        await Channel.CloseAsync();
    }

    public void Dispose()
    {
        if (Channel is not null && Channel.IsOpen)
        {
            Channel.Dispose();
        }
    }
    
    public async Task Publish(ConnectorMessage message)
    {
        message.Client = config.GetName();
        message.Time ??= DateTime.Now;

        var json = JsonConvert.SerializeObject(message);
        await InternalPublish(json, 0);
    }

    private async Task InternalPublish(string json, int retry)
    {
        try
        {
            if (IsClosing)
            {
                return;
            }

            if (retry > config.GetConnectorMaxRetry())
            {
                logger.LogError("Publish failed: exceed max retry");
            }

            if (Channel is null || !Channel.IsOpen)
            {
                Disconnect().Wait();
                Connect().Wait();
            }

            await Channel!.BasicPublishAsync(ExchangeName, RoutingKey, Encoding.UTF8.GetBytes(json));
        }
        catch (Exception ex)
        {
            await InternalPublish(json, retry + 1);
        }
    }
    
    private class MessageConsumer(IConfigProvider config, ILogger<RabbitMqConnector> logger, MessageQueue queue) : DefaultBasicConsumer
    {
        public override Task HandleBasicDeliverAsync(string consumerTag, ulong deliveryTag, bool redelivered, 
            string exchange, string routingKey, ReadOnlyBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            if (queue.IsClosing)
            {
                return Task.CompletedTask;
            }

            var json = Encoding.UTF8.GetString(body.ToArray());
            
            var message = JsonConvert.DeserializeObject<ConnectorMessage>(json);

            if (message == null)
            {
                return Task.CompletedTask;
            }
            
            if (message.Client == config.GetName())
            {
                return Task.CompletedTask;
            }

            try
            {
                queue.OnMessage.Invoke(message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process: {Json}", json);
            }
            
            return Task.CompletedTask;
        }
    }
}
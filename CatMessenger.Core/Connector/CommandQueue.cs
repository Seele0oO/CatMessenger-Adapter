using System.Text;
using CatMessenger.Core.Config;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace CatMessenger.Core.Connector;

public class CommandQueue(IConfigProvider config, ILogger<RabbitMqConnector> logger, IConnection connection) : IDisposable
{
    public delegate void Handle(ConnectorCommand command, ReadOnlyBasicProperties props);

    public event Handle OnCommand;

    private IChannel? Channel { get; set; }

    private string QueueName { get; set; } = $"{config.GetName()}_command";
    
    public bool IsClosing { get; private set; } = false;
    
    public async Task Connect()
    {
        Channel = await connection.CreateChannelAsync();
        
        await Channel.QueueDeclareAsync(queue: QueueName, durable: false, exclusive: true, autoDelete: true);
        await Channel.BasicConsumeAsync(QueueName, false, new CommandConsumer(config, logger, this));
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
    
    public async Task Publish(ConnectorCommand message)
    {
        var json = JsonConvert.SerializeObject(message);
        await InternalPublish(json, message.Client, 0);
    }
    
    public async Task Reply(ReadOnlyBasicProperties props, params string[] message)
    {
        var json = JsonConvert.SerializeObject(message);
        await InternalReply(json, props, 0);
    }

    private async Task InternalPublish(string json, string client, int retry)
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

            await Channel!.BasicPublishAsync(string.Empty, $"{client}_command", Encoding.UTF8.GetBytes(json));
        }
        catch (Exception ex)
        {
            await InternalPublish(json, client, retry + 1);
        }
    }
    
    private async Task InternalReply(string json, ReadOnlyBasicProperties props, int retry)
    {
        try
        {
            if (IsClosing)
            {
                return;
            }

            if (retry > config.GetConnectorMaxRetry())
            {
                logger.LogError("Reply failed: exceed max retry");
            }

            if (Channel is null || !Channel.IsOpen)
            {
                Disconnect().Wait();
                Connect().Wait();
            }

            await Channel!.BasicPublishAsync(string.Empty, props.ReplyTo!, new BasicProperties
            {
                CorrelationId = props.CorrelationId
            }, Encoding.UTF8.GetBytes(json));
        }
        catch (Exception ex)
        {
            await InternalReply(json, props, retry + 1);
        }
    }
    
    private class CommandConsumer(IConfigProvider config, ILogger<RabbitMqConnector> logger, CommandQueue queue) : DefaultBasicConsumer
    {
        public override async Task HandleBasicDeliverAsync(string consumerTag, ulong deliveryTag, bool redelivered, 
            string exchange, string routingKey, ReadOnlyBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            if (queue.IsClosing)
            {
                await Channel.BasicAckAsync(deliveryTag, false);
                return;
            }

            var json = Encoding.UTF8.GetString(body.ToArray());
            
            var message = JsonConvert.DeserializeObject<ConnectorCommand>(json);

            try
            {
                if (message != null)
                {
                    queue.OnCommand.Invoke(message, properties);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process: {Json}", json);
            }
            finally
            {
                await Channel.BasicAckAsync(deliveryTag, false);
            }
        }
    }
}
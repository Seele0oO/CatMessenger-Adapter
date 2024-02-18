using System.Text;
using CatMessenger.Core.Config;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using ConnectionFactory = RabbitMQ.Client.ConnectionFactory;

namespace CatMessenger.Core.Connector;

public class RabbitMqConnector : IDisposable
{
    public RabbitMqConnector(IConfigProvider config, ILogger<RabbitMqConnector> logger)
    {
        Config = config;
        Logger = logger;
    }

    public static string ExchangeName { get; } = "catmessenger";
    public static string ExchangeType { get; } = "fanout";
    public static string RoutingKey { get; } = "";

    private IConfigProvider Config { get; }

    private ILogger<RabbitMqConnector> Logger { get; }

    private ConnectionFactory? Factory { get; set; }
    private IConnection? Connection { get; set; }

    private IChannel? Channel { get; set; }

    private bool IsClosing { get; } = false;
    
    private string QueueName { get; set; }
    
    public delegate void Handle(ConnectorMessage message);

    public event Handle OnMessage;

    public void Dispose()
    {
        DisposeChannel();
    }

    private void CreateFactory()
    {
        Factory = new ConnectionFactory
        {
            HostName = Config.GetConnectorHost(),
            Port = Config.GetConnectorPort(),
            VirtualHost = Config.GetConnectorVirtualHost(),
            UserName = Config.GetConnectorUsername(),
            Password = Config.GetConnectorPassword()
        };
    }

    private async Task CreateChannel()
    {
        CreateFactory();

        Connection = await Factory!.CreateConnectionAsync();
        Channel = await Connection.CreateChannelAsync();
    }

    public async Task Connect()
    {
        await CreateChannel();
        
        await Channel.ExchangeDeclareAsync(ExchangeName, ExchangeType, true, false, null);
        var queue = await Channel.QueueDeclareAsync();
        QueueName = queue.QueueName;
        await Channel.QueueBindAsync(QueueName, ExchangeName, RoutingKey);

        await Channel.BasicConsumeAsync(QueueName, true, new Consumer(this));
    }

    private class Consumer : DefaultBasicConsumer
    {
        private RabbitMqConnector Connector { get; }
        
        public Consumer(RabbitMqConnector connector)
        {
            Connector = connector;
        }
        
        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered,
            string exchange, string routingKey, in ReadOnlyBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            if (Connector.IsClosing)
            {
                return;
            }

            var json = Encoding.UTF8.GetString(body.ToArray());
            
            var message = JsonConvert.DeserializeObject<ConnectorMessage>(json);
            
            if (message.Client == Connector.Config.GetName())
            {
                return;
            }

            try
            {
                Connector.OnMessage.Invoke(message);
            }
            catch (Exception ex)
            {
                Connector.Logger.LogError(ex, "Failed to process: {Json}", json);
            }
        }
    }

    private void DisposeChannel()
    {
        if (Channel is not null && Channel.IsOpen)
        {
            Channel.Dispose();
            Connection?.Dispose();
        }
    }

    public async Task Disconnect()
    {
        await Channel.CloseAsync();
    }

    public async Task Publish(ConnectorMessage message)
    {
        message.Client = Config.GetName();
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

            if (retry > Config.GetConnectorMaxRetry())
            {
                Logger.LogError("Publish failed: exceed max retry");
            }

            if (Channel is null || !Channel.IsOpen)
            {
                DisposeChannel();
                await Connect();
            }

            await Channel!.BasicPublishAsync(ExchangeName, RoutingKey, Encoding.UTF8.GetBytes(json));
        }
        catch (Exception ex)
        {
            await InternalPublish(json, retry + 1);
        }
    }
}
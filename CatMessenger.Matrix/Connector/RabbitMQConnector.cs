using System.Text;
using CatMessenger.Matrix.Config;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using ConnectionFactory = RabbitMQ.Client.ConnectionFactory;

namespace CatMessenger.Matrix.Connector;

public class RabbitMQConnector : IDisposable
{
    public RabbitMQConnector(ConfigManager config, ILogger<RabbitMQConnector> logger)
    {
        Config = config;
        Logger = logger;

        Connect();
    }

    public static string ExchangeName { get; } = "catmessenger";
    public static string ExchangeType { get; } = "fanout";
    public static string RoutingKey { get; } = "";

    private ConfigManager Config { get; }

    private ILogger<RabbitMQConnector> Logger { get; }

    private ConnectionFactory? Factory { get; set; }

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

    private void CreateChannel()
    {
        CreateFactory();

        using var connection = Factory!.CreateConnection();
        Channel = connection.CreateChannel();
    }

    public void Connect()
    {
        if (Channel is null || !Channel.IsOpen)
        {
            DisposeChannel();
            CreateChannel();
        }

        Channel!.ExchangeDeclare(ExchangeName, ExchangeType, true, false, null);
        var queue = Channel.QueueDeclare();
        QueueName = queue.QueueName;
        Channel.QueueBind(QueueName, ExchangeName, RoutingKey);

        Channel.BasicConsume(QueueName, true, new Consumer(this));
    }

    private class Consumer : DefaultBasicConsumer
    {
        private RabbitMQConnector Connector { get; }
        
        public Consumer(RabbitMQConnector connector)
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
        if (Channel is not null)
        {
            if (Channel.IsOpen)
            {
                Channel.Dispose();
            }
        }
    }

    public async Task Disconnect()
    {
        await Channel.CloseAsync();
    }

    public async Task Publish(ConnectorMessage message)
    {
        message.Client = Config.GetName();

        if (message.Time == null)
        {
            message.Time = DateTime.Now;
        }

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
                Connect();
            }

            await Channel!.BasicPublishAsync(ExchangeName, RoutingKey, Encoding.UTF8.GetBytes(json));
        }
        catch (Exception ex)
        {
            await InternalPublish(json, retry + 1);
        }
    }
}
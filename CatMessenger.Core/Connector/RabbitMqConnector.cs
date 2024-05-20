using CatMessenger.Core.Config;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using ConnectionFactory = RabbitMQ.Client.ConnectionFactory;

namespace CatMessenger.Core.Connector;

public class RabbitMqConnector : IDisposable
{
    public RabbitMqConnector(IConfigProvider config, ILogger<RabbitMqConnector> logger)
    {
        Config = config;
        Logger = logger;
        
        Init();
    }
    
    private IConfigProvider Config { get; }
    private ILogger<RabbitMqConnector> Logger { get; }

    private ConnectionFactory? Factory { get; set; }
    private IConnection? Connection { get; set; }
    
    public MessageQueue? MessageQueue { get; private set; }
    public CommandQueue? CommandQueue { get; private set; }

    private void Init()
    {
        Factory = new ConnectionFactory
        {
            HostName = Config.GetConnectorHost(),
            Port = Config.GetConnectorPort(),
            VirtualHost = Config.GetConnectorVirtualHost(),
            UserName = Config.GetConnectorUsername(),
            Password = Config.GetConnectorPassword()
        };
        
        Connection = Factory!.CreateConnectionAsync().Result;
        MessageQueue = new MessageQueue(Config, Logger, Connection);
        CommandQueue = new CommandQueue(Config, Logger, Connection);
    }
    
    public async Task Connect()
    {
        await MessageQueue!.Connect();
        await CommandQueue!.Connect();
    }

    public void Dispose()
    {
        MessageQueue?.Dispose();
        CommandQueue?.Dispose();
        Connection?.Dispose();
    }

    public async Task Disconnect()
    {
        await MessageQueue!.Disconnect();
        await CommandQueue!.Disconnect();
    }

    public async Task Publish(ConnectorMessage message)
    {
        await MessageQueue!.Publish(message);
    }
    
    public async Task Publish(ConnectorCommand command)
    {
        await CommandQueue!.Publish(command);
    }
}
using CatMessenger.Core.Connector;
using CatMessenger.Matrix.Config;
using CatMessenger.Matrix.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CatMessenger.Matrix.Matrix;

public class MatrixClient : IHostedService
{
    private ConfigManager Config { get; }
    private RabbitMqConnector Connector { get; }
    private ILogger<MatrixClient> Logger { get; }
    
    private MatrixBot.Sdk.MatrixBot Bot { get; set; }

    public MatrixClient(ConfigManager config, RabbitMqConnector connector, ILogger<MatrixClient> logger)
    {
        Config = config;
        Connector = connector;
        Logger = logger;
        
        Bot = new MatrixBot.Sdk.MatrixBot(new MatrixLogger(logger), new MatrixConfig(config));

        Bot.OnEvent += (_, args) =>
        {
            if (args.RoomId != Config.GetMatrixRoomId())
            {
            }

            var sender = args.Event.Sender;
            var content = args.Event.Content;
            
            Logger.LogInformation(sender);
            Logger.LogInformation(content.Body);
            Logger.LogInformation(content.Format);
            Logger.LogInformation(args.RoomId);
            Logger.LogInformation(args.Event.EventId);
            Logger.LogInformation(args.Event.Type);
            Logger.LogInformation(args.Event.Type);
        };

        Connector.MessageQueue.OnMessage += message =>
        {
            Bot.PostRoomMessage(Config.GetMatrixRoomId(), MessageHelper.ToMatrixPlain(message.Sender, message.Content), "");
        };
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Connector.Connect();
        Bot.Start();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Bot.Stop();
        await Connector.Disconnect();
    }
}
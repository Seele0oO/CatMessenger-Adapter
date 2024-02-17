using CatMessenger.Matrix.Config;
using CatMessenger.Matrix.Connector;
using CatMessenger.Matrix.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CatMessenger.Matrix.Matrix;

public class MatrixClient : IHostedService
{
    private ConfigManager Config { get; }
    private RabbitMQConnector Connector { get; }
    private ILogger<MatrixClient> Logger { get; }
    
    private MatrixBot.Sdk.MatrixBot Bot { get; set; }

    public MatrixClient(ConfigManager config, RabbitMQConnector connector, ILogger<MatrixClient> logger)
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

        Connector.OnMessage += message =>
        {
            Bot.PostRoomMessage(Config.GetMatrixRoomId(), MessageHelper.ToMatrixPlain(message.Sender, message.Content), "");
        };
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Bot.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Bot.Stop();
        return Task.CompletedTask;
    }
}
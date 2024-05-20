using CatMessenger.Core.Connector;
using CatMessenger.Core.Message.MessageType;
using CatMessenger.Telegram.Bot.Bases;
using CatMessenger.Telegram.Config;
using CatMessenger.Telegram.Utilities;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace CatMessenger.Telegram.Bot;

public class PollingService(
    IServiceProvider serviceProvider,
    ILogger<PollingService> logger,
    ITelegramBotClient bot,
    ConfigProvider config,
    RabbitMqConnector connector)
    : PollingServiceBase<ReceiverService>(serviceProvider, logger)
{
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await connector.Connect();
        connector.MessageQueue.OnMessage += async message =>
        {
            await bot.SendTextMessageAsync(config.GetTelegramChatId(), MessageHelper.ToCombinedHtml(message),
                parseMode: ParseMode.Html, cancellationToken: new CancellationToken());
        };
        
        await bot.SendTextMessageAsync(config.GetTelegramChatId(), $"{config.GetName()} 适配器启动了！", cancellationToken: cancellationToken);
        await connector.Publish(new ConnectorMessage
        {
            Content = new TextMessage
            {
                Text = "适配器启动了！"
            }
        });
        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await connector.Disconnect();
        await bot.SendTextMessageAsync(config.GetTelegramChatId(), $"{config.GetName()} 适配器关闭了！", cancellationToken: cancellationToken);
        await connector.Publish(new ConnectorMessage
        {
            Content = new TextMessage
            {
                Text = "适配器关闭了！"
            }
        });
        await base.StopAsync(cancellationToken);
    }
}
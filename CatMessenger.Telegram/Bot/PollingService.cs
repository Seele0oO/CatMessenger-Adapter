using CatMessenger.Core.Connector;
using CatMessenger.Telegram.Bot.Bases;
using CatMessenger.Telegram.Config;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

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
        await bot.SendTextMessageAsync(config.GetTelegramChatId(), $"{config.GetName()} 适配器启动了！", cancellationToken: cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await connector.Disconnect();
        await bot.SendTextMessageAsync(config.GetTelegramChatId(), $"{config.GetName()} 适配器关闭了！", cancellationToken: cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
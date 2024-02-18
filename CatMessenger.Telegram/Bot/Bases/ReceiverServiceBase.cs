using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace CatMessenger.Telegram.Bot.Bases;

public abstract class ReceiverServiceBase<TUpdateHandler>(
    ITelegramBotClient bot,
    TUpdateHandler updateHandler,
    ILogger<ReceiverServiceBase<TUpdateHandler>> logger)
    : IReceiverService
    where TUpdateHandler : IUpdateHandler
{
    public async Task ReceiveAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions();

        var me = await bot.GetMeAsync(stoppingToken);
        logger.LogInformation("Start receiving updates for @{Name}", me.Username ?? "Telegram Bot");

        await bot.ReceiveAsync(
            updateHandler: updateHandler,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken);
    }
}
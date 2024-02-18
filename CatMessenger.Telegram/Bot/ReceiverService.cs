using CatMessenger.Telegram.Bot.Bases;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace CatMessenger.Telegram.Bot;

public class ReceiverService(ITelegramBotClient bot, UpdateHandler updateHandler, ILogger<ReceiverService> logger)
    : ReceiverServiceBase<UpdateHandler>(bot, updateHandler, logger);
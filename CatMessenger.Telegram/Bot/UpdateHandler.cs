using CatMessenger.Core.Connector;
using CatMessenger.Telegram.Config;
using CatMessenger.Telegram.Utilities;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace CatMessenger.Telegram.Bot;

public class UpdateHandler(
    ILogger<UpdateHandler> logger,
    ConfigProvider config,
    ITelegramBotClient bot,
    RabbitMqConnector connector)
    : IUpdateHandler
{
    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        // if (update.Type == UpdateType.Message 
        //     && update.Message!.Type == MessageType.Text 
        //     && update.Message.Text!.StartsWith("/"))
        // {
        //     var command = update.Message.Text.Substring(0, 1).Split(" ");
        //     logger.LogInformation("Telegram command: {Command}", update.Message.Text);
        //     await OnCommand(update.Message, command[0].Split('@')[0], command[1..]);
        // }
        // else
        // {
        //     var message = MessageParser.FromUpdate(update);
        //     logger.LogInformation("Telegram message: {Message}", message.ToString());
        //     connector.SendChatMessage(new ChatComponentPayload(message.ToString()));
        // }

        var message = UpdateMessageHelper.FromUpdate(update);

        if (config.IsDebug())
        {
            logger.LogInformation("Telegram message: {Message}", message.ToString());
        }
        
        await connector.Publish(message);
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogWarning(exception, "Polling error!");
        return Task.CompletedTask;
    }

    public Task OnCommand(Message message, string command, params string[] args)
    {
        
        // if (command == "online")
        // {
        //     if (args.Length != 1)
        //     {
        //         await bot.SendTextMessageAsync(message.Chat.Id, "用法不正确。/online <服务器名>", 
        //             replyToMessageId: message.MessageId);
        //         return;
        //     }
        //     
        //     bot.QueryRequestOnline(message.MessageId, args[0]);
        //     connector.SendChatMessage(new QueryOnlinePayload(args[0]));   // Todo: qyl27: When server is not exists?
        //     return;
        // }
        //
        // if (command == "time")
        // {
        //     if (args.Length is > 3 or < 1)
        //     {
        //         await bot.SendTextMessageAsync(message.Chat.Id, "用法不正确！/time <服务器名> [世界名] [查询类型]", 
        //             replyToMessageId: message.MessageId);
        //         return;
        //     }
        //
        //     var world = args.Length >= 2 ? args[1] : "world";
        //
        //     var typeStr = args.Length == 3 ? args[2] : "DayTime";
        //     var type = TelegramBotClientExtension.TimeQueryType.DayTime;
        //     if (typeStr == "GameTime")
        //     {
        //         type = TelegramBotClientExtension.TimeQueryType.GameTime;
        //     }
        //     else if (typeStr == "Day")
        //     {
        //         type = TelegramBotClientExtension.TimeQueryType.Day;
        //     }
        //     
        //     bot.QueryRequestTime(message.MessageId, args[0], world, type);
        //     connector.SendChatMessage(new QueryTimePayload(args[0], world));   // Todo: qyl27: When server or world is not exists?
        //     return;
        // }
        return Task.CompletedTask;
    }
}
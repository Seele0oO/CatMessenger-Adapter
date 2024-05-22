using CatMessenger.Core.Connector;
using CatMessenger.Telegram.Config;
using CatMessenger.Telegram.Utilities;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CatMessenger.Telegram.Bot;

public class UpdateHandler(
    ILogger<UpdateHandler> logger,
    ConfigProvider config,
    ITelegramBotClient bot,
    RabbitMqConnector connector)
    : IUpdateHandler
{
    private string? Id { get; set; }

    private static Random Random { get; } = new();

    private static string[] Meow { get; } = [
        "捕捉小猫猫~",
        "喵？喵！",
        "喵喵喵~",
        "Meow~",
        "犬科动物什么时候才能站起来！"
    ];
    
    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        if (update.Message != null && update.Message.Chat.Id != config.GetTelegramChatId())
        {
            return;
        }
        
        if (update.Type == UpdateType.Message 
            && update.Message!.Type == MessageType.Text 
            && update.Message.Text!.StartsWith('/'))
        {
            var command = update.Message.Text[1..].Split(" ");
            logger.LogInformation("Telegram command: {Command}", update.Message.Text);
            await OnCommand(update.Message, command[0], command[1..]);
            return;
        }
        
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

    public async Task OnCommand(Message message, string command, params string[] args)
    {
        Id ??= (await bot.GetMeAsync()).Username!;
        
        if (command.Contains('@'))
        {
            var sp = command.Split('@');
            if (sp[1] != Id)
            {
                return;
            }
        }

        if (command.StartsWith("online"))
        {
            if (args.Length != 1)
            {
                await bot.SendTextMessageAsync(message.Chat.Id, "用法不正确！\n/online <服务器名>", 
                    replyToMessageId: message.MessageId);
                return;
            }

            await connector.Publish(new ConnectorCommand
            {
                Command = ConnectorCommand.EnumCommand.QueryOnline,
                Client = args[0],
                ReplyTo = message.MessageId
            });
            await bot.SendTextMessageAsync(message.Chat.Id, "查询中，请稍候。", 
                replyToMessageId: message.MessageId);
        }
        else if (command.StartsWith("time"))
        {
            if (args.Length is > 3 or < 1)
            {
                await bot.SendTextMessageAsync(message.Chat.Id, "用法不正确！\n/time <服务器名> [世界名] [查询类型]", 
                    replyToMessageId: message.MessageId);
                return;
            }
            
            var world = args.Length >= 2 ? args[1] : "world";
            var typeStr = args.Length >= 3 ? args[2] : "DayTime";
            
            await connector.Publish(new ConnectorCommand
            {
                Command = ConnectorCommand.EnumCommand.QueryWorldTime,
                Client = args[0], 
                ReplyTo = message.MessageId,
                Arguments = [world, typeStr]
            });
            await bot.SendTextMessageAsync(message.Chat.Id, "查询中，请稍候。", 
                replyToMessageId: message.MessageId);
        }
        else if (command.StartsWith("meow"))
        {
            await bot.SendTextMessageAsync(message.Chat.Id, Meow[Random.Next(Meow.Length)], 
                replyToMessageId: message.MessageId);
        }
    }
}
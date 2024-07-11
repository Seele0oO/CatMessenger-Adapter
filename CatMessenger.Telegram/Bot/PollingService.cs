using CatMessenger.Core.Connector;
using CatMessenger.Core.Message.MessageType;
using CatMessenger.Telegram.Bot.Bases;
using CatMessenger.Telegram.Config;
using CatMessenger.Telegram.Utilities;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
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
        
        connector.MessageQueue!.OnMessage += async message =>
        {
            await bot.SendTextMessageAsync(config.GetTelegramChatId(), MessageHelper.ToCombinedHtml(message),
                parseMode: ParseMode.Html, cancellationToken: new CancellationToken());
        };
        connector.CommandQueue!.OnCommand += async (command, props) =>
        {
            if (command.Callback != config.GetName())
            {
                return;
            }
            
            switch (command.Command)
            {
                case ConnectorCommand.EnumCommand.ResponseOnline:
                    if (int.Parse(command.Arguments[0]) > 0)
                    {
                        await bot.SendTextMessageAsync(config.GetTelegramChatId(), 
                            $"<b>服务器 {command.Sender} 有 {command.Arguments[0]} 位玩家在线：</b>\n{string.Join("\n", command.Arguments[1..])}", 
                            replyToMessageId: command.ReplyTo, parseMode: ParseMode.Html, cancellationToken: new CancellationToken());
                        return;
                    }

                    await bot.SendTextMessageAsync(config.GetTelegramChatId(), 
                        $"<b>服务器 {command.Sender} 目前没人在线</b> :(", 
                        replyToMessageId: command.ReplyTo, parseMode: ParseMode.Html, cancellationToken: new CancellationToken());
                    return;

                case ConnectorCommand.EnumCommand.ResponseWorldTime:
                    var time = int.Parse(command.Arguments[0]) switch
                    {
                        > 0 and < 12000 => "白天☀️",
                        > 12000 and < 24000 => "夜晚🌙",
                        _ => "奇奇怪怪的时间"
                    };

                    await bot.SendTextMessageAsync(config.GetTelegramChatId(), 
                        $"<b>服务器 {command.Sender} 的主世界现在是：</b>{time}", 
                        replyToMessageId: command.ReplyTo, cancellationToken: new CancellationToken());
                    return;
                case ConnectorCommand.EnumCommand.Error:
                    await bot.SendTextMessageAsync(config.GetTelegramChatId(), "查询发生错误！", 
                        replyToMessageId: command.ReplyTo, cancellationToken: new CancellationToken());
                    return;
                case ConnectorCommand.EnumCommand.Online:
                case ConnectorCommand.EnumCommand.Offline:
                case ConnectorCommand.EnumCommand.QueryOnline:
                case ConnectorCommand.EnumCommand.QueryWorldTime:
                case ConnectorCommand.EnumCommand.RunCommand:
                case ConnectorCommand.EnumCommand.CommandResult:
                default:
                    return;
            }
        };

        await bot.SetMyCommandsAsync([
            new BotCommand
            {
                Command = "online",
                Description = "查询服务器在线人数。/online <服务器名>"
            },
            new BotCommand
            {
                Command = "time",
                Description = "查询服务器世界时间。/time <服务器名> [世界名] [查询类型]"
            },
            new BotCommand
            {
                Command = "meow",
                Description = "喵~"
            }
        ], cancellationToken: new CancellationToken());
        
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
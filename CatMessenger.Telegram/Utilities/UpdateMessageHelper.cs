using CatMessenger.Core.Connector;
using CatMessenger.Core.Message;
using CatMessenger.Core.Message.MessageType;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CatMessenger.Telegram.Utilities;

public class UpdateMessageHelper
{
    public static ConnectorMessage FromUpdate(Update update)
    {
        return update.Type switch
        {
            UpdateType.Unknown => FromUnknown(update),
            UpdateType.Message => FromMessage(update.Message!),
            UpdateType.InlineQuery => FromUnsupported(update),
            UpdateType.ChosenInlineResult => FromUnsupported(update),
            UpdateType.CallbackQuery => FromUnsupported(update),
            UpdateType.EditedMessage => FromMessage(update.EditedMessage!, true),
            UpdateType.ChannelPost => FromMessage(update.ChannelPost!),
            UpdateType.EditedChannelPost => FromMessage(update.EditedChannelPost!, true),
            UpdateType.ShippingQuery => FromUnsupported(update),
            UpdateType.PreCheckoutQuery => FromUnsupported(update),
            UpdateType.Poll => FromUnsupported(update),
            UpdateType.PollAnswer => FromUnsupported(update),
            UpdateType.MyChatMember => FromUnsupported(update),
            UpdateType.ChatMember => FromUnsupported(update),
            UpdateType.ChatJoinRequest => FromUnsupported(update),
            _ => FromUnknown(update)
        };
    }

    private static AbstractMessage GetFromUser(User? user)
    {
        if (user is null)
        {
            return new EmptyMessage();
        }
        
        var from = new TextMessage();
        if (string.IsNullOrWhiteSpace(user.LastName))
        {
            from.Text = $"{user.FirstName}";
        }
        else
        {
            from.Text = $"{user.FirstName} {user.LastName}";
        }
        
        from.Color = MessageColor.Aqua;

        if (!string.IsNullOrWhiteSpace(user.Username))
        {
            from.Hover = new TextMessage
            {
                Text = $"@{user.Username}"
            };
        }
        
        return from;
    }
    
    private static AbstractMessage GetFromChat(Chat? chat)
    {
        if (chat is null)
        {
            return new EmptyMessage();
        }
        
        var from = new TextMessage();
        switch (chat.Type)
        {
            case ChatType.Channel:
            case ChatType.Supergroup or ChatType.Group:
            {
                from.Text = chat.Title!;
                from.Bold = true; 
                break;
            }
            case ChatType.Private or ChatType.Sender:
            {
                var text = string.Empty;
                if (!string.IsNullOrWhiteSpace(chat.FirstName))
                {
                    text += $"{chat.FirstName}";
                }
                
                if (!string.IsNullOrWhiteSpace(chat.LastName))
                {
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        text += " ";
                    }
                    text += $"{chat.LastName}";
                }

                from.Text = text;
                from.Color = MessageColor.Aqua;

                if (!string.IsNullOrWhiteSpace(chat.Username))
                {
                    from.Hover = new TextMessage
                    {
                        Text = $"@{chat.Username}"
                    };
                }
                break;
            }
        }
        
        return from;
    }
    
    private static AbstractMessage GetText(string originText)
    {
        var message = new EmptyMessage();
    
        if (string.IsNullOrWhiteSpace(originText))
        {
            return message;
        }
        
        var text = originText.Replace('\n', ' ');
    
        message.Extras.Add(new TextMessage
        {
            Text = text
        });
    
        return message;
    }

    private static AbstractMessage GetStyledText(MessageEntity entity, string text, bool disableHover = false)
    {
        var message = new TextMessage
        {
            Text = text
        };
        
        switch (entity.Type)
        {
            case MessageEntityType.Mention:
            case MessageEntityType.TextMention:
            {
                message.Color = MessageColor.Blue;
                message.Underline = true;

                var hover = GetFromUser(entity.User);
                if (disableHover)
                {
                    message.Extras.Add(new TextMessage
                    {
                        Text = " ("
                    });
                    message.Extras.Add(hover);
                    message.Extras.Add(new TextMessage
                    {
                        Text = ") "
                    });
                }
                else
                {
                    message.Hover = hover;
                }
                break;
            }
            case MessageEntityType.Url:
            case MessageEntityType.TextLink:
            {
                
                message.Color = MessageColor.Blue;
                message.Underline = true;

                var hover = new TextMessage
                {
                    Text = entity.Url!
                };
                
                if (disableHover)
                {
                    message.Extras.Add(new TextMessage
                    {
                        Text = " ("
                    });
                    message.Extras.Add(hover);
                    message.Extras.Add(new TextMessage
                    {
                        Text = ") "
                    });
                }
                else
                {
                    message.Hover = hover;
                }
                break;
            }
            case MessageEntityType.PhoneNumber:
                message.Color = MessageColor.Blue;
                break;
            case MessageEntityType.Hashtag:
                message.Color = MessageColor.Blue;
                break;
            case MessageEntityType.Email:
                message.Color = MessageColor.Blue;
                break;
            case MessageEntityType.Bold:
                message.Bold = true;
                break;
            case MessageEntityType.Italic:
                message.Italic = true;
                break;
            case MessageEntityType.Underline:
                message.Underline = true;
                break;
            case MessageEntityType.Strikethrough:
                message.Strikethrough = true;
                break;
            case MessageEntityType.Spoiler:
                message.Spoiler = true;
                break;
            case MessageEntityType.BotCommand:
            case MessageEntityType.Code:
            case MessageEntityType.Pre:
            case MessageEntityType.Cashtag:
            case MessageEntityType.CustomEmoji:
            default:
                break;
        }
        
        return message;
    }

    private static AbstractMessage GetStyledMessage(string text, MessageEntity[] entities)
    {
        var message = new EmptyMessage();

        if (entities.Length == 0)
        {
            return new TextMessage
            {
                Text = text
            };
        }

        var textCursor = 0;
        foreach (var entity in entities)
        {
            if (entity.Offset > textCursor)
            {
                message.Extras.Add(new TextMessage
                {
                    Text = text[textCursor..entity.Offset]
                });
            }

            if (entity.Offset == textCursor)
            {
                // Todo: Not implemented.
                // qyl27: For combined entities.
            }

            message.Extras.Add(GetStyledText(entity, text[entity.Offset..(entity.Offset + entity.Length)]));
            textCursor = entity.Offset;
        }

        return message;
    }

    private static AbstractMessage GetSticker(Sticker sticker)
    {
        return new TextMessage
        {
            Text = $"[贴纸 {sticker.Emoji}] ",
            Color = MessageColor.Green,
            Hover = new TextMessage
            {
                Text = $"来自贴纸包 {sticker.SetName}"
            }
        };
    }
    
    private static ConnectorMessage FromMessage(Message message, bool edited = false)
    {
        var msg = new ConnectorMessage();
        var chatMsg = new EmptyMessage();

        if (message.From != null)
        {
            var sender = GetFromUser(message.From);
            msg.Sender = sender;
        }

        if (edited)
        {
            var edit = new TextMessage
            {
                Text = "[已编辑] ",
                Color = MessageColor.LightPurple
            };
            chatMsg.Extras.Add(edit);
        }
        
        if (message.ReplyToMessage != null)
        {
            var reply = message.ReplyToMessage;

            var hover = GetStyledMessage(reply.Caption ?? reply.Text ?? string.Empty, 
                reply.CaptionEntities ?? reply.Entities ?? []);
            
            var replyMsg = new TextMessage
            {
                Text = "[回复：",
                Color = MessageColor.LightPurple,
                Hover = hover
            };

            var from = GetFromUser(reply.From);
            from.Color = MessageColor.Aqua;
            from.Hover = hover;
            replyMsg.Extras.Add(from);
            
            replyMsg.Extras.Add(new TextMessage 
            {
                Text = "] ",
                Color = MessageColor.LightPurple,
                Hover = hover
            });
            
            chatMsg.Extras.Add(replyMsg);
        }

        if (message.ForwardFrom != null)
        {
            var forwardFrom = message.ForwardFrom;

            var forwardMsg = new TextMessage
            {
                Text = "[转发自 ",
                Color = MessageColor.LightPurple
            };
            forwardMsg.Extras.Add(GetFromUser(forwardFrom));
            forwardMsg.Extras.Add(new TextMessage
            {
                Text = "] "
            });
            
            chatMsg.Extras.Add(forwardMsg);
        }

        if (message.ForwardFromChat != null)
        {
            var forwardFrom = message.ForwardFromChat;
            
            var forwardMsg = new TextMessage
            {
                Text = "[转发自 ",
                Color = MessageColor.LightPurple
            };
            forwardMsg.Extras.Add(GetFromChat(forwardFrom));
            forwardMsg.Extras.Add(new TextMessage
            {
                Text = "] "
            });
            
            chatMsg.Extras.Add(forwardMsg);
        }

        if (message.Photo is { Length: > 0 })
        {
            chatMsg.Extras.Add(new TextMessage
            {
                Text = "[图片] ",
                Color = MessageColor.Green
            });
        }

        if (message.Sticker != null)
        {
            chatMsg.Extras.Add(GetSticker(message.Sticker));
        }

        if (message.Document != null)
        {
            chatMsg.Extras.Add(new TextMessage
            {
                Text = $"[文件 {message.Document.FileName}] ",
                Color = MessageColor.Blue
            });
        }
        
        if (message.Voice != null)
        {
            chatMsg.Extras.Add(new TextMessage
            {
                Text = $"[语音 {message.Voice.Duration}秒] ",
                Color = MessageColor.Blue
            });
        }
        
        if (message.Audio != null)
        {
            chatMsg.Extras.Add(new TextMessage
            {
                Text = $"[音频 {message.Audio.Duration}秒] ",
                Color = MessageColor.Blue
            });
        }
        
        if (message.Video != null)
        {
            chatMsg.Extras.Add(new TextMessage
            {
                Text = $"[视频 {message.Video.Duration}秒] ",
                Color = MessageColor.Blue
            });
        }
        
        chatMsg.Extras.Add(GetStyledMessage(message.Caption ?? message.Text ?? string.Empty,
            message.CaptionEntities ?? message.Entities ?? []));
        
        msg.Content = chatMsg;
        return msg;
    }
    
    private static ConnectorMessage FromUnsupported(Update update)
    {
        return new ConnectorMessage
        {
            Content = new TextMessage
            {
                Text = $"[不支持的消息 {update.Type}] ",
                Color = MessageColor.Red
            }
        };
    }
    
    private static ConnectorMessage FromUnknown(Update update)
    {
        return new ConnectorMessage
        {
            Content = new TextMessage
            {
                Text = $"[未知消息] ",
                Color = MessageColor.Red
            }
        };
    }
}
using System.Text;
using CatMessenger.Core.Connector;
using CatMessenger.Core.Message;
using CatMessenger.Core.Message.MessageType;

namespace CatMessenger.Telegram.Utilities;

public class MessageHelper
{
    public static string ToCombinedHtml(ConnectorMessage message)
    {
        if (message.Sender is null)
        {
            return $"""
                    〔{message.Client}〕{ToHtml(message.Content)}
                    """;
        }
        
        return $"""
                〔{message.Client}〕<b>{ToHtml(message.Sender)}</b>：
                {ToHtml(message.Content)}
                """;
    }

    public static string ToHtml(AbstractMessage? message)
    {
        if (message is null)
        {
            return string.Empty;
        }
        
        var result = new StringBuilder();

        if (message is TextMessage textMessage)
        {
            result.Append(textMessage.Text);
        }
        if (message is TranslatableMessage translatableMessage)
        {
            result.Append(string.Format(translatableMessage.Key, translatableMessage.Args.ToArray()));
        }
        if (message is NewlineMessage newlineMessage)
        {
            result.Append("<br/>");
        }
        if (message is EmptyMessage emptyMessage)
        {
        }

        if (message.HasHoverMessage())
        {
            // Todo: Hover
        }

        if (message.HasClickEvent())
        {
            // Todo: Click
        }

        if (message.Extras.Count != 0)
        {
            foreach (var extra in message.Extras)
            {
                result.Append(ToHtml(extra));
            }
        }

        if (message.Color != MessageColor.Reset)
        {
            // Todo: Color
        }
        
        if (message.Bold)
        {
            result.Insert(0, "<b>");
            result.Append("</b>");
        }
        
        if (message.Italic)
        {
            result.Insert(0, "<i>");
            result.Append("</i>");
        }
        
        if (message.Underline)
        {
            result.Insert(0, "<u>");
            result.Append("</u>");
        }
        
        if (message.Strikethrough)
        {
            result.Insert(0, "<del>");
            result.Append("</del>");
        }
        
        if (message.Spoiler)
        {
            // Todo: Spoiler.
        }

        return result.ToString();
    }
}
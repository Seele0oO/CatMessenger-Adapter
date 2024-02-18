using CatMessenger.Core.Message.Converter;
using Newtonsoft.Json.Linq;

namespace CatMessenger.Core.Message;

public abstract class AbstractMessage
{
    public MessageColor Color { get; set; } = MessageColor.Reset;
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
    public bool Strikethrough { get; set; }
    public bool Spoiler { get; set; }

    public List<AbstractMessage> Extras { get; set; } = new();

    public AbstractMessage? Hover { get; set; }

    public ClickEvent? ClickEvent { get; set; }
    public string? ClickValue { get; set; }

    public bool HasHoverMessage()
    {
        return Hover is not null;
    }

    public bool HasClickEvent()
    {
        return ClickEvent is not null && ClickValue is not null;
    }

    public JObject Write()
    {
        var jObject = WriteData();
        jObject["type"] = GetType();

        if (Color != MessageColor.Reset) jObject["color"] = Color.AsString();
        if (Bold) jObject["bold"] = Bold;
        if (Italic) jObject["italic"] = Italic;
        if (Underline) jObject["underline"] = Underline;
        if (Strikethrough) jObject["strikethrough"] = Strikethrough;
        if (Spoiler) jObject["spoiler"] = Spoiler;

        if (HasHoverMessage()) jObject["hover"] = Hover!.Write();
        if (HasClickEvent())
        {
            jObject["clickEvent"] = ClickEvent.ToString();
            jObject["clickValue"] = ClickValue;
        }

        if (Extras.Count != 0)
        {
            var extras = new JArray();
            foreach (var extra in Extras) extras.Add(extra.Write());

            jObject["extra"] = extras;
        }

        return jObject;
    }

    public void Read(JObject jObject)
    {
        ReadData(jObject);

        if (jObject.ContainsKey("color")) Color = MessageColor.FromString(jObject.Value<string>("color")!);
        if (jObject.ContainsKey("bold")) Bold = jObject.Value<bool>("bold");
        if (jObject.ContainsKey("italic")) Italic = jObject.Value<bool>("italic");
        if (jObject.ContainsKey("underline")) Underline = jObject.Value<bool>("underline");
        if (jObject.ContainsKey("strikethrough")) Strikethrough = jObject.Value<bool>("strikethrough");
        if (jObject.ContainsKey("spoiler")) Spoiler = jObject.Value<bool>("spoiler");

        if (jObject.ContainsKey("hover") && !Spoiler)
        {
            var hoverObj = jObject.Value<JObject>("hover");
            Hover = AbstractMessageConverter.FromType(hoverObj);
        }

        if (jObject.ContainsKey("clickEvent") && jObject.ContainsKey("clickValue"))
        {
            var eventStr = jObject.Value<string>("clickEvent");
            ClickEvent = Enum.Parse<ClickEvent>(eventStr);
            ClickValue = jObject.Value<string>("clickValue");
        }

        if (jObject.ContainsKey("extra"))
        {
            var extras = jObject.Value<JArray>("extra");
            foreach (var extra in extras)
                if (extra is JObject jo)
                    Extras.Add(AbstractMessageConverter.FromType(jo));
        }
    }

    public abstract string GetType();
    public abstract JObject WriteData();
    public abstract void ReadData(JObject jObject);
}
using Newtonsoft.Json.Linq;

namespace CatMessenger.Matrix.Message.MessageType;

public class TextMessage : AbstractMessage
{
    public string Text { get; set; }

    public override string GetType()
    {
        return "text";
    }

    public override JObject WriteData()
    {
        var jObject = new JObject();
        jObject["text"] = Text;
        return jObject;
    }

    public override void ReadData(JObject jObject)
    {
        Text = jObject.Value<string>("text")!;
    }
}
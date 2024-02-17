using Newtonsoft.Json.Linq;

namespace CatMessenger.Matrix.Message.MessageType;

public class NewlineMessage : AbstractMessage
{
    public override string GetType()
    {
        return "newline";
    }

    public override JObject WriteData()
    {
        return new JObject();
    }

    public override void ReadData(JObject jObject)
    {
    }
}
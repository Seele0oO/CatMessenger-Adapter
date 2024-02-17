using Newtonsoft.Json.Linq;

namespace CatMessenger.Matrix.Message.MessageType;

public class EmptyMessage : AbstractMessage
{
    public override string GetType()
    {
        return "empty";
    }

    public override JObject WriteData()
    {
        return new JObject();
    }

    public override void ReadData(JObject jObject)
    {
    }
}
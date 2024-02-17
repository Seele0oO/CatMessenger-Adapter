using Newtonsoft.Json.Linq;

namespace CatMessenger.Matrix.Message.MessageType;

public class TranslatableMessage : AbstractMessage
{
    public string Key { get; set; }
    public List<string> Args { get; set; } = new();

    public override string GetType()
    {
        return "translatable";
    }

    public override JObject WriteData()
    {
        var jObject = new JObject();
        jObject["key"] = Key;

        if (Args.Count != 0)
        {
            var args = new JArray();
            foreach (var arg in Args) args.Add(arg);
            jObject["args"] = args;
        }

        return jObject;
    }

    public override void ReadData(JObject jObject)
    {
        Key = jObject.Value<string>("key")!;

        var args = jObject.Value<JArray>("args");

        if (args is not null)
            foreach (var arg in args)
                if (arg is JValue v)
                    Args.Add(v.Value<string>()!);
    }
}
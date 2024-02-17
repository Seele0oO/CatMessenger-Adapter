using CatMessenger.Matrix.Message.MessageType;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CatMessenger.Matrix.Message.Converter;

public class AbstractMessageConverter : JsonConverter<AbstractMessage>
{
    public static AbstractMessage? FromType(JObject jObject)
    {
        var type = jObject.Value<string>("type");
        AbstractMessage? message = type switch
        {
            "empty" => new EmptyMessage(),
            "newline" => new NewlineMessage(),
            "text" => new TextMessage(),
            "translatable" => new TranslatableMessage(),
            _ => null
        };

        message?.Read(jObject);
        return message;
    }

    public override void WriteJson(JsonWriter writer, AbstractMessage? value, JsonSerializer serializer)
    {
        value?.Write().WriteTo(writer);
    }

    public override AbstractMessage? ReadJson(JsonReader reader, Type objectType, AbstractMessage? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var jObj = JObject.Load(reader);

        if (hasExistingValue)
        {
            existingValue!.Read(jObj);
            return existingValue;
        }

        return FromType(jObj);
    }
}
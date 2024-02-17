using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CatMessenger.Matrix.Message.Converter;

public class DateTimeConverter : JsonConverter<DateTime>
{
    public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
    {
        var v = new JValue(value.ToString("O"));
        v.WriteTo(writer);
    }

    public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var v = reader.ReadAsString();
        return DateTime.Parse(v!);
    }
}
using CatMessenger.Matrix.Message;
using CatMessenger.Matrix.Message.Converter;
using Newtonsoft.Json;

namespace CatMessenger.Matrix.Connector;

public class ConnectorMessage
{
    [JsonProperty("client")] public string Client { get; set; }

    [JsonProperty("content")]
    [JsonConverter(typeof(AbstractMessageConverter))]
    public AbstractMessage Content { get; set; }

    [JsonProperty("sender")]
    [JsonConverter(typeof(AbstractMessageConverter))]
    public AbstractMessage Sender { get; set; }

    [JsonProperty("time")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime? Time { get; set; }
}
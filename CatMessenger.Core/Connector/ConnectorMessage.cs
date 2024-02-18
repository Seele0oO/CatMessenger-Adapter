using CatMessenger.Core.Message;
using CatMessenger.Core.Message.Converter;
using Newtonsoft.Json;

namespace CatMessenger.Core.Connector;

public class ConnectorMessage
{
    [JsonProperty("client")]
    public string Client { get; set; }

    [JsonProperty("content")]
    [JsonConverter(typeof(AbstractMessageConverter))]
    public AbstractMessage Content { get; set; }

    [JsonProperty("sender")]
    [JsonConverter(typeof(AbstractMessageConverter))]
    public AbstractMessage? Sender { get; set; }
    
    [JsonProperty("time")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime? Time { get; set; }
}
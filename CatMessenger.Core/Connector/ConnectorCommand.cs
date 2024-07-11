using Newtonsoft.Json;

namespace CatMessenger.Core.Connector;

public class ConnectorCommand
{
    [JsonProperty("sender")]
    public string Sender { get; set; }
    
    [JsonProperty("callback")]
    public string Callback { get; set; }
    
    [JsonProperty("command")]
    public EnumCommand Command { get; set; }
    
    [JsonProperty("reply_to")]
    public int ReplyTo { get; set; }
    
    [JsonProperty("arguments")]
    public string[] Arguments { get; set; }
    
    public enum EnumCommand
    {
        Error = 0,
        Online = 1,             // Not used.
        Offline = 2,            // Not used.
        QueryOnline = 3,
        QueryWorldTime = 4,
        ResponseOnline = 5,
        ResponseWorldTime = 6,
        RunCommand = 7,         // Not implemented.
        CommandResult = 8,      // Not implemented.
    }
}
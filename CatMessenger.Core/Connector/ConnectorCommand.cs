using Newtonsoft.Json;

namespace CatMessenger.Core.Connector;

public class ConnectorCommand
{
    [JsonProperty("client")]
    public string Client { get; set; }
    
    [JsonProperty("command")]
    public EnumCommand Command { get; set; }
    
    [JsonProperty("arguments")]
    public string[] Arguments { get; set; }
    
    public enum EnumCommand
    {
        Nope = 0,
        Online = 1,            // Not used.
        Offline = 2,           // Not used.
        QueryOnline = 3,
        QueryWorldTime = 4,
        RunCommand = 5,        // Not implemented.
    }
}
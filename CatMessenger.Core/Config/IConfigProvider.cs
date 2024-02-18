namespace CatMessenger.Core.Config;

public interface IConfigProvider
{
    public bool IsDebug();
    public string GetName();
    
    public string GetConnectorHost();
    public int GetConnectorPort();
    public string GetConnectorVirtualHost();
    public string GetConnectorUsername();
    public string GetConnectorPassword();
    public int GetConnectorMaxRetry();
    
}
using CatMessenger.Core.Config;
using Microsoft.Extensions.Configuration;

namespace CatMessenger.Telegram.Config;

public class ConfigProvider(IConfiguration config) : IConfigProvider
{
    public static string GetDevEnvironmentVariable()
    {
        return Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
    }

    private static bool HasDevEnvironmentVariable()
    {
        return GetDevEnvironmentVariable().Equals("Development", StringComparison.OrdinalIgnoreCase);
    }
    
    public string GetTelegramToken()
    {
        return config.GetValue<string>("Telegram:Token") ?? "";
    }
    
    public bool IsTelegramProxyEnabled()
    {
        return config.GetValue<bool>("Telegram:Proxy:Enabled");
    }

    public string GetTelegramProxyUrl()
    {
        return config.GetValue<string>("Telegram:Proxy:Url") ?? "";
    }
    
    public string GetTelegramChatId()
    {
        return config.GetValue<string>("Telegram:ChatId")!;
    }

    public bool IsDebug()
    {
        return HasDevEnvironmentVariable() || config.GetValue<bool>("Debug");
    }

    public string GetName()
    {
        return config.GetValue<string>("Name")!;
    }
    
    public string GetConnectorHost()
    {
        return config.GetValue<string>("Connector:Host") ?? "";
    }

    public int GetConnectorPort()
    {
        return config.GetValue<int>("Connector:Port");
    }

    public string GetConnectorVirtualHost()
    {
        return config.GetValue<string>("Connector:VirtualHost") ?? "";
    }

    public string GetConnectorUsername()
    {
        return config.GetValue<string>("Connector:Username") ?? "";
    }

    public string GetConnectorPassword()
    {
        return config.GetValue<string>("Connector:Password") ?? "";
    }

    public int GetConnectorMaxRetry()
    {
        return config.GetValue<int>("Connector:MaxRetry");
    }
}
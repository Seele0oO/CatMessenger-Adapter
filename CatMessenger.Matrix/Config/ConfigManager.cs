using Microsoft.Extensions.Configuration;

namespace CatMessenger.Matrix.Config;

public class ConfigManager
{
    public ConfigManager(IConfiguration config)
    {
        Config = config;
    }

    private IConfiguration Config { get; }

    public static string GetDevEnvironmentVariable()
    {
        return Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
    }

    private static bool HasDevEnvironmentVariable()
    {
        return GetDevEnvironmentVariable().Equals("Development", StringComparison.OrdinalIgnoreCase);
    }

    public bool IsDebug()
    {
        return HasDevEnvironmentVariable() || Config.GetValue<bool>("Debug");
    }

    public string GetName()
    {
        return Config.GetValue<string>("Name") ?? "";
    }

    public string GetConnectorHost()
    {
        return Config.GetValue<string>("Connector:Host") ?? "";
    }

    public int GetConnectorPort()
    {
        return Config.GetValue<int>("Connector:Port");
    }

    public string GetConnectorVirtualHost()
    {
        return Config.GetValue<string>("Connector:VirtualHost") ?? "";
    }

    public string GetConnectorUsername()
    {
        return Config.GetValue<string>("Connector:Username") ?? "";
    }

    public string GetConnectorPassword()
    {
        return Config.GetValue<string>("Connector:Password") ?? "";
    }

    public int GetConnectorMaxRetry()
    {
        return Config.GetValue<int>("Connector:MaxRetry");
    }
}
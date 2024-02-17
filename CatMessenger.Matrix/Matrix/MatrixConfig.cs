using CatMessenger.Matrix.Config;
using MatrixBot.Sdk;

namespace CatMessenger.Matrix.Matrix;

public class MatrixConfig : IMatrixBotStore
{
    private ConfigManager Config { get; }

    public MatrixConfig(ConfigManager config)
    {
        Config = config;
    }
    
    public MatrixBotConfig? Read()
    {
        return new MatrixBotConfig
        {
            ServerUri = Config.GetMatrixUri(), 
            Username = Config.GetMatrixUsername(),
            Password = Config.GetMatrixPassword()
        };
    }

    public void Write(MatrixBotConfig config)
    {
        // Not implemented.
    }
}
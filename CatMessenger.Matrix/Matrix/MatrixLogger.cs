using Microsoft.Extensions.Logging;
using LoggingLogger = Microsoft.Extensions.Logging.ILogger;
using ILogger = MatrixBot.Sdk.ILogger;

namespace CatMessenger.Matrix.Matrix;

public class MatrixLogger : ILogger
{
    private LoggingLogger Logger { get; }

    public MatrixLogger(LoggingLogger logger)
    {
        Logger = logger;
    }
    
    public void Info(string message)
    {
        Logger.LogInformation(message);
    }

    public void Error(string message)
    {
        Logger.LogError(message);
    }
}
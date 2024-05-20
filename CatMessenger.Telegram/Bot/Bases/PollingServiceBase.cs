using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace CatMessenger.Telegram.Bot.Bases;

public abstract class PollingServiceBase<TReceiverService>(
    IServiceProvider serviceProvider,
    ILogger<PollingServiceBase<TReceiverService>> logger)
    : BackgroundService
    where TReceiverService : IReceiverService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting polling service");
        
        await DoReceive(stoppingToken);
    }

    private async Task DoReceive(CancellationToken stoppingToken)
    {
        try
        {
            // Fixme: qyl27: Maybe we needn't multi bot in same host?
            using var scope = serviceProvider.CreateScope();
            var receiver = scope.ServiceProvider.GetRequiredService<TReceiverService>();
            // var receiver = ServiceProvider.GetService<TReceiverService>();
                
            await receiver.ReceiveAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Polling failed!");
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
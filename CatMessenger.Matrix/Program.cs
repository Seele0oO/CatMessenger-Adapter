using CatMessenger.Matrix.Config;
using CatMessenger.Matrix.Connector;
using CatMessenger.Matrix.Matrix;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("config.json", true, true)
    .AddJsonFile($"config.{ConfigManager.GetDevEnvironmentVariable()}.json", true, true)
    .AddCommandLine(args)
    .AddEnvironmentVariables();

builder.Logging.ClearProviders()
    .SetMinimumLevel(LogLevel.Trace)
    .AddNLog();

builder.Services.AddSingleton<ConfigManager>();
builder.Services.AddSingleton<RabbitMQConnector>();
builder.Services.AddHostedService<MatrixClient>();

using var host = builder.Build();

await host.RunAsync();
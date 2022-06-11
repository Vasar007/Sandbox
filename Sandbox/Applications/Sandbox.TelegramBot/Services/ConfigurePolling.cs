using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;

namespace Sandbox.TelegramBot.Services;

public sealed class ConfigurePolling : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ConfigurePolling> _logger;

    public ConfigurePolling(
        IServiceProvider services,
        ILogger<ConfigurePolling> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _services.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
        var updateHandler = scope.ServiceProvider.GetRequiredService<IUpdateHandler>();
        var updateReceiver = scope.ServiceProvider.GetRequiredService<IUpdateReceiver>();

        _logger.LogInformation("Need to delete webhook at first to start receiving updates via polling.");

        await botClient.DeleteWebhookAsync(null, stoppingToken);

        _logger.LogInformation("Starting receiving updates from Telegram API via polling.");

        await updateReceiver.ReceiveAsync(updateHandler, stoppingToken);

        _logger.LogInformation("Receiving updates from Telegram API via polling has been finished.");
    }
}

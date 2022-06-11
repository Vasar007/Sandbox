using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace Sandbox.TelegramBot.Services;

public sealed class UpdateHandler : IUpdateHandler
{
    private readonly HandleUpdateService _handleUpdateService;

    public UpdateHandler(
        HandleUpdateService handleUpdateService)
    {
        _handleUpdateService = handleUpdateService;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        await _handleUpdateService.EchoAsync(update, cancellationToken);
    }

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        await _handleUpdateService.HandleErrorAsync(exception);
    }
}

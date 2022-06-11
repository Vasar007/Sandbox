namespace Sandbox.TelegramBot.Options;

public sealed class BotOptions
{
    public string BotToken { get; init; } = default!;
    public string HostAddress { get; init; } = default!;
    public bool UseWebhook { get; init; } = true;

    public BotOptions()
    {
    }
}

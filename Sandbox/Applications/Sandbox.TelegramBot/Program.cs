using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Sandbox.TelegramBot.Services;
using Sandbox.TelegramBot.Options;
using Telegram.Bot.Extensions.Polling;

namespace Sandbox.TelegramBot;

public static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            Console.WriteLine("Console application started.");
            RunInternal(args);

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred:{Environment.NewLine}{ex.Message}");
            return -1;
        }
        finally
        {
            Console.WriteLine("Console application stopped.");
            Console.WriteLine("Press any key to close this window...");
            Console.ReadKey();
        }
    }

    private static void RunInternal(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var botConfig = builder.Configuration.GetSection("BotConfiguration").Get<BotOptions>();

        // Register named HttpClient to get benefits of IHttpClientFactory
        // and consume it with ITelegramBotClient typed client.
        // More read:
        //  https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#typed-clients
        //  https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
        builder.Services.AddHttpClient("tgwebhook")
            .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(botConfig.BotToken, httpClient));

        // Dummy business-logic service
        builder.Services.AddScoped<IUpdateReceiver, DefaultUpdateReceiver>(provider => new DefaultUpdateReceiver(provider.GetRequiredService<ITelegramBotClient>()));
        builder.Services.AddScoped<IUpdateHandler, UpdateHandler>();
        builder.Services.AddScoped<HandleUpdateService>();

        // There are several strategies for completing asynchronous tasks during startup.
        // Some of them could be found in this article https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-1/
        // We are going to use IHostedService to add and later remove Webhook
        if (botConfig.UseWebhook)
            builder.Services.AddHostedService<ConfigureWebhook>();
        else
            builder.Services.AddHostedService<ConfigurePolling>();

        // The Telegram.Bot library heavily depends on Newtonsoft.Json library to deserialize
        // incoming webhook updates and send serialized responses back.
        // Read more about adding Newtonsoft.Json to ASP.NET Core pipeline:
        //   https://docs.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-6.0#add-newtonsoftjson-based-json-format-support
        builder.Services
            .AddControllers()
            .AddNewtonsoftJson();

        var app = builder.Build();

        app.UseRouting();
        app.UseCors();

        app.UseEndpoints(endpoints =>
        {
            // Configure custom endpoint per Telegram API recommendations:
            // https://core.telegram.org/bots/api#setwebhook
            // If you'd like to make sure that the Webhook request comes from Telegram, we recommend
            // using a secret path in the URL, e.g. https://www.example.com/<token>.
            // Since nobody else knows your bot's token, you can be pretty sure it's us.
            var token = botConfig.BotToken;
            endpoints.MapControllerRoute(
                name: "tgwebhook",
                pattern: $"bot/{token}",
                new { controller = "Webhook", action = "Post" }
            );
            endpoints.MapControllers();
        });

        app.Run();
    }
}

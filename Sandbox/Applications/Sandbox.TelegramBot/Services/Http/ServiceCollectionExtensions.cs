using Acolyte.Assertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sandbox.TelegramBot.Models.Options;

namespace Sandbox.TelegramBot.Services.Http
{
    public static class ServiceCollectionExtensions
    {
        public static IHttpClientBuilder AddHttpClientWithOptions(
            this IServiceCollection services, HttpClientOptions options, ILogger logger)
        {
            services.ThrowIfNull(nameof(services));
            options.ThrowIfNull(nameof(options));
            logger.ThrowIfNull(nameof(logger));

            return services
                .AddHttpClient(options.HttpClientDefaultName)
                .AddBuilderOptionsInternal(options, logger);
        }

        public static IHttpClientBuilder AddHttpClientWithOptions<TClient>(
           this IServiceCollection services, HttpClientOptions options, ILogger logger)
           where TClient : class
        {
            services.ThrowIfNull(nameof(services));
            options.ThrowIfNull(nameof(options));
            logger.ThrowIfNull(nameof(logger));

            return services
                .AddHttpClient<TClient>(options.HttpClientDefaultName)
                .AddBuilderOptionsInternal(options, logger);
        }

        public static IHttpClientBuilder AddHttpClientWithOptions<TClient, TImplementation>(
            this IServiceCollection services, HttpClientOptions options, ILogger logger)
            where TClient : class
            where TImplementation : class, TClient
        {
            services.ThrowIfNull(nameof(services));
            options.ThrowIfNull(nameof(options));
            logger.ThrowIfNull(nameof(logger));

            return services
                .AddHttpClient<TClient, TImplementation>(options.HttpClientDefaultName)
                .AddBuilderOptionsInternal(options, logger);
        }

        private static IHttpClientBuilder AddBuilderOptionsInternal(this IHttpClientBuilder builder,
            HttpClientOptions options, ILogger logger)
        {
            return builder
                .AddHttpOptions(options, logger);
        }
    }
}

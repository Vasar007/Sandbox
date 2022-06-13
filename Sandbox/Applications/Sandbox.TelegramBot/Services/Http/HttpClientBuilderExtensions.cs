using System.Net.Http;
using Acolyte.Assertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sandbox.TelegramBot.Options;

namespace Sandbox.TelegramBot.Services.Http
{
    public static class HttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddHttpOptions(
           this IHttpClientBuilder builder, HttpClientOptions options, ILogger logger)
        {
            builder.ThrowIfNull(nameof(builder));
            options.ThrowIfNull(nameof(options));
            logger.ThrowIfNull(nameof(logger));

            return builder
                .ConfigurePrimaryHttpMessageHandlerWithOptions(options, logger)
                .AddHttpErrorPoliciesWithOptions(options, logger)
                .AddHttpMessageHandlersWithOptions(options); // Common handlers should be placed after Polly ones!
        }

        public static IHttpClientBuilder AddHttpMessageHandlersWithOptions(
           this IHttpClientBuilder builder, HttpClientOptions options)
        {
            builder.ThrowIfNull(nameof(builder));
            options.ThrowIfNull(nameof(options));

            return builder
                .AddHttpMessageHandler(() => new HttpClientTimeoutHandler(options));
        }

        public static IHttpClientBuilder ConfigurePrimaryHttpMessageHandlerWithOptions(
           this IHttpClientBuilder builder, HttpClientOptions options, ILogger logger)
        {
            builder.ThrowIfNull(nameof(builder));
            options.ThrowIfNull(nameof(options));
            logger.ThrowIfNull(nameof(logger));

            return builder
                .ConfigurePrimaryHttpMessageHandler(() => options.ConfigureHandlerWithOptions(logger));
        }

        /// <summary>
        /// Configures common project error policy for HTTP client.
        /// </summary>
        /// <param name="builder">The <see cref="IHttpClientBuilder" />.</param>
        /// <param name="serviceOptions">The service options.</param>
        /// <returns>
        /// An <see cref="IHttpClientBuilder" /> that can be used to configure the client.
        /// </returns>
        /// <remarks>
        /// Policies configured by AddTransientHttpErrorPolicy handle the following responses:
        /// • Network failures (as <see cref="HttpRequestException" />)<br/>
        /// • HTTP 5XX status codes (server errors)<br/>
        /// • HTTP 408 status code (request timeout)<br/>
        /// Or you can create custom policy and add it by AddPolicyHandler.
        /// </remarks>
        public static IHttpClientBuilder AddHttpErrorPoliciesWithOptions(
            this IHttpClientBuilder builder, HttpClientOptions options, ILogger logger)
        {
            builder.ThrowIfNull(nameof(builder));
            options.ThrowIfNull(nameof(options));
            logger.ThrowIfNull(nameof(logger));

            return builder
                .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.WaitAndRetryWithOptionsAsync(options, logger))
                .AddPolicyHandler(PolicyCreator.WaitAndRetryWithOptionsOnTimeoutExceptionAsync(options, logger));
        }
    }
}

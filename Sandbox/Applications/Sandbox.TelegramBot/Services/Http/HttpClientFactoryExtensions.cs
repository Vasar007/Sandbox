using System;
using System.Net.Http;
using Acolyte.Assertions;
using Acolyte.Common;
using Acolyte.Common.Monads;
using Microsoft.Extensions.Logging;
using Sandbox.TelegramBot.Models.Options;

namespace Sandbox.TelegramBot.Services.Http
{
    public static class HttpClientFactoryExtensions
    {

        public static HttpClient CreateClientWithOptions(this IHttpClientFactory httpClientFactory,
            HttpClientOptions options, ILogger logger)
        {
            httpClientFactory.ThrowIfNull(nameof(httpClientFactory));
            options.ThrowIfNull(nameof(options));

            return httpClientFactory.CreateClientWithOptions(baseAddress: null, options, logger);
        }

        public static HttpClient CreateClientWithOptions(this IHttpClientFactory httpClientFactory,
            string? baseAddress, HttpClientOptions options, ILogger logger)
        {
            httpClientFactory.ThrowIfNull(nameof(httpClientFactory));
            options.ThrowIfNull(nameof(options));
            logger.ThrowIfNull(nameof(logger));

            string defaultClientName = options.HttpClientDefaultName;
            string serviceUrl = baseAddress.ToStringNullSafe(CommonConstants.NotAvailable);
            logger.LogInformation($"Using client '{defaultClientName}' and service URL: {serviceUrl}");

            HttpClient client = httpClientFactory.CreateClient(defaultClientName);
            try
            {
                return client
                    .ApplyIf(!string.IsNullOrWhiteSpace(baseAddress), c => c.ConfigureBaseAddress(baseAddress!))
                    .ConfigureWithOptions(options)
                    .ConfigureWithJsonMedia();
            }
            catch (Exception)
            {
                client.DisposeClient(options);
                throw;
            }
        }
    }
}

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Acolyte.Assertions;
using Acolyte.Common;
using Microsoft.Extensions.Logging;
using Sandbox.TelegramBot.Models.Options;
using Sandbox.TelegramBot.Models.Responses;

namespace Sandbox.TelegramBot.Services.Http
{
    public static class HttpClientExtensions
    {
        public static bool DisposeClient(this HttpClient? client, HttpClientOptions options)
        {
            options.ThrowIfNull(nameof(options));

            return client.DisposeClient(options.ShouldDisposeHttpClient);
        }

        public static bool DisposeClient(this HttpClient? client,
            bool shouldDisposeHttpClient)
        {
            if (shouldDisposeHttpClient)
            {
                client.DisposeSafe();
                return true;
            }

            return false;
        }

        public static HttpClient ConfigureBaseAddress(this HttpClient client, string baseAddress)
        {
            client.ThrowIfNull(nameof(client));
            baseAddress.ThrowIfNull(nameof(baseAddress));

            client.BaseAddress = new Uri(baseAddress);

            return client;
        }

        public static HttpClient ConfigureWithOptions(this HttpClient client,
            HttpClientOptions options)
        {
            client.ThrowIfNull(nameof(client));
            options.ThrowIfNull(nameof(options));

            client.Timeout = options.TimeoutOnRequest;

            return client;
        }

        public static HttpClient ConfigureWithJsonMedia(this HttpClient client)
        {
            client.ThrowIfNull(nameof(client));

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }

        public static Task<Result<TResponse, ErrorResponse>> SendAndReadAsync<TResponse>(
            this HttpClient client, HttpRequestMessage request, ILogger logger)
            where TResponse : class
        {
            return client.SendAndReadAsync<TResponse>(
                request,
                logger,
                continueOnCapturedContext: false,
                cancellationToken: CancellationToken.None
            );
        }

        public static async Task<Result<TResponse, ErrorResponse>> SendAndReadAsync<TResponse>(
            this HttpClient client, HttpRequestMessage request, ILogger logger,
            bool continueOnCapturedContext, CancellationToken cancellationToken)
            where TResponse : class
        {
            client.ThrowIfNull(nameof(client));
            request.ThrowIfNull(nameof(request));
            logger.ThrowIfNull(nameof(logger));

            logger.LogInformation($"Sending {request.Method} request to '{request.RequestUri}'.");

            using var response = await client.SendAsync(request, cancellationToken)
                .ConfigureAwait(continueOnCapturedContext);

            return await response.ReadContentAsAsync<TResponse>(
                    logger, continueOnCapturedContext, cancellationToken
                )
                .ConfigureAwait(continueOnCapturedContext);
        }
    }
}

using System;
using System.Net.Http;
using Acolyte.Assertions;
using Acolyte.Common.Monads;
using Microsoft.Extensions.Logging;
using MihaZupan;
using Sandbox.TelegramBot.Models.Options;

namespace Sandbox.TelegramBot.Services.Http
{
    public static class HttpClientHandlerExtensions
    {
        public static HttpClientHandler ConfigureHandlerWithOptions(
            this HttpClientOptions options, ILogger logger)
        {
            options.ThrowIfNull(nameof(options));
            logger.ThrowIfNull(nameof(logger));

            return new HttpClientHandler()
                .ConfigureHandlerWithOptions(options, logger);
        }

        public static HttpClientHandler ConfigureHandlerWithOptions(
            this HttpClientHandler handler, HttpClientOptions options, ILogger logger)
        {
            handler.ThrowIfNull(nameof(handler));
            options.ThrowIfNull(nameof(options));
            logger.ThrowIfNull(nameof(logger));

            handler.AllowAutoRedirect = options.AllowAutoRedirect;
            handler.UseCookies = options.UseCookies;
            handler.UseProxy = options.UseDefaultProxy;

            return handler
                .ApplyIf(!options.ValidateServerCertificates, h => h.DisableServerCertificateValidation(logger))
                .ApplyIf(options.UseSocks5Proxy, h => h.ConfigureSocks5Proxy(options, logger));
        }

        public static HttpClientHandler DisableServerCertificateValidation(
            this HttpClientHandler handler, ILogger logger)
        {
            handler.ThrowIfNull(nameof(handler));
            logger.ThrowIfNull(nameof(logger));

            logger.LogWarning("ATTENTION! Server certificates validation is disabled.");
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            return handler;
        }

        public static HttpClientHandler ConfigureSocks5Proxy(this HttpClientHandler handler,
            HttpClientOptions options, ILogger logger)
        {
            handler.ThrowIfNull(nameof(handler));
            options.ThrowIfNull(nameof(options));
            logger.ThrowIfNull(nameof(logger));

            var socks5HostName = options.Socks5HostName;
            var socks5Port = options.Socks5Port;
            if (string.IsNullOrWhiteSpace(socks5HostName) ||
                socks5Port is null)
            {
                const string message = "Failed to configure SOCKS5 proxy: specify valid options.";
                throw new ArgumentException(message, nameof(options));
            }

            logger.LogInformation($"Using SOCKS5 proxy [{socks5HostName}, {socks5Port}].");

            handler.UseProxy = true;
            handler.Proxy = new HttpToSocks5Proxy(
                socks5HostName, socks5Port.Value
            );

            return handler;
        }
    }
}

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Acolyte.Assertions;
using Microsoft.Extensions.Logging;
using Polly;
using Sandbox.TelegramBot.Models.Options;

namespace Sandbox.TelegramBot.Services.Http
{
    public static class PolicyCreator
    {
        public static IAsyncPolicy<HttpResponseMessage> WaitAndRetryWithOptionsAsync(
            this PolicyBuilder<HttpResponseMessage> policyBuilder,
            HttpClientOptions options, ILogger logger)
        {
            policyBuilder.ThrowIfNull(nameof(policyBuilder));
            options.ThrowIfNull(nameof(options));
            logger.ThrowIfNull(nameof(logger));

            return policyBuilder
                .WaitAndRetryAsync(
                    options.RetryCountOnFailed,
                    retryCount => options.RetryTimeoutOnFailed,
                    (o, s, r, c) => OnFailedAsync(o, s, r, c, logger)
                );
        }

        public static IAsyncPolicy<HttpResponseMessage> WaitAndRetryWithOptionsOnTimeoutExceptionAsync(
            HttpClientOptions options, ILogger logger)
        {
            options.ThrowIfNull(nameof(options));

            return Policy<HttpResponseMessage>
                .Handle<TimeoutException>()
                .WaitAndRetryWithOptionsAsync(options, logger);
        }

        private static Task OnFailedAsync(DelegateResult<HttpResponseMessage> outcome,
            TimeSpan sleepDuration, int retryCount, Context _, ILogger logger)
        {
            logger.LogRetryingInfo(outcome, sleepDuration, retryCount);
            return Task.CompletedTask;
        }

        public static IAsyncPolicy<HttpResponseMessage> HandleUnauthorizedAsync(
            HttpClientOptions options,
            Func<DelegateResult<HttpResponseMessage>, TimeSpan, int, Context, Task> onRetryAsync)
        {
            options.ThrowIfNull(nameof(options));
            onRetryAsync.ThrowIfNull(nameof(onRetryAsync));

            return Policy<HttpResponseMessage>
                .HandleResult(response => IsUnauthorized(response))
                .WaitAndRetryAsync(
                    options.RetryCountOnAuth,
                    retryCount => options.RetryTimeoutOnAuth,
                    onRetryAsync
                );
        }

        private static bool IsUnauthorized(HttpResponseMessage response)
        {
            return response.StatusCode == HttpStatusCode.Unauthorized;
        }
    }
}

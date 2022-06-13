using System;
using System.Net.Http;
using Acolyte.Assertions;
using Microsoft.Extensions.Logging;
using Polly;

namespace Sandbox.TelegramBot.Services.Http
{
    public static class LoggerExtensions
    {
        public static void LogRetryingInfo(this ILogger logger,
            DelegateResult<HttpResponseMessage> outcome, TimeSpan sleepDuration, int retryCount)
        {
            logger.ThrowIfNull(nameof(logger));
            outcome.ThrowIfNull(nameof(outcome));

            string commonPart = $"Retrying attempt {retryCount} with timeout {sleepDuration}.";

            if (outcome.Result is not null)
            {
                string statusCode = ((int) outcome.Result.StatusCode).ToString();
                string details = $"{outcome.Result.ReasonPhrase} (code: {statusCode})";
                logger.LogWarning($"Request failed: {details}. {commonPart}");
            }
            else if (outcome.Exception is not null)
            {
                logger.LogWarning(outcome.Exception, $"Request failed. {commonPart}");
            }
            else
            {
                logger.LogWarning($"Request failed. {commonPart}");
            }
        }
    }
}

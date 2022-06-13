using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Acolyte.Assertions;
using Acolyte.Common;
using Microsoft.Extensions.Logging;
using Sandbox.TelegramBot.Models.Responses;

namespace Sandbox.TelegramBot.Services.Http
{
    public static class HttpResponseMessageExtensions
    {
        public static Task<Result<TResponse, ErrorResponse>> ReadContentAsAsync<TResponse>(
           this HttpResponseMessage response, ILogger logger)
           where TResponse : class
        {
            return response.ReadContentAsAsync<TResponse>(
                logger,
                continueOnCapturedContext: false,
                cancellationToken: CancellationToken.None
            );
        }

        public static async Task<Result<TResponse, ErrorResponse>> ReadContentAsAsync<TResponse>(
           this HttpResponseMessage response, ILogger logger, bool continueOnCapturedContext,
           CancellationToken cancellationToken)
           where TResponse : class
        {
            response.ThrowIfNull(nameof(response));
            logger.ThrowIfNull(nameof(logger));

            var requestUri = response.RequestMessage?.RequestUri;
            var statusCode = ((int) response.StatusCode).ToString();
            string responseDetails = $"[{response.ReasonPhrase}] (code: {statusCode})";

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation($"Got a success status code from '{requestUri}': {responseDetails}.");
                var result = await response.Content.ReadAsAsync<TResponse>(cancellationToken)
                    .ConfigureAwait(continueOnCapturedContext);

                return Result.Ok(result);
            }

            logger.LogError($"Got an error status code from '{requestUri}': {responseDetails}.");

            // Response does not have content for 401 error, e.g. calling method with "Authorize"
            // attribute but request does not contain "Authorization" header.
            var error = await response.Content.ReadAsAsync<ErrorResponse>(cancellationToken)
                .ConfigureAwait(continueOnCapturedContext);

            if (error is null)
            {
                // In case response does not have any content, create error from common properties.
                error = new ErrorResponse
                {
                    Success = false,
                    ErrorCode = statusCode,
                    ErrorMessage = response.ReasonPhrase
                };
            }

            return Result.Error(error);
        }
    }
}

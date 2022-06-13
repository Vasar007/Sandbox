using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Acolyte.Assertions;
using Acolyte.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sandbox.TelegramBot.Models.Options;
using Sandbox.TelegramBot.Services.Http;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Sandbox.TelegramBot.Services
{
    public sealed class BotService : ITelegramBotClient
    {
        private readonly HttpClientOptions _hcOptions;
        private readonly BotOptions _botServiceOptions;

        private readonly HttpClient _httpClient;

        private readonly bool _continueOnCapturedContext;

        private string BotToken => _botServiceOptions.BotToken;

        /// <inheritdoc />
        public ITelegramBotClient BotClient { get; }


        public BotService(
            IHttpClientFactory httpClientFactory,
            IOptions<HttpClientOptions> hcOptions,
            IOptions<BotOptions> botServiceOptions,
            ILogger<BotService> logger)
        {
            httpClientFactory.ThrowIfNull(nameof(httpClientFactory));
            _hcOptions = hcOptions.ThrowIfNull(nameof(httpClientFactory)).Value;
            _botServiceOptions = botServiceOptions.ThrowIfNull(nameof(botServiceOptions)).Value;

            try
            {
                _httpClient = httpClientFactory.CreateClientWithOptions(_hcOptions, logger);
                _continueOnCapturedContext = false;

                BotClient = new TelegramBotClient(BotToken, _httpClient);
            }
            catch (Exception)
            {
                _httpClient.DisposeClient(_hcOptions);
                throw;
            }
        }

        #region IDisposable Implementation

        /// <summary>
        /// Boolean flag used to show that object has already been disposed.
        /// </summary>
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;

            _httpClient.DisposeClient(_hcOptions);

            _disposed = true;
        }

        #endregion

        #region IBotService Implementation

        /// <inheritdoc />
        public async Task<IReadOnlyList<Update>> GetUpdatesAsync(
            int? offset = null,
            int? limit = null,
            int? timeout = null,
            IEnumerable<UpdateType>? allowedUpdates = null,
            CancellationToken cancellationToken = default)
        {
            return await InternalCall(
                () => BotClient.GetUpdatesAsync(
                    offset: offset,
                    limit: limit,
                    timeout: timeout,
                    allowedUpdates: allowedUpdates,
                    cancellationToken: cancellationToken
                )
            ).ConfigureAwait(_continueOnCapturedContext);
        }

        /// <inheritdoc />
        public async Task SetWebhookAsync(
            string url,
            InputFileStream? certificate = null,
            string? ipAddress = null,
            int? maxConnections = null,
            IEnumerable<UpdateType>? allowedUpdates = null,
            bool? dropPendingUpdates = null,
            CancellationToken cancellationToken = default)
        {
            await InternalCall(
               () => BotClient.SetWebhookAsync(
                    url: url,
                    certificate: certificate,
                    ipAddress: ipAddress,
                    maxConnections: maxConnections,
                    allowedUpdates: allowedUpdates,
                    dropPendingUpdates: dropPendingUpdates,
                    cancellationToken: cancellationToken
                )
            ).ConfigureAwait(_continueOnCapturedContext);
        }

        /// <inheritdoc />
        public async Task DeleteWebhookAsync(
            bool? dropPendingUpdates = null,
            CancellationToken cancellationToken = default)
        {
            await InternalCall(
                () => BotClient.DeleteWebhookAsync(
                    dropPendingUpdates: dropPendingUpdates,
                    cancellationToken: cancellationToken
                )
            ).ConfigureAwait(_continueOnCapturedContext);
        }

        /// <inheritdoc />
        public async Task<WebhookInfo> GetWebhookInfoAsync(
            CancellationToken cancellationToken = default)
        {
            return await InternalCall(
                 () => BotClient.GetWebhookInfoAsync(
                     cancellationToken: cancellationToken
                 )
             ).ConfigureAwait(_continueOnCapturedContext);
        }

        /// <inheritdoc />
        public async Task<Message> SendTextMessageAsync(
            ChatId chatId,
            string text,
            ParseMode? parseMode = null,
            IEnumerable<MessageEntity>? entities = null,
            bool? disableWebPagePreview = null,
            bool? disableNotification = null,
            int? replyToMessageId = null,
            bool? allowSendingWithoutReply = null,
            IReplyMarkup? replyMarkup = null,
            CancellationToken cancellationToken = default)
        {
            return await InternalCall(
                () => BotClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: text,
                    parseMode: parseMode,
                    entities: entities,
                    disableWebPagePreview: disableWebPagePreview,
                    disableNotification: disableNotification,
                    replyToMessageId: replyToMessageId,
                    allowSendingWithoutReply: allowSendingWithoutReply,
                    replyMarkup: replyMarkup,
                    cancellationToken: cancellationToken
                )
            ).ConfigureAwait(_continueOnCapturedContext);
        }

        #endregion

        private Task InternalCall(Func<Task> action)
        {
            return InternalCall(async () =>
            {
                await action()
                    .ConfigureAwait(_continueOnCapturedContext);
                return default(bool);
            });
        }

        private async Task<TResult> InternalCall<TResult>(Func<Task<TResult>> action)
        {
            try
            {
                return await action()
                    .ConfigureAwait(_continueOnCapturedContext);
            }
            catch (Exception ex)
            {
                throw ReconstructExceptionIfNeeded(ex);
            }
        }

        private static Exception ReconstructExceptionIfNeeded(Exception ex)
        {
            return ex switch
            {
                ApiRequestException apiEx => new Exception(ConstructMessageFrom(apiEx), apiEx),

                _ => ex
            };
        }

        private static string ConstructMessageFrom(ApiRequestException ex)
        {
            var requestParameters = ex.Parameters;

            string parameters = requestParameters is null
                ? "No parameters were specified"
                : $"[MigrateToChatId: {requestParameters.MigrateToChatId.ToStringNullSafe()}, " +
                  $"RetryAfter: {requestParameters.RetryAfter.ToStringNullSafe()}]";

            return $"Telegram API exception: {ex.Message} ({ex.ErrorCode}). Parameters: {parameters}.";
        }

        #region ITelegramBotClient Implementation

        long? ITelegramBotClient.BotId => BotClient.BotId;

        TimeSpan ITelegramBotClient.Timeout
        {
            get => BotClient.Timeout;
            set => BotClient.Timeout = value;
        }
        IExceptionParser ITelegramBotClient.ExceptionsParser
        {
            get => BotClient.ExceptionsParser;
            set => BotClient.ExceptionsParser = value;
        }

        event AsyncEventHandler<ApiRequestEventArgs>? ITelegramBotClient.OnMakingApiRequest
        {
            add => BotClient.OnMakingApiRequest += value;
            remove => BotClient.OnMakingApiRequest -= value;
        }

        event AsyncEventHandler<ApiResponseEventArgs>? ITelegramBotClient.OnApiResponseReceived
        {
            add => BotClient.OnApiResponseReceived += value;
            remove => BotClient.OnApiResponseReceived -= value;
        }

        Task<TResponse> ITelegramBotClient.MakeRequestAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            return BotClient.MakeRequestAsync(request, cancellationToken);
        }

        Task<bool> ITelegramBotClient.TestApiAsync(CancellationToken cancellationToken)
        {
            return BotClient.TestApiAsync(cancellationToken);
        }

        Task ITelegramBotClient.DownloadFileAsync(string filePath, Stream destination, CancellationToken cancellationToken)
        {
            return BotClient.DownloadFileAsync(filePath, destination, cancellationToken);
        }

        #endregion
    }
}

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Acolyte.Assertions;
using Microsoft.Extensions.Options;
using Sandbox.TelegramBot.Models.Options;

namespace Sandbox.TelegramBot.Services.Http
{
    public sealed class HttpClientTimeoutHandler : DelegatingHandler
    {
        public TimeSpan Timeout { get; }


        public HttpClientTimeoutHandler(
            TimeSpan timeout)
        {
            Timeout = timeout;
        }

        public HttpClientTimeoutHandler(
            HttpClientOptions options)
            : this(options.ThrowIfNull(nameof(options)).HttpHandlerTimeout)
        {
        }

        public HttpClientTimeoutHandler(
            IOptions<HttpClientOptions> options)
            : this(options.ThrowIfNull(nameof(options)).Value)
        {
        }

        protected async override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            using var cts = TryCreateCancellationTokenSource(request, cancellationToken);

            try
            {
                return await base.SendAsync(
                    request,
                    cts?.Token ?? cancellationToken
                );
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException("Request timeout was reached.", ex);
            }
        }

        private CancellationTokenSource? TryCreateCancellationTokenSource(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var timeout = request.FindTimeout() ?? Timeout;
            if (timeout == System.Threading.Timeout.InfiniteTimeSpan)
            {
                // No need to create a CTS if there's no timeout.
                return null;
            }

            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            try
            {
                cts.CancelAfter(timeout);
                return cts;
            }
            catch (Exception)
            {
                cts.Dispose();
                throw;
            }
        }
    }
}

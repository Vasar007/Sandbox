using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using Acolyte.Assertions;

namespace Sandbox.TelegramBot.Services.Http
{
    public static class HttpRequestExtensions
    {
        private static readonly string TimeoutPropertyKey = "RequestTimeout";
        private static HttpRequestOptionsKey<TimeSpan?> TimeoutPropertyStrongKey => new(TimeoutPropertyKey);

        public static void SetTimeout(this HttpRequestMessage request, TimeSpan? timeout)
        {
            request.ThrowIfNull(nameof(request));

            request.Options.Set(TimeoutPropertyStrongKey, timeout);
        }

        public static TimeSpan? FindTimeout(this HttpRequestMessage request)
        {
            request.ThrowIfNull(nameof(request));

            if (request.Options.TryGetValue(TimeoutPropertyStrongKey, out var timeout))
            {
                return timeout;
            }

            return null;
        }

        public static HttpRequestMessage AsJson<TValue>(this HttpRequestMessage request,
            TValue value)
            where TValue : class
        {
            request.ThrowIfNull(nameof(request));

            request.Content = new ObjectContent<TValue>(value, new JsonMediaTypeFormatter());
            return request;
        }
    }
}

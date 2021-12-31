using Miellax.Models;
using System.Net;
using System.Net.Http;

namespace Miellax.Utilities
{
    public static class HttpClientBuilder
    {
        public static HttpClient GetHttpClient(CheckerSettings checkerSettings, Proxy proxy = null, CookieContainer cookieContainer = null)
        {
            var httpMessageHandler = GetHttpMessageHandler(checkerSettings, proxy, cookieContainer);

            var httpClient = new HttpClient(httpMessageHandler)
            {
                Timeout = proxy.Settings.Timeout
            };

            if (proxy.Settings.Rotating)
            {
                httpClient.DefaultRequestHeaders.ConnectionClose = true;
            }

            return httpClient;
        }

        public static HttpMessageHandler GetHttpMessageHandler(CheckerSettings checkerSettings, Proxy proxy = null, CookieContainer cookieContainer = null)
        {
            IWebProxy webProxy = proxy != null ? new WebProxy($"{proxy.Settings.Protocol.ToString().ToLower()}://{proxy.Host}:{proxy.Port}")
            {
                Credentials = proxy.Credentials,
            } : null;

            return new HttpClientHandler()
            {
                Proxy = webProxy,
                AllowAutoRedirect = checkerSettings.AllowAutoRedirect,
                UseCookies = checkerSettings.UseCookies,
                CookieContainer = cookieContainer ?? new CookieContainer(),
                MaxAutomaticRedirections = checkerSettings.MaxAutomaticRedirections
            };
        }
    }
}

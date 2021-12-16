using Milky.Enums;
using SocksSharp;
using SocksSharp.Proxy;
using System.Net;
using System.Net.Http;

namespace Milky.Models
{
    public class Proxy
    {
        public Proxy(string proxy, ProxySettings settings)
        {
            Settings = settings;

            string[] split = proxy.Split(':');

            if (split.Length != 2 && split.Length != 4)
            {
                return;
            }

            Host = split[0];

            if (!int.TryParse(split[1], out int port) || port > 65535)
            {
                return;
            }

            Port = port;

            if (split.Length == 4)
            {
                Credentials = new NetworkCredential(split[2], split[3]);
            }

            Valid = true;
        }

        internal bool Valid { get; }

        internal ProxySettings Settings { get; }

        internal string Host { get; }

        internal int Port { get; }

        internal NetworkCredential Credentials { get; }

        public HttpClient GetHttpClient(CookieContainer cookieContainer = null, bool useCookies = false, bool allowAutomaticRedirects = true, int maximumAutoRedirects = 50)
        {
            var httpMessageHandler = GetHttpMessageHandler(cookieContainer, useCookies, allowAutomaticRedirects, maximumAutoRedirects);

            var httpClient = new HttpClient(httpMessageHandler)
            {
                Timeout = Settings.Timeout
            };

            if (Settings.Rotating)
            {
                httpClient.DefaultRequestHeaders.ConnectionClose = true;
            }

            return httpClient;
        }

        public HttpMessageHandler GetHttpMessageHandler(CookieContainer cookieContainer = null, bool useCookies = false, bool allowAutomaticRedirects = true, int maximumAutoRedirects = 50)
        {
            if (Settings.Protocol == ProxyProtocol.HTTP)
            {
                return new HttpClientHandler()
                {
                    Proxy = new WebProxy(Host, Port) { Credentials = Credentials },
                    AllowAutoRedirect = allowAutomaticRedirects,
                    UseCookies = useCookies,
                    CookieContainer = cookieContainer ?? new CookieContainer(),
                    MaxAutomaticRedirections = maximumAutoRedirects
                };
            }

            var timeoutMilliseconds = (int)Settings.Timeout.TotalMilliseconds;

            var proxySettings = new SocksSharp.Proxy.ProxySettings()
            {
                Host = Host, Port = Port,
                Credentials = Credentials,
                ConnectTimeout = timeoutMilliseconds,
                ReadWriteTimeOut = timeoutMilliseconds
            };

            // Note: MaxAutomaticRedirections not supported by SocksSharp
            return Settings.Protocol switch
            {
                ProxyProtocol.SOCKS4 => new ProxyClientHandler<Socks4>(proxySettings)
                {
                    AllowAutoRedirect = allowAutomaticRedirects,
                    UseCookies = useCookies,
                    CookieContainer = cookieContainer ?? new CookieContainer(),
                },
                ProxyProtocol.SOCKS4A => new ProxyClientHandler<Socks4a>(proxySettings)
                {
                    AllowAutoRedirect = allowAutomaticRedirects,
                    UseCookies = useCookies,
                    CookieContainer = cookieContainer ?? new CookieContainer(),
                },
                ProxyProtocol.SOCKS5 => new ProxyClientHandler<Socks5>(proxySettings)
                {
                    AllowAutoRedirect = allowAutomaticRedirects,
                    UseCookies = useCookies,
                    CookieContainer = cookieContainer ?? new CookieContainer(),
                }
            };
        }
    }
}

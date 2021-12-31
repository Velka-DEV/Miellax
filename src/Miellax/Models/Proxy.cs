using Miellax.Exceptions;
using System.Net;

namespace Miellax.Models
{
    public class Proxy
    {
        internal string Host { get; }

        internal int Port { get; }

        internal ProxySettings Settings { get; }

        internal NetworkCredential Credentials { get; }

        public Proxy(string proxy, ProxySettings settings)
        {
            Settings = settings;

            string[] split = proxy.Split(':');

            if (split.Length != 2 && split.Length != 4)
            {
                throw new InvalidProxyException();
            }

            Host = split[0];

            if (!int.TryParse(split[1], out int port) || port > 65535)
            {
                throw new InvalidProxyException();
            }

            Port = port;

            if (split.Length == 4)
            {
                Credentials = new NetworkCredential(split[2], split[3]);
            }
        }
    }
}
using Miellax.Exceptions;
using System;

namespace Miellax.Models
{
    public class UrlCredential : ICredential
    {
        public string Url { get; }
        public Uri Uri { get; }
        public string Raw { get; }

        public UrlCredential(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new InvalidUrlException();
            }

            Url = Raw = url;

            try
            {
                Uri = new Uri(url);
            }
            catch (UriFormatException)
            {
                throw new InvalidUrlException();
            }
        }

        public UrlCredential(Uri uri)
        {
            Url = Raw = uri.AbsolutePath;
            Uri = uri;
        }
    }
}

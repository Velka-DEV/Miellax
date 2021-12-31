using System;

namespace Miellax.Exceptions
{
    internal class InvalidProxyException : Exception
    {
        public InvalidProxyException() { }

        public InvalidProxyException(string message)
            : base(message) { }

        public InvalidProxyException(string message, Exception inner)
            : base(message, inner) { }
    }
}

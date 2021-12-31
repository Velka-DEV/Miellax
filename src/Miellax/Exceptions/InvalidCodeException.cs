using System;

namespace Miellax.Exceptions
{
    internal class InvalidCodeException : Exception
    {
        public InvalidCodeException() { }

        public InvalidCodeException(string message)
            : base(message) { }

        public InvalidCodeException(string message, Exception inner)
            : base(message, inner) { }
    }
}

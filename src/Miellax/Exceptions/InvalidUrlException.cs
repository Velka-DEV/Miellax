using System;

namespace Miellax.Exceptions
{
    public class InvalidUrlException : Exception
    {
        public InvalidUrlException() { }

        public InvalidUrlException (string message)
            : base(message) { }

        public InvalidUrlException (string message, Exception inner)
            : base(message, inner) { }
    }
}

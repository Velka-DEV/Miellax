using System;

namespace Miellax.Exceptions
{
    internal class InvalidComboException : Exception
    {
        public InvalidComboException() { }

        public InvalidComboException(string message)
            : base(message) { }

        public InvalidComboException(string message, Exception inner)
            : base(message, inner) { }
    }
}

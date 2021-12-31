using Miellax.Exceptions;

namespace Miellax.Models
{
    public class CodeCredential : ICredential
    {
        public string Value { get; }
        public string Raw { get; }

        public CodeCredential(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new InvalidComboException();
            }

            Raw = Value = code;
        }
    }
}

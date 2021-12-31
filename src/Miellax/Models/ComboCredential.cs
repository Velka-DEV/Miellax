using Miellax.Exceptions;
using System;

namespace Miellax.Models
{
    public class ComboCredential : ICredential
    {
        private const int SplitCount = 2;

        public string Username { get; }
        public string Password { get; }
        public string Raw { get; }
        private string Separator { get; }

        public ComboCredential(string combo, string separator = ":")
        {
            if (string.IsNullOrEmpty(combo))
            {
                throw new InvalidComboException();
            }

            Separator = separator;
            Raw = combo;

            string[] split = combo.Split(separator, SplitCount, StringSplitOptions.RemoveEmptyEntries);

            if (split.Length != SplitCount)
            {
                throw new InvalidComboException();
            }

            Username = split[0];
            Password = split[1];
        }

        public override string ToString()
        {
            return string.Join(Separator, Username, Password);
        }
    }
}

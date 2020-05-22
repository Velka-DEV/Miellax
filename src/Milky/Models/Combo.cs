﻿using System;

namespace Milky.Models
{
    public class Combo
    {
        private static readonly int _splitCount = 2;

        public Combo(string combo, string separator = ":")
        {
            string[] split = combo.Split(separator, _splitCount, StringSplitOptions.RemoveEmptyEntries);

            if (split.Length != _splitCount)
            {
                return;
            }

            Username = split[0];
            Password = split[1];
            Separator = separator;
            IsValid = true;
        }

        internal bool IsValid { get; private set; }

        public string Username { get; private set; }

        public string Password { get; private set; }

        private string Separator { get; set; }

        public override string ToString()
        {
            return string.Join(Separator, Username, Password);
        }
    }
}

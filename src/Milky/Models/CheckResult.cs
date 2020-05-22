﻿using Milky.Enums;
using System.Collections.Generic;

namespace Milky.Models
{
    public class CheckResult
    {
        public CheckResult()
        {
            // Hi
        }

        public CheckResult(ComboResult comboResult) : this()
        {
            ComboResult = comboResult;
        }

        public CheckResult(ComboResult comboResult, IDictionary<string, object> captures) : this(comboResult)
        {
            Captures = captures;
        }

        public ComboResult ComboResult { get; private set; } = ComboResult.Invalid;

        public IDictionary<string, object> Captures { get; private set; }

        /// <summary>
        /// File name to output combo to in the <see cref="OutputSettings.OutputDirectory"/>, ".txt" will automatically be added to it
        /// </summary>
        public string OutputFile { get; set; }
    }
}

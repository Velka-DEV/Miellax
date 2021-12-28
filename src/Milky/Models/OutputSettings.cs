﻿using Milky.Enums;
using System;
using System.Globalization;
using System.IO;

namespace Milky.Models
{
    public class OutputSettings
    {
        /// <summary>
        /// Directory to output results to
        /// </summary>
        public string OutputDirectory { get; set; } = Path.Combine("Results", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(DateTime.Now.ToString("MMM dd, yyyy — HH.mm.ss")));

        /// <summary>
        /// Whether to output invalids or not
        /// Warning: This may slow down your check substantially
        /// </summary>
        public bool OutputInvalids { get; set; } = false;

        /// <summary>
        /// Whether to display free's in console
        /// </summary>
        public bool DisplayFrees { get; set; } = true;

        /// <summary>
        /// Whether to display unknown's in console
        /// </summary>
        public bool DisplayUnknowns { get; set; } = false;

        /// <summary>
        /// Whether to display banned's in console
        /// </summary>
        public bool DisplayBanneds { get; set; } = false;

        /// <summary>
        /// Separator that's going to be used to separate the combo and the capture, as well as each capture
        /// </summary>
        public string CaptureSeparator { get; set; } = " | ";

        /// <summary>
        /// Console output color for <see cref="ComboResult.Hit"/>
        /// </summary>
        public ConsoleColor HitColor { get; set; } = ConsoleColor.Green;

        /// <summary>
        /// Console output color for <see cref="ComboResult.Free"/>
        /// </summary>
        public ConsoleColor FreeColor { get; set; } = ConsoleColor.Cyan;

        /// <summary>
        /// Console output color for <see cref="ComboResult.Unknown"/>
        /// </summary>
        public ConsoleColor UnknownColor { get; set; } = ConsoleColor.DarkRed;
        
        /// <summary>
        /// Console output color for <see cref="ComboResult.Banned"/>
        /// </summary>
        public ConsoleColor BannedColor { get; set; } = ConsoleColor.Yellow;

        /// <summary>
        /// Console output color for <see cref="ComboResult.Invalid"/>
        /// </summary>
        public ConsoleColor InvalidColor { get; set; } = ConsoleColor.Red;
    }
}

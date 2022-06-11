using Miellax.Enums;
using Miellax.Extensions;
using Miellax.Models;
using Miellax.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Miellax
{
    public class Checker
    {
        public CheckerInfo Info { get; }

        private readonly CheckerSettings _checkerSettings;
        private readonly OutputSettings _outputSettings;
        private readonly Func<ICredential, HttpClient, int, Task<CheckResult>> _checkProcess;
        private readonly Action<ICredential, CheckResult> _outputProcess;
        private readonly List<ICredential> _credentials;
        private readonly Library<HttpClient> _httpClientLibrary;

        public static Checker CheckerInstance { get; private set; }

        internal Checker(CheckerSettings checkerSettings, OutputSettings outputSettings, Func<ICredential, HttpClient, int, Task<CheckResult>> checkProcess, Action<ICredential, CheckResult> outputProcess, List<ICredential> credentials, Library<HttpClient> httpClientLibrary)
        {
            Info = new CheckerInfo(credentials.Count);

            _checkerSettings = checkerSettings;
            _outputSettings = outputSettings;
            _outputProcess = outputProcess;
            _checkProcess = checkProcess;
            _credentials = credentials;
            _httpClientLibrary = httpClientLibrary;

            CheckerInstance = this;
        }

        public async Task StartAsync()
        {
            if (Info.Status != CheckerStatus.Idle)
            {
                throw new Exception("Checker already started.");
            }

            _ = StartCpmCounterAsync();

            Info.Start = DateTime.Now;
            Info.Status = CheckerStatus.Running;

            await _credentials.ForEachAsync(_checkerSettings.MaxThreads, async credential =>
            {
                Info.CancellationTokenSource.Token.ThrowIfCancellationRequested();

                while (Info.Status == CheckerStatus.Paused)
                {
                    await Task.Delay(1000).ConfigureAwait(false); // I'm not sure if this is the best practice
                }

                int attempts = 1;
                CheckResult checkResult;

                while (true)
                {
                    KeyValuePair<int, HttpClient> httpClient;

                    if (_checkerSettings.UseProxies)
                    {
                        _httpClientLibrary.TryBorrowRandom(out httpClient);
                    }
                    else
                    {
                        httpClient = _httpClientLibrary.Items[0];
                    }

                    checkResult = await _checkProcess(credential, httpClient.Value, attempts).ConfigureAwait(false);

                    _httpClientLibrary.Return(httpClient);

                    if (checkResult.IncrementAttempts)
                    {
                        attempts++;

                        if (attempts > _checkerSettings.MaxAttempts)
                            break;
                    }

                    if (checkResult.ComboResult == ComboResult.Retry)
                    {
                        lock (Info.Locker)
                        {
                            Info.Retries++;
                        }

                        continue;
                    }

                    break;
                }

                _outputProcess(credential, checkResult);

                lock (Info.Locker)
                {
                    Info.Checked.Add(credential);

                    if (checkResult.ComboResult == ComboResult.Hit)
                    {
                        Info.Hits++;
                    }
                    else if (checkResult.ComboResult == ComboResult.Free)
                    {
                        Info.Free++;
                    }
                }
            }).ConfigureAwait(false);

            lock (Info.Locker)
            {
                if (!Info.CancellationTokenSource.IsCancellationRequested)
                {
                    Abort();
                }
            }
        }

        public void Abort()
        {
            lock (Info.Locker)
            {
                if (Info.Status == CheckerStatus.Idle)
                {
                    throw new Exception("Checker not started.");
                }

                if (Info.Status == CheckerStatus.Done)
                {
                    throw new Exception("Checker already ended.");
                }

                Info.CancellationTokenSource.Cancel();
                Info.End = DateTime.Now;
                Info.Status = CheckerStatus.Done;
            }
        }

        public void Pause()
        {
            lock (Info.Locker)
            {
                if (Info.Status != CheckerStatus.Running)
                {
                    throw new Exception("Checker not running.");
                }

                Info.LastPause = DateTime.Now;
                Info.Status = CheckerStatus.Paused;
            }
        }

        /// <returns>Pause duration <see cref="TimeSpan"/></returns>
        public TimeSpan Resume()
        {
            lock (Info.Locker)
            {
                if (Info.Status != CheckerStatus.Paused)
                {
                    throw new Exception("Checker not paused.");
                }

                TimeSpan pauseDuration = DateTime.Now - Info.LastPause;

                Info.TotalPause = Info.TotalPause.Add(pauseDuration);
                Info.Status = CheckerStatus.Running;

                return pauseDuration;
            }
        }

        public int SaveUnchecked()
        {
            lock (Info.Locker)
            {
                Pause();

                string outputPath = Path.Combine(_outputSettings.OutputDirectory ?? string.Empty, "Unchecked.txt");

                List<ICredential> @unchecked = _credentials.Except(Info.Checked).ToList();

                File.WriteAllLines(outputPath, @unchecked.Select(c => c.ToString()));

                Resume();

                return @unchecked.Count;
            }
        }

        private async Task StartCpmCounterAsync()
        {
            while (Info.Status != CheckerStatus.Done)
            {
                int checkedBefore = Info.Checked.Count;
                await Task.Delay(6000).ConfigureAwait(false);
                int checkedAfter = Info.Checked.Count;

                Info.Cpm = (checkedAfter - checkedBefore) * 10;
            }
        }

        public static void OutputProcess(ICredential combo, CheckResult checkResult)
        {
            if ((checkResult.ComboResult == ComboResult.Invalid && !CheckerInstance._outputSettings.OutputInvalids) || (checkResult.ComboResult == ComboResult.Locked && !CheckerInstance._outputSettings.OutputLockeds))
            {
                return;
            }

            var outputBuilder = new StringBuilder(combo.Raw);

            if (checkResult.Captures != null && checkResult.Captures.Count != 0)
            {
                IEnumerable<string> captures = checkResult.Captures
                    .Where(c => !string.IsNullOrWhiteSpace(c.Value?.ToString())) // If capture value is either null, empty or white-space, we don't want it to be included
                    .Select(c => $"{c.Key} = {c.Value}");

                outputBuilder.Append(CheckerInstance._outputSettings.CaptureSeparator).AppendJoin(CheckerInstance._outputSettings.CaptureSeparator, captures);
            }

            var outputString = outputBuilder.ToString();

            lock (CheckerInstance.Info.Locker)
            {
                foreach (string outputFile in checkResult.OutputFiles ?? new[] { checkResult.ComboResult.ToString() })
                {
                    string outputPath = Path.Combine(CheckerInstance._outputSettings.OutputDirectory ?? string.Empty, outputFile + ".txt");

                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                    File.AppendAllText(outputPath, outputString + Environment.NewLine);

                    if (CheckerInstance._outputSettings.GlobalOutput)
                    {
                        string globalOutputPath = Path.Combine(Path.Combine(CheckerInstance._outputSettings.OutputDirectory == null ? "Results" : Directory.GetParent(CheckerInstance._outputSettings.OutputDirectory).FullName, "Global"), outputFile + ".txt");

                        Directory.CreateDirectory(Path.GetDirectoryName(globalOutputPath));

                        File.AppendAllText(globalOutputPath, outputString + Environment.NewLine);
                    }
                }

                Console.ForegroundColor = checkResult.ComboResult switch
                {
                    ComboResult.Hit => CheckerInstance._outputSettings.HitColor,
                    ComboResult.Free => CheckerInstance._outputSettings.FreeColor,
                    ComboResult.Invalid => CheckerInstance._outputSettings.InvalidColor,
                    ComboResult.Locked => CheckerInstance._outputSettings.BannedColor,
                    ComboResult.Unknown => CheckerInstance._outputSettings.UnknownColor
                };

                if (CheckerInstance._outputSettings.CustomColors.Count > 0 && checkResult.Captures != null)
                {
                    foreach (var customColor in CheckerInstance._outputSettings.CustomColors)
                    {
                        if (checkResult.Captures.TryGetValue(customColor.Value.Key, out var capturedObject))
                        {
                            if (customColor.Value.Value(capturedObject))
                            {
                                Console.ForegroundColor = customColor.Key;
                                break;
                            }
                        }
                    }
                }

                switch (checkResult.ComboResult)
                {
                    case ComboResult.Free:
                        if (CheckerInstance._outputSettings.DisplayFrees)
                            Console.WriteLine(outputString);
                        break;
                    case ComboResult.Unknown:
                        if (CheckerInstance._outputSettings.OutputUnknowns)
                            Console.WriteLine(outputString);
                        break;
                    case ComboResult.Locked:
                        if (CheckerInstance._outputSettings.OutputLockeds)
                            Console.WriteLine(outputString);
                        break;
                    case ComboResult.Invalid:
                        if (CheckerInstance._outputSettings.OutputInvalids)
                            Console.WriteLine(outputString);
                        break;
                    case ComboResult.Hit:
                    default:
                        Console.WriteLine(outputString);
                        break;
                }

                CheckerInstance.Info.LastHit = DateTime.Now;
            }
        }
    }
}

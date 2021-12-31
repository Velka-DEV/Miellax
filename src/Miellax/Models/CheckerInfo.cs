using Miellax.Enums;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Miellax.Models
{
    public class CheckerInfo
    {
        internal CheckerInfo(int credentials)
        {
            Credentials = credentials;
        }

        internal object Locker { get; } = new object();

        internal CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

        public CheckerStatus Status { get; internal set; }

        public int Credentials { get; }

        public List<ICredential> Checked { get; } = new List<ICredential>();

        public int Cpm { get; internal set; }

        public int Hits { get; internal set; }

        public int Free { get; internal set; }

        public int Retries { get; internal set; }

        public int EstimatedHits
        {
            get
            {
                if (Checked.Count == 0 || Hits == 0)
                {
                    return 0;
                }

                return (int)((double)Credentials / Checked.Count * Hits);
            }
        }

        public DateTime Start { get; internal set; }

        public DateTime? End { get; internal set; }

        internal DateTime LastPause { get; set; }

        internal TimeSpan TotalPause { get; set; }

        internal DateTime LastHit { get; set; }

        public TimeSpan Elapsed => TimeSpan.FromSeconds((int)((End ?? DateTime.Now) - Start - TotalPause - (Status == CheckerStatus.Paused ? DateTime.Now - LastPause : TimeSpan.Zero)).TotalSeconds);

        public TimeSpan? Remaining
        {
            get
            {
                try
                {
                    return TimeSpan.FromSeconds((Credentials - Checked.Count) / (Cpm / 60));
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}

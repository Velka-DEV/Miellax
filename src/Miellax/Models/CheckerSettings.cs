namespace Miellax.Models
{
    public class CheckerSettings
    {
        /// <param name="maxThreads">Maximum combos to check concurrently</param>
        /// <param name="useProxies">Whether proxies should be used or not</param>
        public CheckerSettings(
            int maxThreads,
            bool useProxies
            )
        {
            MaxThreads = maxThreads;
            UseProxies = useProxies;
        }

        public int MaxThreads { get; }

        public bool UseProxies { get; }

        public bool UseCookies { get; } = false;

        public bool AllowAutoRedirect { get; set; } = true;

        public int MaxAutomaticRedirections { get; set; } = 10;

        public int MaxAttempts { get; set; } = 5;
    }
}
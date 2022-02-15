using Miellax.Enums;
using Miellax.Exceptions;
using Miellax.Models;
using Miellax.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Miellax
{
    public class CheckerBuilder
    {


        private readonly CheckerSettings _checkerSettings;
        private readonly Func<ICredential, HttpClient, int, Task<CheckResult>> _checkProcess;
        private Action<ICredential, CheckResult> _outputProcess = Checker.OutputProcess;
        private OutputSettings _outputSettings = new OutputSettings();
        private readonly List<ICredential> _combos = new List<ICredential>();
        private readonly Library<HttpClient> _httpClientLibrary = new Library<HttpClient>();
        private readonly Lazy<WebClient> _lazyWebClient = new Lazy<WebClient>();
        private readonly Dictionary<string, string> _defaultRequestHeaders = new Dictionary<string, string>();

        public CheckerBuilder(CheckerSettings checkerSettings, Func<ICredential, HttpClient, int, Task<CheckResult>> checkProcess)
        {
            _checkerSettings = checkerSettings;
            _checkProcess = checkProcess;
        }

        public CheckerBuilder WithOutputSettings(OutputSettings outputSettings)
        {
            _outputSettings = outputSettings;

            return this;
        }

        public CheckerBuilder WithOutputProcess(Action<ICredential, CheckResult> outputProcess)
        {
            _outputProcess = outputProcess;

            return this;
        }

        public CheckerBuilder WithComboCredentials(IEnumerable<string> combos)
        {
            foreach (var combo in combos)
            {
                try
                {
                    _combos.Add(new ComboCredential(combo));
                }
                catch (InvalidComboException) { }
            }

            return this;
        }

        public CheckerBuilder WithCodeCredentials(IEnumerable<string> codes)
        {
            foreach (var code in codes)
            {
                try
                {
                    _combos.Add(new CodeCredential(code));
                }
                catch (InvalidCodeException) { }
            }

            return this;
        }

        public CheckerBuilder WithUrlCredentials(IEnumerable<string> urls)
        {
            foreach (var url in urls)
            {
                try
                {
                    _combos.Add(new UrlCredential(url));
                }
                catch (InvalidComboException) { }
            }

            return this;
        }

        public CheckerBuilder WithCredentials(IEnumerable<string> credentials, CredentialType credentialType, string separator = ":")
        {
            switch (credentialType)
            {
                case CredentialType.Combo:
                    return WithComboCredentials(credentials);
                case CredentialType.Code:
                    return WithCodeCredentials(credentials);
                case CredentialType.Url:
                    return WithUrlCredentials(credentials);
            }

            return this;
        }

        public CheckerBuilder WithProxies(IEnumerable<string> proxies, ProxySettings settings)
        {
            foreach (var proxy in proxies)
            {
                try
                {
                    _httpClientLibrary.Add(HttpClientBuilder.GetHttpClient(_checkerSettings, new Proxy(proxy, settings)));
                }
                catch (InvalidProxyException) { }
            }

            return this;
        }

        public CheckerBuilder WithProxiesFromUrl(string url, ProxySettings settings)
        {
            var responseString = _lazyWebClient.Value.DownloadString(url);

            string[] proxies = responseString.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            WithProxies(proxies, settings);

            return this;
        }

        public CheckerBuilder WithDefaultRequestHeader(string name, string value)
        {
            _defaultRequestHeaders.Add(name, value);

            return this;
        }

        public CheckerBuilder WithDefaultRequestHeaders(IDictionary<string, string> headers)
        {
            foreach (var header in headers)
            {
                WithDefaultRequestHeader(header.Key, header.Value);
            }

            return this;
        }

        public Checker Build()
        {
            SetUpHttpClientLibrary();
            SetUpMiscellaneous();

            return new Checker(_checkerSettings, _outputSettings, _checkProcess, _outputProcess, _combos, _httpClientLibrary);
        }

        private void SetUpHttpClientLibrary()
        {
            if (!_checkerSettings.UseProxies)
            {
                _httpClientLibrary.Items.Clear();

                _httpClientLibrary.Add(new HttpClient(new HttpClientHandler()
                {
                    AllowAutoRedirect = _checkerSettings.AllowAutoRedirect,
                    MaxAutomaticRedirections = _checkerSettings.MaxAutomaticRedirections,
                    UseCookies = _checkerSettings.UseCookies
                }));
            }
            else if (_httpClientLibrary.Items.Count == 0)
            {
                throw new Exception("No (valid) proxy loaded.");
            }
            else
            {
                _httpClientLibrary.Fill(_checkerSettings.MaxThreads * 2);
            }

            foreach (var header in _defaultRequestHeaders)
            {
                foreach (var httpClient in _httpClientLibrary.Items)
                {
                    httpClient.Value.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        private void SetUpMiscellaneous(int extraThreads = 10)
        {
            ThreadPool.SetMinThreads(_checkerSettings.MaxThreads + extraThreads, _checkerSettings.MaxThreads + extraThreads);
            Directory.CreateDirectory(_outputSettings.OutputDirectory);
        }
    }
}
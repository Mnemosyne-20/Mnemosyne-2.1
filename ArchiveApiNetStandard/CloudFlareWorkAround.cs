using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ArchiveApi
{
    // THIS ENTIRE FILE IS A SHAMELESS STEAL FROM https://github.com/elcattivo/CloudFlareUtilities SO THAT IT WORKS ON dotnet v4.6.2
    /// <summary>
    /// The exception that is thrown if CloudFlare clearance failed after the declared number of attempts.
    /// </summary>
    [Serializable]
    internal class CloudFlareClearanceException : HttpRequestException
    {
        public CloudFlareClearanceException(int attempts) : this(attempts, $"Clearance failed after {attempts} attempt(s).") { }

        public CloudFlareClearanceException(int attempts, string message) : base(message)
        {
            Attempts = attempts;
        }

        public CloudFlareClearanceException(int attempts, string message, Exception inner) : base(message, inner)
        {
            Attempts = attempts;
        }
        /// <summary>
        /// Returns the number of failed clearance attempts.
        /// </summary>
        public int Attempts { get; }
    }
    internal static class CookieExtensions
    {
        public static string ToHeaderValue(this Cookie cookie)
        {
            return $"{cookie.Name}={cookie.Value}";
        }

        public static IEnumerable<Cookie> GetCookiesByName(this CookieContainer container, Uri uri, params string[] names)
        {
            return container.GetCookies(uri).Cast<Cookie>().Where(c => names.Contains(c.Name));
        }
    }
    internal static class HttpMessageHandlerExtensions
    {
        public static HttpMessageHandler GetMostInnerHandler(this HttpMessageHandler self)
        {
            return !(self is DelegatingHandler delegatingHandler) ? self : delegatingHandler.InnerHandler.GetMostInnerHandler();
        }
    }
    internal static class HttpHeader
    {
        public const string UserAgent = "User-Agent";

        public const string Cookie = "Cookie";

        public const string SetCookie = "Set-Cookie";

        public const string Refresh = "Refresh";
    }
    /// <summary>
    /// A HTTP handler that transparently manages Cloudflare's Anti-DDoS measure.
    /// </summary>
    /// <remarks>
    /// Only the JavaScript challenge can be handled. CAPTCHA and IP address blocking cannot be bypassed.
    /// </remarks>
    public class ClearanceHandler : DelegatingHandler
    {
        /// <summary>
        /// The default number of retries, if clearance fails.
        /// </summary>
        public static readonly int DefaultMaxRetries = 3;

        private const string CloudFlareServerName = "cloudflare-nginx";
        private const string IdCookieName = "__cfduid";
        private const string ClearanceCookieName = "cf_clearance";

        private readonly CookieContainer _cookies = new CookieContainer();
        private readonly HttpClient _client;

        /// <summary>
        /// Creates a new instance of the <see cref="ClearanceHandler"/> class with a <see cref="HttpClientHandler"/> as inner handler.
        /// </summary>
        public ClearanceHandler() : this(new HttpClientHandler()) { }

        /// <summary>
        /// Creates a new instance of the <see cref="ClearanceHandler"/> class with a specific inner handler.
        /// </summary>
        /// <param name="innerHandler">The inner handler which is responsible for processing the HTTP response messages.</param>
        public ClearanceHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
            _client = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = false,
                CookieContainer = _cookies
            });
        }

        /// <summary>
        /// Gets or sets the number of clearance retries, if clearance fails.
        /// </summary>
        /// <remarks>A negative value causes an infinite amount of retries.</remarks>
        public int MaxRetries { get; set; } = DefaultMaxRetries;

        private HttpClientHandler ClientHandler => InnerHandler.GetMostInnerHandler() as HttpClientHandler;

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ClearanceHandler"/>, and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to releases only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _client.Dispose();

            base.Dispose(disposing);
        }

        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var IdCookieBefore = ClientHandler.CookieContainer.GetCookiesByName(request.RequestUri, IdCookieName).FirstOrDefault();
            var clearanceCookieBefore = ClientHandler.CookieContainer.GetCookiesByName(request.RequestUri, ClearanceCookieName).FirstOrDefault();

            EnsureClientHeader(request);
            InjectCookies(request);

            var response = await base.SendAsync(request, cancellationToken);

            // (Re)try clearance if required.
            var retries = 0;
            while (IsClearanceRequired(response) && (MaxRetries < 0 || retries <= MaxRetries))
            {
                cancellationToken.ThrowIfCancellationRequested();

                await PassClearance(response, cancellationToken);
                InjectCookies(request);
                response = await base.SendAsync(request, cancellationToken);

                retries++;
            }

            // Clearance failed.
            if (IsClearanceRequired(response))
                throw new CloudFlareClearanceException(retries);

            var IdCookieAfter = ClientHandler.CookieContainer.GetCookiesByName(request.RequestUri, IdCookieName).FirstOrDefault();
            var clearanceCookieAfter = ClientHandler.CookieContainer.GetCookiesByName(request.RequestUri, ClearanceCookieName).FirstOrDefault();

            // inject set-cookie headers in case the cookies changed
            if (IdCookieAfter != null && IdCookieAfter != IdCookieBefore)
            {
                response.Headers.Add(HttpHeader.SetCookie, IdCookieAfter.ToHeaderValue());
            }
            if (clearanceCookieAfter != null && clearanceCookieAfter != clearanceCookieBefore)
            {
                response.Headers.Add(HttpHeader.SetCookie, clearanceCookieAfter.ToHeaderValue());
            }

            return response;
        }

        private static void EnsureClientHeader(HttpRequestMessage request)
        {
            if (!request.Headers.UserAgent.Any())
                request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Client", "1.0"));
        }

        private static bool IsClearanceRequired(HttpResponseMessage response)
        {
            var isServiceUnavailable = response.StatusCode == HttpStatusCode.ServiceUnavailable;
            var isCloudFlareServer = response.Headers.Server.Any(i => i.Product != null && i.Product.Name == CloudFlareServerName);

            return isServiceUnavailable && isCloudFlareServer;
        }

        private void InjectCookies(HttpRequestMessage request)
        {
            var cookies = _cookies.GetCookies(request.RequestUri).Cast<Cookie>().ToList();
            var idCookie = cookies.FirstOrDefault(c => c.Name == IdCookieName);
            var clearanceCookie = cookies.FirstOrDefault(c => c.Name == ClearanceCookieName);

            if (idCookie == null || clearanceCookie == null)
                return;

            if (ClientHandler.UseCookies)
            {
                ClientHandler.CookieContainer.Add(request.RequestUri, idCookie);
                ClientHandler.CookieContainer.Add(request.RequestUri, clearanceCookie);
            }
            else
            {
                request.Headers.Add(HttpHeader.Cookie, idCookie.ToHeaderValue());
                request.Headers.Add(HttpHeader.Cookie, clearanceCookie.ToHeaderValue());
            }
        }

        private async Task PassClearance(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            SaveIdCookie(response);

            var pageContent = await response.Content.ReadAsStringAsync();
            var scheme = response.RequestMessage.RequestUri.Scheme;
            var host = response.RequestMessage.RequestUri.Host;
            var port = response.RequestMessage.RequestUri.Port;
            var solution = ChallengeSolver.Solve(pageContent, host);

            var clearanceUri = $"{scheme}://{host}:{port}{solution.ClearanceQuery}";

            await Task.Delay(5000, cancellationToken);

            using (var clearanceRequest = new HttpRequestMessage(HttpMethod.Get, clearanceUri))
            {
                if (response.RequestMessage.Headers.TryGetValues(HttpHeader.UserAgent, out IEnumerable<string> userAgent))
                    clearanceRequest.Headers.Add(HttpHeader.UserAgent, userAgent);

                var passResponse = await _client.SendAsync(clearanceRequest, cancellationToken);

                SaveIdCookie(passResponse); // new ID might be set as a response to the challenge in some cases
            }
        }

        private void SaveIdCookie(HttpResponseMessage response)
        {
            var cookies = response.Headers
                .Where(pair => pair.Key == HttpHeader.SetCookie)
                .SelectMany(pair => pair.Value)
                .Where(cookie => cookie.StartsWith($"{IdCookieName}="));

            if (cookies.Count() == 0)
                return;

            // Expire any old cloudflare cookies.
            // If e.g.the cookie domain / path changed we'll have duplicate cookies (breaking the clearance) in some cases
            var oldCookies = ClientHandler.CookieContainer.GetCookies(response.RequestMessage.RequestUri);
            foreach (Cookie oldCookie in oldCookies)
            {
                if (oldCookie.Name == IdCookieName || oldCookie.Name == ClearanceCookieName)
                    oldCookie.Expired = true;
            }

            foreach (var cookie in cookies)
                _cookies.SetCookies(response.RequestMessage.RequestUri, cookie);
        }
    }
    /// <summary>
    /// Provides methods to solve the JavaScript challenge, which is part of CloudFlares clearance process.
    /// </summary>
    internal static class ChallengeSolver
    {
        private const string ScriptTagPattern = @"<script\b[^>]*>(?<Content>.*?)<\/script>";
        private const string ObfuscatedNumberPattern = @"(?<Number>[\(\)\+\!\[\]]+)";
        private const string SimplifiedObfuscatedDigitPattern = @"\([1+[\]]+\)";
        private const string SeedPattern = ":" + ObfuscatedNumberPattern;
        private const string OperatorPattern = @"(?<Operator>[\+\-\*\/]{1})\=";
        private const string StepPattern = OperatorPattern + ObfuscatedNumberPattern;

        /// <summary>
        /// Solves the given JavaScript challenge.
        /// </summary>
        /// <param name="challengePageContent">The HTML content of the clearance page, which contains the challenge.</param>
        /// <param name="targetHost">The hostname of the protected website.</param>
        /// <returns>The solution.</returns>
        public static ChallengeSolution Solve(string challengePageContent, string targetHost)
        {
            var jschlAnswer = DecodeSecretNumber(challengePageContent, targetHost);
            var jschlVc = Regex.Match(challengePageContent, "name=\"jschl_vc\" value=\"(?<jschl_vc>[^\"]+)").Groups["jschl_vc"].Value;
            var pass = Regex.Match(challengePageContent, "name=\"pass\" value=\"(?<pass>[^\"]+)").Groups["pass"].Value;
            var clearancePage = Regex.Match(challengePageContent, "id=\"challenge-form\" action=\"(?<action>[^\"]+)").Groups["action"].Value;

            return new ChallengeSolution(clearancePage, jschlVc, pass, jschlAnswer);
        }

        private static int DecodeSecretNumber(string challengePageContent, string targetHost)
        {
            var challengeScript = Regex.Matches(challengePageContent, ScriptTagPattern, RegexOptions.Singleline)
                .Cast<Match>().Select(m => m.Groups["Content"].Value)
                .First(c => c.Contains("jschl-answer"));
            var seed = DeobfuscateNumber(Regex.Match(challengeScript, SeedPattern).Groups["Number"].Value);
            var steps = Regex.Matches(challengeScript, StepPattern).Cast<Match>()
                .Select(s => new Tuple<string, int>(s.Groups["Operator"].Value, DeobfuscateNumber(s.Groups["Number"].Value)));
            var secretNumber = steps.Aggregate(seed, ApplyDecodingStep) + targetHost.Length;

            return secretNumber;
        }

        private static int DeobfuscateNumber(string obfuscatedNumber)
        {
            var simplifiedObfuscatedNumber = SimplifyObfuscatedNumber(obfuscatedNumber);

            if (!simplifiedObfuscatedNumber.Contains("("))
                return CountOnes(simplifiedObfuscatedNumber);

            var digitMatches = Regex.Matches(simplifiedObfuscatedNumber, SimplifiedObfuscatedDigitPattern);
            var numberAsText = digitMatches.Cast<Match>()
                .Select(m => CountOnes(m.Value))
                .Aggregate(string.Empty, (number, digit) => number + digit);

            return int.Parse(numberAsText);
        }

        private static string SimplifyObfuscatedNumber(string obfuscatedNumber)
        {
            return obfuscatedNumber.Replace("!![]", "1").Replace("!+[]", "1");
        }

        private static int CountOnes(string obfuscatedNumber)
        {
            return obfuscatedNumber.ToCharArray().Count(c => c == '1');
        }

        private static int ApplyDecodingStep(int number, Tuple<string, int> step)
        {
            var op = step.Item1;
            var operand = step.Item2;

            switch (op)
            {
                case "+":
                    return number + operand;
                case "-":
                    return number - operand;
                case "*":
                    return number * operand;
                case "/":
                    return number / operand;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown operator: {op}");
            }
        }
    }
    /// <summary>
    /// Holds the information, which is required to pass the CloudFlare clearance.
    /// </summary>
    internal struct ChallengeSolution : IEquatable<ChallengeSolution>
    {
        public ChallengeSolution(string clearancePage, string verificationCode, string pass, int answer)
        {
            ClearancePage = clearancePage;
            VerificationCode = verificationCode;
            Pass = pass;
            Answer = answer;
        }

        public string ClearancePage { get; }

        public string VerificationCode { get; }

        public string Pass { get; }

        public int Answer { get; }

        public string ClearanceQuery => $"{ClearancePage}?jschl_vc={VerificationCode}&pass={Pass}&jschl_answer={Answer}";

        public static bool operator ==(ChallengeSolution solutionA, ChallengeSolution solutionB)
        {
            return solutionA.Equals(solutionB);
        }

        public static bool operator !=(ChallengeSolution solutionA, ChallengeSolution solutionB)
        {
            return !(solutionA == solutionB);
        }

        public override bool Equals(object obj)
        {
            var other = obj as ChallengeSolution?;
            return other.HasValue && Equals(other.Value);
        }

        public override int GetHashCode()
        {
            return ClearanceQuery.GetHashCode();
        }

        public bool Equals(ChallengeSolution other)
        {
            return other.ClearanceQuery == ClearanceQuery;
        }
    }
}
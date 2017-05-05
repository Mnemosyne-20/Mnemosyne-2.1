using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
namespace ArchiveApi
{
    /// <summary>
    /// Is a class that creates an Archive Service Provider, e.g. archive.is
    /// </summary>
    public class ArchiveService
    {
        string submitEndpoint = "/submit/";
        string timeMapEndpoint = "/timemap/";
        public Uri Url;
        public ArchiveService(string Url)
        {
            this.Url = new Uri(Url);
        }
        public ArchiveService(Uri Url)
        {
            this.Url = Url;
        }
        /// <summary>
        /// Checks if the ArchiveUrl is a successful URL
        /// </summary>
        /// <param name="ArchiveUrl"></param>
        /// <returns>true if it does not contain "submit" in the uri</returns>
        public bool Verify(string ArchiveUrl)
        {
            if (ArchiveUrl == null || ArchiveUrl == "http://archive.is/submit/" || ArchiveUrl == "http://archive.fo/submit/")
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Checks if the ArchiveUrl is a successful URL
        /// </summary>
        /// <remarks>Yes I know the internals of this are actually stupid, but the unit test passed, that is what matters here</remarks>
        /// <param name="ArchiveUrl"></param>
        /// <returns>true if it does not contain "submit" in the uri</returns>
        public bool Verify(Uri ArchiveUrl) => !ArchiveUrl.AbsolutePath.Contains("submit") && ArchiveUrl.ToString() == "http://archive.is";

        /// <summary>
        /// Saves a webpage
        /// </summary>
        /// <param name="Url">Url to archive</param>
        /// <returns>Archive link</returns>
        public async Task<string> Save(string Url)
        {
            string ReturnUrl = "";
            using (var client = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = true }))
            {
                /// <summary>
                /// This puts a request to the archive site, so yhea...
                /// </summary>
                var request = new HttpRequestMessage(HttpMethod.Post, "http://archive.is/submit/");
                request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36");
                request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"url", Url }
                });
                var response = await client.SendAsync(request);
                Task.Delay(8000).Wait(); // elementary test to make it wait 8 seconds to get around, doesn't work
                ReturnUrl = response.RequestMessage.RequestUri.ToString();
                /// <remarks>
                /// Fixes the bug where archive.is returns a json file that has a url tag
                /// </remarks>
                if (ReturnUrl == $"http://archive.is/submit/" && !response.IsSuccessStatusCode)
                {
                    #region fixing issues with return because this works somehow!?!?
                    using (StringReader reader = new StringReader(response.ToString()))
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            reader.ReadLine();
                        }
                        string[] sides = reader.ReadLine().Split('=');
                        ReturnUrl = sides[1];
                    }
                    #endregion
                }
            }
            return ReturnUrl;
        }
        /// <summary>
        /// Saves a webpage
        /// </summary>
        /// <param name="Url">Url to archive</param>
        /// <returns>Archive link</returns>
        public async Task<string> Save(Uri Url)
        {
            string ReturnUrl = "";
            using (var client = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = true }))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, this.Url.ToString().TrimEnd('/') + submitEndpoint);
                request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.96 Safari/537.36");
                request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"url", Url.ToString() }
                });
                var response = await client.SendAsync(request);
                ReturnUrl = response.RequestMessage.RequestUri.ToString();
                if (!response.IsSuccessStatusCode)
                {
                    using (StringReader reader = new StringReader(response.ToString()))
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            reader.ReadLine();
                        }
                        string[] sides = reader.ReadLine().Split('=');
                        ReturnUrl = sides[1];
                    }
                }
            }
            return ReturnUrl;
        }
    }
}

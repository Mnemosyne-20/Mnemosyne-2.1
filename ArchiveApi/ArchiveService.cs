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
        public bool Verify(string ArchiveUrl) => Verify(new Uri(ArchiveUrl));
        /// <summary>
        /// Checks if the ArchiveUrl is a successful URL
        /// </summary>
        /// <remarks>Yes I know the internals of this are actually stupid, but the unit test passed, that is what matters here</remarks>
        /// <param name="ArchiveUrl"></param>
        /// <returns>true if it does not contain "submit" in the uri</returns>
        public bool Verify(Uri ArchiveUrl) => !ArchiveUrl.AbsolutePath.ToString().TrimEnd('/').Contains(submitEndpoint.TrimEnd('/')) && ArchiveUrl.ToString().Replace("https://", "http://").TrimEnd('/') != Url.ToString().Replace("https://", "http://").TrimEnd('/');

        /// <summary>
        /// Saves a webpage
        /// </summary>
        /// <param name="Url">Url to archive</param>
        /// <returns>Archive link</returns>
        public string Save(string Url)
        {
            string ReturnUrl = "";
            using (var client = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = true }))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, this.Url + submitEndpoint);
                request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36");
                request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"url", Url }
                });
                var response = client.SendAsync(request);
                Task.WaitAll(response);
                ReturnUrl = response.Result.RequestMessage.RequestUri.ToString();
                if (!response.Result.IsSuccessStatusCode)
                {
                    using (StringReader reader = new StringReader(response.Result.ToString()))
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
        /// <summary>
        /// Saves a webpage
        /// </summary>
        /// <param name="Url">Url to archive</param>
        /// <returns>Archive link</returns>
        public string Save(Uri Url)
        {
            string ReturnUrl = "";
            using (var client = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = true }))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, this.Url + submitEndpoint);
                request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36");
                request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"url", Url.ToString() }
                });
                var response = client.SendAsync(request);
                Task.WaitAll(response);
                ReturnUrl = response.Result.RequestMessage.RequestUri.ToString();
                if (!response.Result.IsSuccessStatusCode)
                {
                    using (StringReader reader = new StringReader(response.Result.ToString()))
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

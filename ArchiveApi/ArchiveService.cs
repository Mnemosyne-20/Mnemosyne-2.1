using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.IO;

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
            return (!new Uri(ArchiveUrl).LocalPath.Contains(submitEndpoint));
        }
        public bool Verify(Uri ArchiveUrl)
        {
            return !ArchiveUrl.LocalPath.Contains(submitEndpoint);
        }
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
                var request = new HttpRequestMessage(HttpMethod.Post, Url);
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

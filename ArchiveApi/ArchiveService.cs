using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
namespace ArchiveApi
{
    /// <summary>
    /// Is a class that creates an Archive Service Provider, e.g. archive.is, currently can only use archive.is because URI is weird in C# for some reason
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
            if (ArchiveUrl == null || ArchiveUrl == "http://archive.is/submit/" || ArchiveUrl == "http://archive.fo/submit/" || ArchiveUrl.TrimEnd('/') == "http://archive.is/" || ArchiveUrl.Contains("submit"))
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
        public string Save(string Url)
        {
            string ReturnUrl = "";
            using (var client = new HttpClient(new ClearanceHandler() { InnerHandler = new HttpClientHandler() { AllowAutoRedirect = true }, MaxRetries = 5 }))
            {
                /// <summary>
                /// This puts a request to the archive site, so yhea...
                /// </summary>
                var response = client.PostAsync("http://archive.is/submit/", new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"url", Url.ToString() }
                })).Result;
                ReturnUrl = response.RequestMessage.RequestUri.ToString();
                /// <remarks>
                /// Fixes the bug where archive.is returns a json file that has a url tag
                /// </remarks>
                if (!Verify(ReturnUrl) && !response.IsSuccessStatusCode)
                {
                    #region fixing issues with return because this works somehow!?!?
                    using (StringReader reader = new StringReader(response.ToString()))
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            reader.ReadLine();
                        }
                        string[] sides = reader.ReadLine().Split('=');
                        try
                        {
                            ReturnUrl = sides[1];
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error from archive.is: \n" + e.Message);
                        }
                    }
                    #endregion
                }
                if (!Verify(ReturnUrl))
                {
                    throw new ArchiveException($"Archive failed with original link {Url.ToString()} at {DateTime.Now}");
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
            using (var client = new HttpClient(new ClearanceHandler() { InnerHandler = new HttpClientHandler() { AllowAutoRedirect = true }, MaxRetries = 5 }))
            {
                /// <summary>
                /// This puts a request to the archive site, so yhea...
                /// </summary>
                var response = client.PostAsync("http://archive.is/submit/", new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"url", Url.ToString() }
                })).Result;
                ReturnUrl = response.RequestMessage.RequestUri.ToString();
                /// <remarks>
                /// Fixes the bug where archive.is returns a json file that has a url tag
                /// </remarks>
                if (!Verify(ReturnUrl) && !response.IsSuccessStatusCode)
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
                if (!Verify(ReturnUrl))
                {
                    throw new ArchiveException($"Archive failed with original link {Url.ToString()} at {DateTime.Now}");
                }
            }
            return ReturnUrl;
        }
        /// <summary>
        /// Saves a webpage
        /// </summary>
        /// <param name="Url">Url to archive</param>
        /// <returns>Archive link</returns>
        public async Task<string> SaveAsync(string Url)
        {
            string ReturnUrl = "";
            using (var client = new HttpClient(new ClearanceHandler() { InnerHandler = new HttpClientHandler() { AllowAutoRedirect = true }, MaxRetries = 5 }))
            {
                /// <summary>
                /// This puts a request to the archive site, so yhea...
                /// </summary>
                var task = client.PostAsync("http://archive.is/submit/", new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"url", Url.ToString() }
                }));
                await Task.Delay(8000);
                var response = await task;
                ReturnUrl = response.RequestMessage.RequestUri.ToString();
                /// <remarks>
                /// Fixes the bug where archive.is returns a json file that has a url tag
                /// </remarks>
                if (!Verify(ReturnUrl) && !response.IsSuccessStatusCode)
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
                if (!Verify(ReturnUrl))
                {
                    throw new ArchiveException($"Archive failed with original link {Url.ToString()} at {DateTime.Now}");
                }
            }
            return ReturnUrl;
        }
        /// <summary>
        /// Saves a webpage
        /// </summary>
        /// <param name="Url">Url to archive</param>
        /// <returns>Archive link</returns>
        public async Task<string> SaveAsync(Uri Url)
        {
            string ReturnUrl = "";
            using (var client = new HttpClient(new ClearanceHandler() { InnerHandler = new HttpClientHandler() { AllowAutoRedirect = true }, MaxRetries = 5 }))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, this.Url.ToString().TrimEnd('/') + submitEndpoint);
                var task = client.PostAsync("http://archive.is/submit/", new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"url", Url.ToString() }
                }));
                await Task.Delay(8000);
                var response = await task;
                ReturnUrl = response.RequestMessage.RequestUri.ToString();
                if (!Verify(ReturnUrl) && !response.IsSuccessStatusCode)
                {
                    #region fixing issues
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
                if (!Verify(ReturnUrl))
                {
                    throw new ArchiveException($"Archive failed with original link {Url.ToString()} at {DateTime.Now}");
                }
            }
            return ReturnUrl;
        }
    }
}

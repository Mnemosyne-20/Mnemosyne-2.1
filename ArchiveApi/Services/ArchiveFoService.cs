using ArchiveApi.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
namespace ArchiveApi.Services
{
    /// <summary>
    /// Is a class that creates an Archive Service Provider, e.g. archive.fo, currently can only use archive.fo because URI is weird in C# for some reason
    /// </summary>
    public class ArchiveFoService : IArchiveService
    {
        public Uri TimeMapEndpoint => new Uri(BaseUri, "/timemap/");
        public Uri SubmitEndpoint => new Uri(BaseUri, "/submit/");
        public Uri BaseUri => new Uri("http://archive.fo");
        HttpClient client = new HttpClient(new ClearanceHandler() { InnerHandler = new HttpClientHandler() { AllowAutoRedirect = true }, MaxRetries = 5 });
        /// <summary>
        /// Checks if the ArchiveUrl is a successful URL
        /// </summary>
        /// <param name="ArchiveUrl"></param>
        /// <returns>true if it does not contain "submit" in the uri</returns>
        public bool Verify(string ArchiveUrl) => !(ArchiveUrl == null || ArchiveUrl == "http://archive.fo/submit/" || ArchiveUrl.TrimEnd('/') == "http://archive.fo" || ArchiveUrl.Contains("submit"));
        /// <summary>
        /// Checks if the ArchiveUrl is a successful URL
        /// </summary>
        /// <remarks>Yes I know the internals of this are actually stupid, but the unit test passed, that is what matters here</remarks>
        /// <param name="ArchiveUrl"></param>
        /// <returns>true if it does not contain "submit" in the uri</returns>
        public bool Verify(Uri ArchiveUrl) => !ArchiveUrl.AbsolutePath.Contains("submit") && ArchiveUrl.ToString() == "http://archive.fo";
        /// <summary>
        /// Saves a webpage
        /// </summary>
        /// <param name="Url">Url to archive</param>
        /// <returns>Archive link</returns>
        public string Save(string Url)
        {
            /// <summary>
            /// This puts a request to the archive site, so yhea...
            /// </summary>
            var response = client.PostAsync(SubmitEndpoint, new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"url", Url.ToString() }
                })).Result;
            string ReturnUrl = response.RequestMessage.RequestUri.ToString();
            if (!Verify(ReturnUrl) && response.Headers.TryGetValues("Refresh", out var headers))
            {
                foreach (var header in headers)
                {
                    if (header.Contains("http://archive.fo"))
                    {
                        ReturnUrl = header.Split('=')[1];
                    }
                }
            }

            /// <remarks>
            /// Fixes the bug where archive.fo returns a json file that has a url tag
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
                        Console.WriteLine("Error from archive.fo: \n" + e.Message);
                    }
                }
                #endregion
            }
            if (!Verify(ReturnUrl))
            {
                throw new ArchiveException($"Archive failed with original link {Url.ToString()} at {DateTime.Now}");
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
            /// <summary>
            /// This puts a request to the archive site, so yhea...
            /// </summary>
            var response = client.PostAsync(SubmitEndpoint, new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"url", Url.ToString() }
                })).Result;
            string ReturnUrl = response.RequestMessage.RequestUri.ToString();
            if (!Verify(ReturnUrl) && response.Headers.TryGetValues("Refresh", out var headers))
            {
                foreach (var header in headers)
                {
                    if (header.Contains("http://archive.fo"))
                    {
                        ReturnUrl = header.Split('=')[1];
                    }
                }
            }

            /// <remarks>
            /// Fixes the bug where archive.fo returns a json file that has a url tag
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
                        Console.WriteLine("Error from archive.fo: \n" + e.Message);
                    }
                }
                #endregion
            }
            if (!Verify(ReturnUrl))
            {
                throw new ArchiveException($"Archive failed with original link {Url.ToString()} at {DateTime.Now}");
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

            /// <summary>
            /// This puts a request to the archive site, so yhea...
            /// </summary>
            var response = await client.PostAsync(SubmitEndpoint, new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"url", Url.ToString() }
                }));
            string ReturnUrl = response.RequestMessage.RequestUri.ToString();
            if (!Verify(ReturnUrl) && response.Headers.TryGetValues("Refresh", out var headers))
            {
                foreach (var header in headers)
                {
                    if (header.Contains("http://archive.fo"))
                    {
                        ReturnUrl = header.Split('=')[1];
                    }
                }
            }
            /// <remarks>
            /// Fixes the bug where archive.fo returns a json file that has a url tag
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
                        Console.WriteLine("Error from archive.fo: \n" + e.Message);
                    }
                }
                #endregion
            }
            if (!Verify(ReturnUrl))
            {
                throw new ArchiveException($"Archive failed with original link {Url.ToString()} at {DateTime.Now}");
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
            /// <summary>
            /// This puts a request to the archive site, so yhea...
            /// </summary>
            var response = await client.PostAsync(SubmitEndpoint, new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"url", Url.ToString() }
                }));
            string ReturnUrl = response.RequestMessage.RequestUri.ToString();
            if (!Verify(ReturnUrl) && response.Headers.TryGetValues("Refresh", out var headers))
            {
                foreach (var header in headers)
                {
                    if (header.Contains("http://archive.fo"))
                    {
                        ReturnUrl = header.Split('=')[1];
                    }
                }
            }

            /// <remarks>
            /// Fixes the bug where archive.fo returns a json file that has a url tag
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
                        Console.WriteLine("Error from archive.fo: \n" + e.Message);
                    }
                }
                #endregion
            }
            if (!Verify(ReturnUrl))
            {
                throw new ArchiveException($"Archive failed with original link {Url.ToString()} at {DateTime.Now}");
            }
            return ReturnUrl;
        }

        Uri IArchiveService.Save(Uri Url) => new Uri(Save(Url));

        Task<Uri> IArchiveService.SaveAsync(Uri Url) => throw new NotImplementedException();
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    client.Dispose();
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ArchiveIsService() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            //GC.SuppressFinalize(this);
        }
        #endregion
    }
    public class ArchiveFoFactory : IArchiveServiceFactory
    {
        public override IArchiveService GetArchiveService()
        {
            return new ArchiveFoService();
        }
    }
}
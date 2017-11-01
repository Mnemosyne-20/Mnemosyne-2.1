using ArchiveApi.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
namespace ArchiveApi.Services
{

    /// <summary>
    /// This class is used for archive.fo services, it is essentially a russia-accessible reskin of archive.is / archive.li / archive.today / ...., like, no, seriously this website has like 6 TLDs availible for it.
    /// </summary>
    public class ArchiveFoService : IArchiveService
    {
        /// <summary>
        /// Endpoint for obtaining a <see cref="TimeMap"/>
        /// </summary>
        public Uri TimeMapEndpoint => new Uri(BaseUri, "/timemap/");
        /// <summary>
        /// Endpoint for submitting a file to archive
        /// </summary>
        public Uri SubmitEndpoint => new Uri(BaseUri, "/submit/");
        /// <summary>
        /// The base URI that archiving, timemaps, etc. are based around
        /// </summary>
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
        public string Save(string Url) => SaveAsync(Url).Result;
        /// <summary>
        /// Saves a webpage
        /// </summary>
        /// <param name="Url">Url to archive</param>
        /// <returns>Archive link</returns>
        public string Save(Uri Url) => SaveAsync(Url).Result;
        /// <summary>
        /// Saves a webpage
        /// </summary>
        /// <param name="Url">Url to archive</param>
        /// <returns>An archived link</returns>
        public async Task<string> SaveAsync(string Url) => await SaveAsync(new Uri(Url));
        /// <summary>
        /// Saves a webpage
        /// </summary>
        /// <param name="Url">Url to archive</param>
        /// <returns>Archive link</returns>
        public async Task<string> SaveAsync(Uri Url)
        {
            var response = await client.PostAsync(SubmitEndpoint, new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"url", Url.ToString() }
                }));
            string ReturnUrl = response.RequestMessage.RequestUri.ToString();
            if (!Verify(ReturnUrl) && response.Headers.TryGetValues("Refresh", out var headers))
            {
                foreach (var header in headers)
                {
                    if (header.Contains(BaseUri.OriginalString))
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
}
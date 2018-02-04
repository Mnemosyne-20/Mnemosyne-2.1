using ArchiveApi.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
namespace ArchiveApi.Services
{
    public class ArchiveTodayInternal : IArchiveService, IDisposable
    {
        private string _tld;
        private static List<string> _tlds = new List<string>();
        private string TldRegexBuilder
        {
            get
            {
                string app = "";
                for (int i = 0; i < _tlds.Count; i++)
                {
                    if (i == 0)
                    {
                        app += _tlds[i];
                    }
                    else
                    {
                        app += $"|{_tlds[i]}";
                    }
                }
                return $"({app})";
            }
        }
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
        public Uri BaseUri => internalBase;
        private Uri internalBase;
        internal ArchiveTodayInternal(string TLD)
        {
            internalBase = new Uri($"http://archive.{TLD}");
            _tld = TLD;
            if (!_tlds.Contains(_tld)) _tlds.Add(_tld);
        }
        HttpClient client = new HttpClient(new ClearanceHandler() { InnerHandler = new HttpClientHandler() { AllowAutoRedirect = true }, MaxRetries = 5 });
        /// <summary>
        /// Checks if the ArchiveUrl is a successful URL
        /// </summary>
        /// <param name="ArchiveUrl">A string that is a valid URI to check if it is valid</param>
        /// <returns>False if it is an archive website that isn't an actual webpage</returns>
        public bool Verify(string ArchiveUrl) => Verify(new Uri(ArchiveUrl));
        /// <summary>
        /// Checks if the ArchiveUrl is a successful URL
        /// </summary>
        /// <param name="ArchiveUrl"></param>
        /// <returns>False if it is an archive website that isn't an actual webpage</returns>
        public bool Verify(Uri ArchiveUrl)
        {
            return Regex.IsMatch(ArchiveUrl.ToString(), $"https?://archive\\.{TldRegexBuilder}") && !ArchiveUrl.AbsolutePath.Contains("submit") && ArchiveUrl.AbsolutePath != "/";
        }

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
        /// <returns>Archive link</returns>
        public async Task<string> SaveAsync(string Url) => await SaveAsync(new Uri(Url));
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
                    if (header.Contains(BaseUri.OriginalString))
                    {
                        ReturnUrl = header.Split('=')[1];
                    }
                }
            }

            /// <remarks>
            /// Fixes the bug where archive.is returns a json file that has a url tag
            /// </remarks>
            if (!Verify(ReturnUrl) && !response.IsSuccessStatusCode)
            {
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
                        Console.WriteLine($"Error from archive.{_tld}: \n" + e.Message);
                    }
                }
            }
            if (!Verify(ReturnUrl))
            {
                throw new ArchiveException($"Archive failed with original link {Url.ToString()} at {DateTime.Now}, link gotten: {ReturnUrl}.");
            }
            return ReturnUrl;
        }

        Uri IArchiveService.Save(Uri Url) => new Uri(Save(Url));

        async Task<Uri> IArchiveService.SaveAsync(Uri Url) => new Uri(await SaveAsync(Url));
        public void ClearDomains() => _tlds.RemoveAll(a => a != _tld);
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

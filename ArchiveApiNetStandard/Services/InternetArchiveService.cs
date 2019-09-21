using ArchiveApi.Interfaces;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArchiveApi.Services
{
    [Obsolete("In development, do not use")]
    public class InternetArchiveService : IArchiveService
    {
        public Uri TimeMapEndpoint => new Uri(BaseUri, "/web/timemap/link/");

        public Uri BaseUri => new Uri("https://web.archive.org/");

        public Uri SubmitEndpoint => new Uri(BaseUri, "/save/");

        HttpClient client = new HttpClient();

        public string Save(string Url) => SaveAsync(Url).Result;

        public Uri Save(Uri Url) => SaveAsync(Url).Result;

        public Task<string> SaveAsync(string Url)
        {
#pragma warning disable 0162
            throw new NotImplementedException();
            var response = client.GetAsync(new Uri(SubmitEndpoint, Url));
#pragma warning restore
        }

        public Task<Uri> SaveAsync(Uri Url)
        {
            throw new NotImplementedException();
        }

        public bool Verify(string Url) => Verify(new Uri(Url));

        public bool Verify(Uri Url)
        {
            throw new NotImplementedException();
        }
        #region IDisposable
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

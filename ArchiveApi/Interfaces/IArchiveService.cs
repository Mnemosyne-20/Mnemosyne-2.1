using System;
using System.Threading.Tasks;

namespace ArchiveApi.Interfaces
{
    public interface IArchiveService : IDisposable
    {
        Uri TimeMapEndpoint { get; }
        Uri BaseUri { get; }
        bool Verify(string Url);
        bool Verify(Uri Url);
        string Save(string Url);
        Uri Save(Uri Url);
        Task<string> SaveAsync(string Url);
        Task<Uri> SaveAsync(Uri Url);
    }
}
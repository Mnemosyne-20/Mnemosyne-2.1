using ArchiveApi.Interfaces;
using ArchiveApi.Services;
namespace ArchiveApi
{
    public enum DefaultServices
    {
        ArchiveIs,
        ArchiveFo
    }
    public class ArchiveService : IArchiveServiceFactory
    {
        DefaultServices service;
        public ArchiveService(DefaultServices service = DefaultServices.ArchiveFo) => this.service = service;
        public ArchiveService(string service)
        {
            if (service.Contains("archive.is"))
            {
                this.service = DefaultServices.ArchiveIs;
            }
            else if (service.Contains("archive.fo"))
            {
                this.service = DefaultServices.ArchiveFo;
            }
        }
        public override IArchiveService CreateNewService()
        {
            switch (service)
            {
                case DefaultServices.ArchiveIs:
                    return new ArchiveIsService();
                case DefaultServices.ArchiveFo:
                default:
                    return new ArchiveFoService();
            }
        }
    }
}

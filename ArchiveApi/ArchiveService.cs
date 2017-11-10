using ArchiveApi.Interfaces;
using ArchiveApi.Services;
namespace ArchiveApi
{
    public enum DefaultServices
    {
        ArchiveIs,
        ArchiveFo,
        ArchiveLi
    }
    public class ArchiveService : IArchiveServiceFactory
    {
        DefaultServices service;
        public ArchiveService(DefaultServices service = DefaultServices.ArchiveFo) => this.service = service;
        public ArchiveService(string service)
        {
            switch(service.ToLower())
            {
                case "archive.is":
                    this.service = DefaultServices.ArchiveIs;
                    break;
                case "archive.fo":
                    this.service = DefaultServices.ArchiveFo;
                    break;
                case "archive.li":
                    this.service = DefaultServices.ArchiveLi;
                    break;
                default:
                    this.service = DefaultServices.ArchiveFo;
                    break;
            }
        }
        public override IArchiveService CreateNewService()
        {
            switch (service)
            {
                case DefaultServices.ArchiveIs:
                    return new ArchiveIsService();
                case DefaultServices.ArchiveFo:
                    return new ArchiveFoService();
                case DefaultServices.ArchiveLi:
                    return new ArchiveLiService();
                default:
                    return new ArchiveFoService();
            }
        }
    }
}

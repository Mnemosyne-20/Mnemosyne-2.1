using ArchiveApi.Interfaces;
using ArchiveApi.Services;
namespace ArchiveApi
{
    public enum DefaultServices
    {
        ArchiveIs,
        ArchiveFo,
        ArchiveLi,
        ArchivePh,
        ArchiveVn,
        ArchiveMd,
        ArchiveToday
    }
    public class ArchiveService : IArchiveServiceFactory
    {
        readonly DefaultServices service;
        public ArchiveService(DefaultServices service = DefaultServices.ArchiveFo) => this.service = service;
        public ArchiveService(string service)
        {
            switch (service.ToLower())
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
                case "archive.vn":
                    this.service = DefaultServices.ArchiveVn;
                    break;
                case "archive.ph":
                    this.service = DefaultServices.ArchivePh;
                    break;
                case "archive.md":
                    this.service = DefaultServices.ArchiveMd;
                    break;
                case "archive.today":
                    this.service = DefaultServices.ArchiveToday;
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
                case DefaultServices.ArchivePh:
                    return new ArchivePhService();
                case DefaultServices.ArchiveVn:
                    return new ArchiveVnService();
                case DefaultServices.ArchiveMd:
                    return new ArchiveMdService();
                case DefaultServices.ArchiveToday:
                    return new ArchiveTodayService();
                default:
                    return new ArchiveFoService();
            }
        }
    }
}

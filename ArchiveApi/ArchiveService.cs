using ArchiveApi.Interfaces;
using ArchiveApi.Services;
namespace ArchiveApi
{
    public enum DefaultServices
    {
        ArchiveIs,
        ArchiveFo
    }
    public static class ArchiveService
    {
        public static IArchiveService CreateService(DefaultServices service = DefaultServices.ArchiveFo)
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

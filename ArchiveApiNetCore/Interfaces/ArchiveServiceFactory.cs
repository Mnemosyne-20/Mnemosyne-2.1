namespace ArchiveApi.Interfaces
{
    public abstract class IArchiveServiceFactory
    {
        public abstract IArchiveService CreateNewService();
    }
}
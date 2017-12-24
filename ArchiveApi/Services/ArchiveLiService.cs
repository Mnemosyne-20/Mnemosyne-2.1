namespace ArchiveApi.Services
{
    /// <summary>
    /// This class is used for archive.li services, it is essentially a non-russian-accessible reskin of archive.fo / archive.li / archive.is / archive.today /....
    /// </summary>
    public sealed class ArchiveLiService : ArchiveTodayInternal
    {
        public ArchiveLiService() : base("li")
        {

        }
    }

}

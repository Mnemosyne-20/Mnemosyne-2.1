namespace ArchiveApi.Services
{
    /// <summary>
    /// This class is used for archive.is services, it is essentially a non-russian-accessible reskin of archive.fo / archive.li / archive.today / ...., like, no, seriously this website has like 6 TLDs availible for it.
    /// </summary>
    public sealed class ArchiveIsService : ArchiveTodayInternal
    {
        public ArchiveIsService() : base("is")
        {

        }
    }
}

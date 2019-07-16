using ArchiveApi;
using ArchiveApi.Interfaces;
using ArchiveApi.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace ArchiveApiTest
{
    [TestClass]
    public class ArchiveApiUnitTest
    {
        [TestCategory("ArchiveFoTest")]
        [TestMethod]
        public void TestArchiveFoVerify()
        {
            IArchiveService archiveService = new ArchiveFoService();
            Assert.IsFalse(archiveService.Verify("http://archive.fo"));
            Assert.IsFalse(archiveService.Verify("http://archive.fo/"));
            Assert.IsFalse(archiveService.Verify("http://archive.fo/submit"));
            Assert.IsFalse(archiveService.Verify("http://archive.fo/submit/"));
            Assert.IsTrue(archiveService.Verify("http://archive.fo/V8YhZ"));
            ((ArchiveFoService)archiveService).ClearDomains();
            Assert.IsFalse(archiveService.Verify("http://archive.is/V8YhZ"));
            // Archive.is failures
            new ArchiveIsService();
            Assert.IsFalse(archiveService.Verify("http://archive.is"));
            Assert.IsFalse(archiveService.Verify("http://archive.is/"));
            Assert.IsFalse(archiveService.Verify("http://archive.is/submit"));
            Assert.IsFalse(archiveService.Verify("http://archive.is/submit/"));
            Assert.IsTrue(archiveService.Verify("http://archive.is/V8YhZ"));

            Assert.IsFalse(archiveService.Verify("https://archive.is"));
            Assert.IsFalse(archiveService.Verify("https://archive.is/"));
            Assert.IsFalse(archiveService.Verify("https://archive.is/submit"));
            Assert.IsFalse(archiveService.Verify("https://archive.is/submit/"));
            Assert.IsTrue(archiveService.Verify("https://archive.is/V8YhZ"));
        }
        [TestCategory("ArchiveIsTest")]
        [TestMethod]
        public void TestArchiveIsVerify()
        {
            IArchiveService archiveService = new ArchiveIsService();
            Assert.IsFalse(archiveService.Verify("http://archive.is"));
            Assert.IsFalse(archiveService.Verify("http://archive.is/"));
            Assert.IsFalse(archiveService.Verify("http://archive.is/submit"));
            Assert.IsFalse(archiveService.Verify("http://archive.is/submit/"));
            Assert.IsTrue(archiveService.Verify("http://archive.is/V8YhZ"));

            Assert.IsFalse(archiveService.Verify("https://archive.is"));
            Assert.IsFalse(archiveService.Verify("https://archive.is/"));
            Assert.IsFalse(archiveService.Verify("https://archive.is/submit"));
            Assert.IsFalse(archiveService.Verify("https://archive.is/submit/"));
            Assert.IsTrue(archiveService.Verify("https://archive.is/V8YhZ"));
            ((ArchiveIsService)archiveService).ClearDomains();
            Assert.IsFalse(archiveService.Verify("http://archive.fo/V8YhZ"));
            new ArchiveFoService();
            // Archive.FO failures
            Assert.IsFalse(archiveService.Verify("http://archive.fo"));
            Assert.IsFalse(archiveService.Verify("http://archive.fo/"));
            Assert.IsFalse(archiveService.Verify("http://archive.fo/submit"));
            Assert.IsFalse(archiveService.Verify("http://archive.fo/submit/"));
            Assert.IsTrue(archiveService.Verify("http://archive.fo/V8YhZ"));

            Assert.IsFalse(archiveService.Verify("https://archive.fo"));
            Assert.IsFalse(archiveService.Verify("https://archive.fo/"));
            Assert.IsFalse(archiveService.Verify("https://archive.fo/submit"));
            Assert.IsFalse(archiveService.Verify("https://archive.fo/submit/"));
            Assert.IsTrue(archiveService.Verify("https://archive.fo/V8YhZ"));
        }
        [TestCategory("ArchiveIsTest")]
        [TestMethod]
        public void TestArchiveMdVerify()
        {
            //  https://archive.md/XJwrn
            new ArchiveService(DefaultServices.ArchiveIs).CreateNewService();
            new ArchiveService(DefaultServices.ArchiveLi).CreateNewService();
            new ArchiveService(DefaultServices.ArchivePh).CreateNewService();
            new ArchiveService(DefaultServices.ArchiveVn).CreateNewService();
            new ArchiveService(DefaultServices.ArchiveMd).CreateNewService();
            new ArchiveService(DefaultServices.ArchiveToday).CreateNewService();
            IArchiveService archiveService = new ArchiveMdService();
            Assert.IsFalse(archiveService.Verify("http://archive.md"));
            Assert.IsFalse(archiveService.Verify("http://archive.md/"));
            Assert.IsFalse(archiveService.Verify("http://archive.md/submit"));
            Assert.IsFalse(archiveService.Verify("http://archive.md/submit/"));
            Assert.IsTrue(archiveService.Verify("http://archive.md/V8YhZ"));
            Assert.IsTrue(archiveService.Verify("http://archive.md/XJwrn"));

            Assert.IsFalse(archiveService.Verify("https://archive.md"));
            Assert.IsFalse(archiveService.Verify("https://archive.md/"));
            Assert.IsFalse(archiveService.Verify("https://archive.md/submit"));
            Assert.IsFalse(archiveService.Verify("https://archive.md/submit/"));
            Assert.IsTrue(archiveService.Verify("https://archive.md/V8YhZ"));
            Assert.IsTrue(archiveService.Verify("https://archive.md/XJwrn"));
            ((ArchiveMdService)archiveService).ClearDomains();
            Assert.IsFalse(archiveService.Verify("http://archive.fo/V8YhZ"));
            Assert.IsTrue(archiveService.Verify("https://archive.md/XJwrn"));

        }
    }
}
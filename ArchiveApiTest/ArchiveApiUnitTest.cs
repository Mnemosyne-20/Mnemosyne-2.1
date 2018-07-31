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
            ((ArchiveIsService)archiveService).ClearDomains();
            Assert.IsFalse(archiveService.Verify("http://archive.fo/V8YhZ"));
            new ArchiveFoService();
            // Archive.FO failures
            Assert.IsFalse(archiveService.Verify("http://archive.fo"));
            Assert.IsFalse(archiveService.Verify("http://archive.fo/"));
            Assert.IsFalse(archiveService.Verify("http://archive.fo/submit"));
            Assert.IsFalse(archiveService.Verify("http://archive.fo/submit/"));
            Assert.IsTrue(archiveService.Verify("http://archive.fo/V8YhZ"));
        }
    }
}
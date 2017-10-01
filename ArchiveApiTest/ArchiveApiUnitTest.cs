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
        }
    }
}
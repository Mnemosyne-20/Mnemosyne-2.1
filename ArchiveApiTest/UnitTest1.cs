using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArchiveApi;
namespace ArchiveApiTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestVerify()
        {
            ArchiveService service = new ArchiveService("https://archive.is");
            Assert.IsFalse(service.Verify("http://archive.is/"));
            Assert.IsFalse(service.Verify("http://archive.is"));
            Assert.IsFalse(service.Verify("http://archive.is/submit"));
            Assert.IsFalse(service.Verify("https://archive.is/"));
            Assert.IsFalse(service.Verify("https://archive.is/submit"));
            Assert.IsFalse(service.Verify("https://archive.is"));
        }
    }
}

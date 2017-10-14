using ArchiveApi;
using CoAP;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
namespace ArchiveApiTest
{
    [TestClass]
    public class MementoUnitTest
    {
        #region teststring
        static string Test = @"<http://kernel.org/>; rel=""original"",
<http://archive.is/timegate/http://kernel.org/>; rel=""timegate"",
<http://archive.is/20120523210412/http://kernel.org/>; rel=""first memento""; datetime=""Wed, 23 May 2012 21:04:12 GMT"",
<http://archive.is/20130109172600/http://kernel.org/>; rel=""memento""; datetime=""Wed, 09 Jan 2013 17:26:00 GMT"",
<http://archive.is/20130224014512/http://kernel.org/>; rel=""memento""; datetime=""Sun, 24 Feb 2013 01:45:12 GMT"",
<http://archive.is/20131125152406/https://kernel.org/>; rel=""memento""; datetime=""Mon, 25 Nov 2013 15:24:06 GMT"",
<http://archive.is/20140601131731/http://kernel.org/>; rel=""memento""; datetime=""Sun, 01 Jun 2014 13:17:31 GMT"",
<http://archive.is/20150401170814/http://kernel.org/>; rel=""memento""; datetime=""Wed, 01 Apr 2015 17:08:14 GMT"",
<http://archive.is/20150703202917/https://kernel.org/>; rel=""memento""; datetime=""Fri, 03 Jul 2015 20:29:17 GMT"",
<http://archive.is/20150715032634/http://kernel.org/>; rel=""memento""; datetime=""Wed, 15 Jul 2015 03:26:34 GMT"",
<http://archive.is/20150908032902/https://kernel.org/>; rel=""memento""; datetime=""Tue, 08 Sep 2015 03:29:02 GMT"",
<http://archive.is/20150922214548/https://kernel.org/>; rel=""memento""; datetime=""Tue, 22 Sep 2015 21:45:48 GMT"",
<http://archive.is/20151013090704/https://kernel.org/>; rel=""memento""; datetime=""Tue, 13 Oct 2015 09:07:04 GMT"",
<http://archive.is/20151030023534/https://kernel.org/>; rel=""memento""; datetime=""Fri, 30 Oct 2015 02:35:34 GMT"",
<http://archive.is/20160102094030/https://kernel.org/>; rel=""memento""; datetime=""Sat, 02 Jan 2016 09:40:30 GMT"",
<http://archive.is/20160204181713/https://kernel.org/>; rel=""memento""; datetime=""Thu, 04 Feb 2016 18:17:13 GMT"",
<http://archive.is/20160416010219/https://kernel.org/>; rel=""memento""; datetime=""Sat, 16 Apr 2016 01:02:19 GMT"",
<http://archive.is/20161021231009/https://kernel.org/>; rel=""memento""; datetime=""Fri, 21 Oct 2016 23:10:09 GMT"",
<http://archive.is/20170308191942/https://kernel.org/>; rel=""last memento""; datetime=""Wed, 08 Mar 2017 19:19:42 GMT"",
<http://archive.is/timemap/http://kernel.org/>; rel=""self""; type=""application/link-format""; from=""Wed, 23 May 2012 21:04:12 GMT""; until=""Wed, 08 Mar 2017 19:19:42 GMT""";
        #endregion
        [TestCategory("TimeGate")]
        [TestMethod]
        public void TimeGateTest()
        {
            Mementos mementos = new Mementos(LinkFormat.Parse(Test));
            Assert.IsTrue(mementos.TimeGate == "http://archive.is/timegate/http://kernel.org/");
        }
        [TestCategory("Mementos")]
        [TestMethod]
        public void MementoFirstTest()
        {
            Mementos mementos = new Mementos(LinkFormat.Parse(Test));
            Assert.IsTrue(mementos.FirstMemento == "http://archive.is/20120523210412/http://kernel.org/");
        }
        [TestCategory("Mementos")]
        [TestMethod]
        public void MementoLastTest()
        {
            Mementos mementos = new Mementos(LinkFormat.Parse(Test));
            Assert.IsTrue(mementos.LastMemento == "http://archive.is/20170308191942/https://kernel.org/");
        }
        [TestCategory("Mementos")]
        [TestMethod]
        public void MementoOriginalTest()
        {
            Mementos mementos = new Mementos(LinkFormat.Parse(Test));
            Assert.IsTrue(mementos.Original == "http://kernel.org/");
        }
        [TestCategory("TimeMap")]
        [TestMethod]
        public void TimeMapFromTest()
        {
            WebLink link = LinkFormat.Parse(Test).Where(a => a.Attributes.GetValues("rel").Contains("self")).First();
            Assert.IsTrue(new TimeMap(link).From == DateTime.Parse("Wed, 23 May 2012 21:04:12 GMT"));
        }
        [TestCategory("TimeMap")]
        [TestMethod]
        public void TimeMapUntilTest()
        {
            WebLink link = LinkFormat.Parse(Test).Where(a => a.Attributes.GetValues("rel").Contains("self")).First();
            Assert.IsTrue(new TimeMap(link).Until == DateTime.Parse("Wed, 08 Mar 2017 19:19:42 GMT"));
        }
    }
}

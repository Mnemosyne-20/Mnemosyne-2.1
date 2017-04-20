﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Mnemosyne2Reborn;
using ArchiveApi;
using RedditSharp;
namespace MnemosyneTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestExclusions()
        {
            List<string> foundLinks = new List<string>
            {
                "archive.is"
            };
            List<string> s = ArchiveLinks.ArchivePostLinks(ref foundLinks, new System.Text.RegularExpressions.Regex[] { new System.Text.RegularExpressions.Regex(@"archive\.is") }, new Reddit().GetUser("chugga_fan"), new ArchiveService("http://www.archive.is"));
            Assert.IsTrue(s.Count == 0 && foundLinks.Count == 0);
        }
        [TestMethod]
        public void TestProfiles()
        {
            RedditUserProfile profile = new RedditUserProfile(new Reddit().GetUser("chugga_fan"), false);
            int before = profile.ArchivedUrlsUsed;
            profile.AddUrlUsed("archive.is");
            Assert.AreNotEqual(before, profile.ArchivedUrlsUsed);
        }
        [TestMethod]
        public void Test()
        {

        }
    }
}
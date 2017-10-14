using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mnemosyne2Reborn.UserData;
using RedditSharp;
namespace MnemosyneTest
{
    [TestClass]
    public class RedditUserProfileSQLiteUnitTest
    {
        Reddit red;
        [TestInitialize]
        public void InitializeTestVars()
        {
            red = new Reddit();
            System.IO.Directory.CreateDirectory(".\\Data\\5");
        }
        [TestCategory("RedditUserProfileSQLite")]
        [TestMethod]
        public void AddArchivedTest()
        {
            new RedditUserProfileSqlite("1\\redditusers.sqlite");
            RedditUserProfileSqlite redditUserProfileSqlite = new RedditUserProfileSqlite(red.GetUser("chugga_fan"));
            var current = redditUserProfileSqlite.Archived;
            var next = ++redditUserProfileSqlite.Archived;
            Assert.IsFalse(next == current);
        }
        [TestCategory("RedditUserProfileSQLite")]
        [TestMethod]
        public void TestOptOut()
        {
            new RedditUserProfileSqlite("2\\redditusers.sqlite");
            RedditUserProfileSqlite redditUserProfileSqlite = new RedditUserProfileSqlite(red.GetUser("chugga_fan"))
            {
                OptedOut = true
            };
            Assert.IsTrue(redditUserProfileSqlite.OptedOut);
        }
        [TestCategory("RedditUserProfileSQLite")]
        [TestMethod]
        public void TestAddUnarchived()
        {
            new RedditUserProfileSqlite("3\\redditusers.sqlite");
            RedditUserProfileSqlite redditUserProfileSqlite = new RedditUserProfileSqlite(red.GetUser("chugga_fan"));
            var current = redditUserProfileSqlite.Unarchived;
            var next = ++redditUserProfileSqlite.Unarchived;
            Assert.IsFalse(next == current);
        }
        [TestCategory("RedditUserProfileSQLite")]
        [TestMethod]
        public void TestExcluded()
        {
            new RedditUserProfileSqlite("4\\redditusers.sqlite");
            RedditUserProfileSqlite redditUserProfileSqlite = new RedditUserProfileSqlite(red.GetUser("chugga_fan"));
            var current = redditUserProfileSqlite.Excluded;
            var next = ++redditUserProfileSqlite.Excluded;
            Assert.IsFalse(next == current);
        }
        [TestCategory("RedditUserProfileSQLite")]
        [TestMethod]
        public void TestImage()
        {
            new RedditUserProfileSqlite("5\\redditusers.sqlite");
            RedditUserProfileSqlite redditUserProfileSqlite = new RedditUserProfileSqlite(red.GetUser("chugga_fan"));
            var current = redditUserProfileSqlite.Image;
            var next = ++redditUserProfileSqlite.Image;
            Assert.IsFalse(next == current);
        }
    }
}

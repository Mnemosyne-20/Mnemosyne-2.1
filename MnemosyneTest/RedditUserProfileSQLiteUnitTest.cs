using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mnemosyne2Reborn.UserData;
using RedditSharp;
namespace MnemosyneTest
{
    [TestClass]
    public class RedditUserProfileSQLiteUnitTest
    {
        [TestMethod]
        public void AddArchived()
        {
            new RedditUserProfileSqlite("1\\redditusers.sqlite");
            var red = new Reddit();
            RedditUserProfileSqlite redditUserProfileSqlite = new RedditUserProfileSqlite(red.GetUser("chugga_fan"));
            var current = redditUserProfileSqlite.Archived;
            redditUserProfileSqlite.Archived++;
            var next = redditUserProfileSqlite.Archived;
            Assert.IsFalse(next == current);
        }
        [TestMethod]
        public void TestOptOut()
        {
            new RedditUserProfileSqlite("2\\redditusers.sqlite");
            var red = new Reddit();
            RedditUserProfileSqlite redditUserProfileSqlite = new RedditUserProfileSqlite(red.GetUser("chugga_fan"))
            {
                OptedOut = true
            };
            Assert.IsTrue(redditUserProfileSqlite.OptedOut);
        }
        [TestMethod]
        public void TestAddUnarchived()
        {
            new RedditUserProfileSqlite("3\\redditusers.sqlite");
            var red = new Reddit();
            RedditUserProfileSqlite redditUserProfileSqlite = new RedditUserProfileSqlite(red.GetUser("chugga_fan"));
            var current = redditUserProfileSqlite.Unarchived;
            redditUserProfileSqlite.Unarchived++;
            var next = redditUserProfileSqlite.Unarchived;
            Assert.IsFalse(next == current);
        }
        [TestMethod]
        public void TestExcluded()
        {
            new RedditUserProfileSqlite("4\\redditusers.sqlite");
            var red = new Reddit();
            RedditUserProfileSqlite redditUserProfileSqlite = new RedditUserProfileSqlite(red.GetUser("chugga_fan"));
            var current = redditUserProfileSqlite.Excluded;
            redditUserProfileSqlite.Excluded++;
            var next = redditUserProfileSqlite.Excluded;
            Assert.IsFalse(next == current);
        }
        [TestMethod]
        public void TestImage()
        {
            System.IO.Directory.CreateDirectory(".\\Data\\5");
            new RedditUserProfileSqlite("5\\redditusers.sqlite");
            var red = new Reddit();
            RedditUserProfileSqlite redditUserProfileSqlite = new RedditUserProfileSqlite(red.GetUser("chugga_fan"));
            var current = redditUserProfileSqlite.Image;
            redditUserProfileSqlite.Image++;
            var next = redditUserProfileSqlite.Image;
            Assert.IsFalse(next == current);
        }
    }
}

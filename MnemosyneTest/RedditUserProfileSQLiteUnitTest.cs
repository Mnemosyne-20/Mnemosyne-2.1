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
            redditUserProfileSqlite.Archived = 1;
            var next = redditUserProfileSqlite.Archived;
            Assert.IsFalse(next == current);
        }
        [TestMethod]
        public void TestOptOut()
        {
            new RedditUserProfileSqlite("2\\redditusers.sqlite");
            var red = new Reddit();
            RedditUserProfileSqlite redd = new RedditUserProfileSqlite(red.GetUser("chugga_fan"))
            {
                OptedOut = true
            };
            Assert.IsTrue(redd.OptedOut);
        }
        [TestMethod]
        public void TestAddUnarchived()
        {
            new RedditUserProfileSqlite("3\\redditusers.sqlite");
            var red = new Reddit();
            RedditUserProfileSqlite redditUserProfileSqlite = new RedditUserProfileSqlite(red.GetUser("chugga_fan"));
            var current = redditUserProfileSqlite.GetUnarchived("chugga_fan");
            redditUserProfileSqlite.SetUnarchived("chugga_fan", 1);
            var next = redditUserProfileSqlite.GetUnarchived("chugga_fan");
            Assert.IsFalse(next == current);
        }
    }
}

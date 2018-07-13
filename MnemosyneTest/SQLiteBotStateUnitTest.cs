using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mnemosyne2Reborn.BotState;
using System.Linq;
namespace MnemosyneTest
{
    [TestClass]
    [DeploymentItem("x64\\SQLite.Interop.dll", "x64")] //it's half a bit stupid that this is even necessary, and another half a bit stupid that this specifically isn't deleted afterwards
    [DeploymentItem("x86\\SQLite.Interop.dll", "x86")]
    public class SQLiteBotStateUnitTest
    {
        [TestMethod]
        [TestCategory("SQLiteBotState")]
        [DeploymentItem("Test.sqlite", "Data\\1")]
        public void TestAddBotCommentSqlite()
        {
            SQLiteBotState sqliteBotState = new SQLiteBotState("1\\Test.sqlite");
            sqliteBotState.AddCheckedPost("sad");
            sqliteBotState.AddBotComment("sad", "postcomment");
            Assert.AreEqual(sqliteBotState.GetCommentForPost("sad"), "postcomment");
        }
        [TestMethod]
        [DeploymentItem("Test.sqlite", "Data\\2")]
        [TestCategory("SQLiteBotState")]
        public void TestCheckCommentSqlite()
        {
            SQLiteBotState sqliteBotState = new SQLiteBotState("2\\Test.sqlite");
            sqliteBotState.AddCheckedComment("postcomment");
            Assert.IsTrue(sqliteBotState.HasCommentBeenChecked("postcomment"));
        }
        [TestMethod]
        [DeploymentItem("Test.sqlite", "Data\\3")]
        [TestCategory("SQLiteBotState")]
        public void TestCheckPostSqlite()
        {
            SQLiteBotState sqliteBotState = new SQLiteBotState("3\\Test.sqlite");
            sqliteBotState.AddCheckedPost("postpost");
            Assert.IsTrue(sqliteBotState.HasPostBeenChecked("postpost"));
        }
        [TestMethod]
        [DeploymentItem("Test.sqlite", "Data\\4")]
        [TestCategory("SQLiteBotState")]
        public void TestUpdateCommentSqlite()
        {
            SQLiteBotState sqliteBotState = new SQLiteBotState("4\\Test.sqlite");
            sqliteBotState.AddCheckedPost("post");
            sqliteBotState.AddBotComment("post", "postcomment");
            sqliteBotState.UpdateBotComment("post", "postcomment2");
            Assert.AreEqual(sqliteBotState.GetCommentForPost("post"), "postcomment2");
        }
        [TestMethod]
        [DeploymentItem("Test.sqlite", "Data\\5")]
        [TestCategory("SQLiteBotState")]
        public void TestArchive24HoursSqlite()
        {
            SQLiteBotState sqliteBotState = new SQLiteBotState("5\\Test.sqlite");
            sqliteBotState.AddCheckedPost("post");
            Assert.IsFalse(sqliteBotState.Is24HourArchived("post"));
            Assert.AreEqual(sqliteBotState.GetNon24HourArchivedPosts().Length, 1);
            sqliteBotState.Archive24Hours("post");
            Assert.IsTrue(sqliteBotState.Is24HourArchived("post"));
            Assert.AreEqual(sqliteBotState.GetNon24HourArchivedPosts().Length, 0);
        }
        [TestMethod]
        [DeploymentItem("Test.sqlite", "Data\\6")]
        [TestCategory("SQLiteBotState")]
        public void TestDeleteCommentSqlite()
        {
            SQLiteBotState sqliteBotState = new SQLiteBotState("6\\Test.sqlite");
            sqliteBotState.AddCheckedPost("post");
            sqliteBotState.AddBotComment("post", "thing");
            Assert.IsTrue(sqliteBotState.HasPostBeenChecked("post"));
            Assert.AreEqual(sqliteBotState.GetCommentForPost("post"), "thing");
            sqliteBotState.DeletePostChecked("post");
            Assert.IsFalse(sqliteBotState.HasPostBeenChecked("post"));
            Assert.ThrowsException<System.InvalidOperationException>(() => sqliteBotState.AddBotComment("post", "thing"));
        }
        [TestMethod]
        [DeploymentItem("Test.sqlite", "Data\\7")]
        [TestCategory("SQLiteBotState")]
        public void TestTransferSqlite()
        {
            FlatBotState flatBotState = new FlatBotState(".\\Data\\7");
            flatBotState.AddCheckedPost("post");
            flatBotState.AddBotComment("post", "thing");
            flatBotState.AddCheckedComment("things");
            flatBotState.AddCheckedPost("post2");
            flatBotState.Archive24Hours("post2");
            SQLiteBotState sqliteBotState = new SQLiteBotState(flatBotState, "7\\Test.sqlite");
            Assert.IsTrue(sqliteBotState.HasPostBeenChecked("post"));
            Assert.AreEqual(sqliteBotState.GetCommentForPost("post"), "thing");
            Assert.IsTrue(sqliteBotState.HasCommentBeenChecked("things"));
            Assert.IsTrue(sqliteBotState.Is24HourArchived("post2"));
            Assert.IsTrue(sqliteBotState.GetNon24HourArchivedPosts().Contains("post"));
        }
    }
}
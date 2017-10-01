using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mnemosyne2Reborn.BotState;
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
            sqliteBotState.AddBotComment("sad", "postcomment");
            Assert.IsTrue(sqliteBotState.GetCommentForPost("sad") == "postcomment");
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
            sqliteBotState.AddBotComment("post", "postcomment");
            sqliteBotState.UpdateBotComment("post", "postcomment2");
            Assert.IsTrue(sqliteBotState.GetCommentForPost("post") == "postcomment2");
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mnemosyne2Reborn.BotState;
namespace MnemosyneTest
{
    [TestClass]
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
    }
}

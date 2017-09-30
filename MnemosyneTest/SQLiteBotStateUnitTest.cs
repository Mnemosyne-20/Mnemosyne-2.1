using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools;
using Mnemosyne2Reborn.BotState;
namespace MnemosyneTest
{
    [TestClass]
    public class SQLiteBotStateUnitTest
    {
        [TestInitialize]
        public void Init()
        {
            System.IO.File.Create("Test.sqlite");
        }
        [TestCategory("SQLiteBotState")]
        [DeploymentItem("Test.sqlite", "1")]
        [TestMethod]
        public void TestAddBotCommentSqlite()
        {
            SQLiteBotState sqliteBotState = new SQLiteBotState("1\\Test.sqlite");
            sqliteBotState.AddBotComment("sad", "postcomment");
            Assert.IsTrue(sqliteBotState.GetCommentForPost("sad") == "postcomment");
        }
        [DeploymentItem("Test.sqlite", "2")]
        [TestCategory("SQLiteBotState")]
        [TestMethod]
        public void TestCheckCommentSqlite()
        {
            SQLiteBotState sqliteBotState = new SQLiteBotState("2\\Test.sqlite");
            sqliteBotState.AddCheckedComment("postcomment");
            Assert.IsTrue(sqliteBotState.HasCommentBeenChecked("postcomment"));
        }
    }
}

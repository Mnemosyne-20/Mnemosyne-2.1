using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mnemosyne2Reborn.BotState;
namespace MnemosyneTest
{
    [TestClass]
    public class FlatFileBostStateUnitTest
    {
        [TestMethod]
        public void TestAddBotCommentFlatFile()
        {
            FlatBotState flatBotState = new FlatBotState("./Data/1\\");
            flatBotState.AddBotComment("post", "postcomment");
            Assert.IsTrue(flatBotState.GetCommentForPost("post") == "postcomment");
        }
        [TestMethod]
        public void TestCheckCommentFlatFile()
        {
            FlatBotState flatBotState = new FlatBotState("./Data/2\\");
            flatBotState.AddCheckedComment("postcomment");
            Assert.IsTrue(flatBotState.HasCommentBeenChecked("postcomment"));
        }
        [TestMethod]
        public void TestCheckPostFlatFile()
        {
            FlatBotState flatBotState = new FlatBotState("./Data/3\\");
            flatBotState.AddCheckedPost("post");
            Assert.IsTrue(flatBotState.HasPostBeenChecked("post"));
        }
        [TestMethod]
        public void TestUpdateCommentFlatFile()
        {
            FlatBotState flatBotState = new FlatBotState("./Data/4\\");
            flatBotState.AddBotComment("post", "postcomment");
            flatBotState.UpdateBotComment("post", "postcomment2");
            Assert.IsTrue(flatBotState.GetCommentForPost("post") == "postcomment2");
        }
    }
}

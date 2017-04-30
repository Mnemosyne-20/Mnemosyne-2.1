using System;
using System.Data;
using System.Data.SQLite;
namespace Mnemosyne2Reborn.BotState
{
    public class SQLiteBotState : IBotState
    {
        public SQLiteBotState()
        {
        }
        void InitializeDatabase()
        {

        }
        void InitializeCommands()
        {

        }
        public void AddBotComment(string postID, string commentID)
        {
            throw new NotImplementedException();
        }

        public void AddCheckedComment(string commentID)
        {
            throw new NotImplementedException();
        }

        public bool DoesCommentExist(string commentID)
        {
            throw new NotImplementedException();
        }

        public string GetCommentForPost(string postID)
        {
            throw new NotImplementedException();
        }

        public bool HasCommentBeenChecked(string CommentID)
        {
            throw new NotImplementedException();
        }
    }
}

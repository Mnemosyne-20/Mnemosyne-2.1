using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mnemosyne2Reborn.BotState
{
    public class SQLiteBotState : IBotState
    {
        public SQLiteBotState()
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

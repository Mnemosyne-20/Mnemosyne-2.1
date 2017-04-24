namespace Mnemosyne2Reborn.BotState
{
    public interface IBotState
    {
        bool DoesCommentExist(string postID);
        string GetCommentForPost(string postID);
        bool HasCommentBeenChecked(string CommentID);
        void AddBotComment(string postID, string commentID);
        void AddCheckedComment(string commentID);
    }
}

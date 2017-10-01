﻿using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Mnemosyne2Reborn.BotState
{
    public class SQLiteBotState : IBotState
    {
        SQLiteConnection dbConnection;
        SQLiteCommand SQLCmd_AddBotComment, SQLCmd_AddCheckedComment, SQLCmd_DoesBotCommentExist, SQLCmd_GetBotComment, SQLCmd_HasCommentBeenChecked, SQLCmd_HasPostBeenChecked, SQLCmd_AddCheckedPost, SQLCmd_UpdateBotComment;
        public SQLiteBotState(string filename = "botstate.sqlite")
        {
            if (!File.Exists(filename))
            {
                SQLiteConnection.CreateFile($"{AppDomain.CurrentDomain.BaseDirectory.TrimEnd('/')}/Data/{filename}");
            }
            AppDomain.CurrentDomain.SetData("DataDirectory", $"{AppDomain.CurrentDomain.BaseDirectory.TrimEnd('/')}/Data/");
            dbConnection = new SQLiteConnection($"Data Source=|DataDirectory|{filename};Version=3;");
            dbConnection.Open();
            InitializeDatabase();
            InitializeCommands();
        }
        public bool DoesCommentExist(string postID)
        {
            SQLCmd_DoesBotCommentExist.Parameters["@postID"].Value = postID;
            long count = SQLCmd_DoesBotCommentExist.ExecuteScalar() as long? ?? 0;
            return count != 0;
        }
        public void AddCheckedComment(string commentID)
        {
            try
            {
                SQLCmd_AddCheckedComment.Parameters["@commentID"].Value = commentID;
                SQLCmd_AddCheckedComment.ExecuteNonQuery();
            }
            catch (SQLiteException ex) when (ex.ResultCode == SQLiteErrorCode.Constraint)
            {
                throw new InvalidOperationException($"The comment {commentID} already exists in database");
            }
        }
        public bool HasCommentBeenChecked(string commentID)
        {
            SQLCmd_HasCommentBeenChecked.Parameters["@commentID"].Value = commentID;
            long count = SQLCmd_HasCommentBeenChecked.ExecuteScalar() as long? ?? 0;
            return count != 0;
        }
        void InitializeDatabase()
        {
            string query = "create table if not exists replies (postID text unique, botReplyID text)";
            using (SQLiteCommand cmd = new SQLiteCommand(query, dbConnection))
            {
                cmd.ExecuteNonQuery();
            }
            query = "create table if not exists comments (commentID text unique)"; // yes this is a table with one column and eventually along with the reply table won't even be needed at all
            using (SQLiteCommand cmd = new SQLiteCommand(query, dbConnection))
            {
                cmd.ExecuteNonQuery();
            }
            query = "create table if not exists archives (originalURL text unique, numArchives integer)";
            using (SQLiteCommand cmd = new SQLiteCommand(query, dbConnection))
            {
                cmd.ExecuteNonQuery();
            }
            query = "create table if not exists posts (postID text unique)";
            using (SQLiteCommand cmd = new SQLiteCommand(query, dbConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        void InitializeCommands()
        {
            var PostParam = new SQLiteParameter("@postID", DbType.String);
            SQLCmd_AddBotComment = new SQLiteCommand("insert or abort into replies(postID, botReplyID) values(@postID, @botReplyID)", dbConnection);
            SQLCmd_AddBotComment.Parameters.Add(PostParam);
            SQLCmd_AddBotComment.Parameters.Add(new SQLiteParameter("@botReplyID", DbType.String));

            SQLCmd_AddCheckedComment = new SQLiteCommand("insert or abort into comments (commentID) values (@commentID)", dbConnection);
            SQLCmd_AddCheckedComment.Parameters.Add(new SQLiteParameter("@commentID", DbType.String));

            SQLCmd_AddCheckedPost = new SQLiteCommand("insert or abort into posts (postID) values (@postID)", dbConnection);
            SQLCmd_AddCheckedPost.Parameters.Add(PostParam);

            SQLCmd_DoesBotCommentExist = new SQLiteCommand("select count(*) from replies where postID = @postID", dbConnection);
            SQLCmd_DoesBotCommentExist.Parameters.Add(PostParam);

            SQLCmd_GetBotComment = new SQLiteCommand("select botReplyID from replies where postID = @postID", dbConnection);
            SQLCmd_GetBotComment.Parameters.Add(new SQLiteParameter("@postID", DbType.String));

            SQLCmd_HasCommentBeenChecked = new SQLiteCommand("select count(commentID) from comments where commentID = @commentID", dbConnection);
            SQLCmd_HasCommentBeenChecked.Parameters.Add(new SQLiteParameter("@commentID", DbType.String));

            SQLCmd_HasPostBeenChecked = new SQLiteCommand("select count(postID) from posts where postID = @postID", dbConnection);
            SQLCmd_HasPostBeenChecked.Parameters.Add(PostParam);

            SQLCmd_UpdateBotComment = new SQLiteCommand("update replies set botReplyID = @botReplyID where postID = @postID", dbConnection);
            SQLCmd_UpdateBotComment.Parameters.Add(new SQLiteParameter("@botReplyID", DbType.String));
            SQLCmd_UpdateBotComment.Parameters.Add(PostParam);


        }
        public void AddBotComment(string postID, string commentID)
        {
            try
            {
                SQLCmd_AddBotComment.Parameters["@postID"].Value = postID;
                SQLCmd_AddBotComment.Parameters["@botReplyID"].Value = commentID;
                SQLCmd_AddBotComment.ExecuteNonQuery();
            }
            catch (SQLiteException ex) when (ex.ResultCode == SQLiteErrorCode.Constraint)
            {
                throw new InvalidOperationException($"The post {postID} already exists in database");
            }
        }

        public string GetCommentForPost(string postID)
        {
            SQLCmd_GetBotComment.Parameters["@postID"].Value = postID;
            string botReplyID = (string)SQLCmd_GetBotComment.ExecuteScalar();
            if (string.IsNullOrWhiteSpace(botReplyID))
            {
                throw new InvalidOperationException($"Comment ID for post {postID} is null or empty");
            }
            return botReplyID;
        }

        public void UpdateBotComment(string postID, string commentID)
        {
            SQLCmd_UpdateBotComment.Parameters["@postID"].Value = postID;
            SQLCmd_UpdateBotComment.Parameters["@botReplyID"].Value = commentID;
            SQLCmd_UpdateBotComment.ExecuteNonQuery();
        }

        public void AddCheckedPost(string postId)
        {
            try
            {
                SQLCmd_AddCheckedPost.Parameters["@postID"].Value = postId;
                SQLCmd_AddCheckedPost.ExecuteNonQuery();
            }
            catch (SQLiteException ex) when (ex.ResultCode == SQLiteErrorCode.Constraint)
            {
                throw new InvalidOperationException($"the post {postId} already exists in the database");
            }
        }

        public bool HasPostBeenChecked(string postId)
        {
            SQLCmd_HasPostBeenChecked.Parameters["@postID"].Value = postId;
            long count = SQLCmd_HasPostBeenChecked.ExecuteScalar() as long? ?? 0;
            return count != 0;
        }
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    SQLCmd_AddBotComment.Dispose();
                    SQLCmd_AddCheckedComment.Dispose();
                    SQLCmd_DoesBotCommentExist.Dispose();
                    SQLCmd_GetBotComment.Dispose();
                    SQLCmd_HasCommentBeenChecked.Dispose();
                    SQLCmd_HasPostBeenChecked.Dispose();
                    SQLCmd_AddCheckedPost.Dispose();
                    SQLCmd_UpdateBotComment.Dispose();
                    dbConnection.Dispose();
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SQLiteBotState() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

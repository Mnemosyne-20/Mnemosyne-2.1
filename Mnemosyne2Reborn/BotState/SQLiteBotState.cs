using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Mnemosyne2Reborn.BotState
{
    public class SQLiteBotState : IBotState
    {
        SQLiteConnection dbConnection;
        SQLiteCommand SQLCmd_AddBotComment,
            SQLCmd_AddCheckedComment,
            SQLCmd_DoesBotCommentExist,
            SQLCmd_GetBotComment,
            SQLCmd_HasCommentBeenChecked,
            SQLCmd_HasPostBeenChecked,
            SQLCmd_AddCheckedPost,
            SQLCmd_UpdateBotComment,
            SQLCmd_Update24HourArchive,
            SQLCmd_Is24HourArchived,
            SQLCmd_GetNon24HourArchived,
            SQLCmd_RemoveBotPost;
        public SQLiteBotState(FlatBotState flatBotState, string filename = "botstate.sqlite")
        {
            if (flatBotState == null)
            {
                throw new ArgumentNullException(nameof(flatBotState));
            }
            if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory.TrimEnd('/')}/Data/{filename}"))
            {
                SQLiteConnection.CreateFile($"{AppDomain.CurrentDomain.BaseDirectory.TrimEnd('/')}/Data/{filename}");
            }
            AppDomain.CurrentDomain.SetData("DataDirectory", $"{AppDomain.CurrentDomain.BaseDirectory.TrimEnd('/')}/Data/");
            dbConnection = new SQLiteConnection($"Data Source=|DataDirectory|{filename};Version=3;");
            dbConnection.Open();
            InitializeDatabase();
            InitializeCommands();
            foreach(var thing in flatBotState.GetAllCheckedPosts())
            {
                this.AddCheckedPost(thing);
            }
            foreach(var thing in flatBotState.GetAllPosts24Hours())
            {
                if(thing.Value)
                {
                    this.Archive24Hours(thing.Key);
                }
            }
            foreach(var thing in flatBotState.GetAllBotComments())
            {
                this.AddBotComment(thing.Key, thing.Value);
            }
            foreach(var thing in flatBotState.GetAllCheckedComments())
            {
                this.AddCheckedComment(thing);
            }
        }
        public SQLiteBotState(string filename = "botstate.sqlite")
        {
            if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory.TrimEnd('/')}/Data/{filename}"))
            {
                SQLiteConnection.CreateFile($"{AppDomain.CurrentDomain.BaseDirectory.TrimEnd('/')}/Data/{filename}");
            }
            AppDomain.CurrentDomain.SetData("DataDirectory", $"{AppDomain.CurrentDomain.BaseDirectory.TrimEnd('/')}/Data/");
            dbConnection = new SQLiteConnection($"Data Source=|DataDirectory|{filename};Version=3;foreign keys=True;");
            dbConnection.Open();
            InitializeDatabase();
            InitializeCommands();
        }
        public bool DoesCommentExist(string postID)
        {
            SQLCmd_DoesBotCommentExist.Parameters["@postID"].Value = postID;
            return Convert.ToBoolean(SQLCmd_DoesBotCommentExist.ExecuteScalar());
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
            return Convert.ToBoolean(SQLCmd_HasCommentBeenChecked.ExecuteScalar());
        }
        void InitializeDatabase()
        {
            string query = "create table if not exists posts (postID TEXT NOT NULL UNIQUE, reArchived INTEGER NOT NULL, PRIMARY KEY(postID))";
            using (SQLiteCommand cmd = new SQLiteCommand(query, dbConnection))
            {
                cmd.ExecuteNonQuery();
            }
            query = "create table if not exists replies (postID TEXT NOT NULL UNIQUE, botReplyID text, FOREIGN KEY (postID) REFERENCES posts(postID) ON DELETE CASCADE)";
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
        }

        void InitializeCommands()
        {
            var PostParam = new SQLiteParameter("@postID", DbType.String);
            var BotReplyParam = new SQLiteParameter("@botReplyID", DbType.String);
            var CommentIdParam = new SQLiteParameter("@commentID", DbType.String);
            SQLCmd_AddBotComment = new SQLiteCommand("insert or abort into replies(postID, botReplyID) values(@postID, @botReplyID)", dbConnection);
            SQLCmd_AddBotComment.Parameters.Add(PostParam);
            SQLCmd_AddBotComment.Parameters.Add(BotReplyParam);

            SQLCmd_AddCheckedComment = new SQLiteCommand("insert or abort into comments (commentID) values (@commentID)", dbConnection);
            SQLCmd_AddCheckedComment.Parameters.Add(CommentIdParam);

            SQLCmd_AddCheckedPost = new SQLiteCommand("insert or abort into posts (postID, reArchived) values (@postID, 0)", dbConnection);
            SQLCmd_AddCheckedPost.Parameters.Add(PostParam);

            SQLCmd_DoesBotCommentExist = new SQLiteCommand("select count(*) from replies where postID = @postID", dbConnection);
            SQLCmd_DoesBotCommentExist.Parameters.Add(PostParam);

            SQLCmd_GetBotComment = new SQLiteCommand("select botReplyID from replies where postID = @postID", dbConnection);
            SQLCmd_GetBotComment.Parameters.Add(PostParam);

            SQLCmd_HasCommentBeenChecked = new SQLiteCommand("select count(commentID) from comments where commentID = @commentID", dbConnection);
            SQLCmd_HasCommentBeenChecked.Parameters.Add(CommentIdParam);

            SQLCmd_HasPostBeenChecked = new SQLiteCommand("select count(postID) from posts where postID = @postID", dbConnection);
            SQLCmd_HasPostBeenChecked.Parameters.Add(PostParam);

            SQLCmd_UpdateBotComment = new SQLiteCommand("update replies set botReplyID = @botReplyID where postID = @postID", dbConnection);
            SQLCmd_UpdateBotComment.Parameters.Add(BotReplyParam);
            SQLCmd_UpdateBotComment.Parameters.Add(PostParam);

            SQLCmd_Update24HourArchive = new SQLiteCommand("update posts set reArchived = 1 where postID = @postID", dbConnection);
            SQLCmd_Update24HourArchive.Parameters.Add(PostParam);

            SQLCmd_Is24HourArchived = new SQLiteCommand("select reArchived from posts where postID = @postID", dbConnection);
            SQLCmd_Is24HourArchived.Parameters.Add(PostParam);

            SQLCmd_GetNon24HourArchived = new SQLiteCommand("select postID from posts where reArchived = 0", dbConnection);
            SQLCmd_RemoveBotPost = new SQLiteCommand("DELETE FROM posts WHERE postID = @postID", dbConnection);
            SQLCmd_RemoveBotPost.Parameters.Add(PostParam);
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
                throw new InvalidOperationException($"The post {postID} already exists in database or has not been checked before");
            }
        }

        public string GetCommentForPost(string postID)
        {
            SQLCmd_GetBotComment.Parameters["@postID"].Value = postID;
            string botReplyID = Convert.ToString(SQLCmd_GetBotComment.ExecuteScalar());
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
            return Convert.ToBoolean(SQLCmd_HasPostBeenChecked.ExecuteScalar());
        }
        public bool Is24HourArchived(string postId)
        {
            SQLCmd_Is24HourArchived.Parameters["@postID"].Value = postId;
            return Convert.ToBoolean(SQLCmd_Is24HourArchived.ExecuteScalar());
        }

        public void Archive24Hours(string postId)
        {
            SQLCmd_Update24HourArchive.Parameters["@postID"].Value = postId;
            SQLCmd_Update24HourArchive.ExecuteNonQuery();
        }
        public string[] GetNon24HourArchivedPosts()
        {
            List<string> readerCounter = new List<string>();
            using (SQLiteDataReader reader = SQLCmd_GetNon24HourArchived.ExecuteReader())
            {
                while (reader.Read())
                {
                    readerCounter.Add((string)reader["postID"]);
                }
            }
            return readerCounter.ToArray();
        }
        public void DeletePostChecked(string postID)
        {
            SQLCmd_RemoveBotPost.Parameters["@postID"].Value = postID;
            SQLCmd_RemoveBotPost.ExecuteNonQuery();
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
                    SQLCmd_Is24HourArchived.Dispose();
                    SQLCmd_Update24HourArchive.Dispose();
                    SQLCmd_GetNon24HourArchived.Dispose();
                    SQLCmd_RemoveBotPost.Dispose();
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
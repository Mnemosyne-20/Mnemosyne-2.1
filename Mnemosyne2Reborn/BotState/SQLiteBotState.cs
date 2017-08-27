using System;
using System.Data.SQLite;
using System.IO;
namespace Mnemosyne2Reborn.BotState
{
    public class SQLiteBotState : IBotState
    {
        SQLiteConnection dbConnection;
        SQLiteCommand SQLCmd_AddBotComment, SQLCmd_AddCheckedComment, SQLCmd_DoesBotCommentExist, SQLCmd_GetBotComment, SQLCmd_HasCommentBeenChecked, SQLCmd_HasPostBeenChecked, SQLCmd_AddCheckedPost;
        public SQLiteBotState(string filename = "botstate.sqlite")
        {
            if (!File.Exists(filename))
            {
                SQLiteConnection.CreateFile(filename);
            }
            string assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            AppDomain.CurrentDomain.SetData("DataDirectory", assemblyPath);
            dbConnection = new SQLiteConnection($"Data Source=|DataDirectory|/Data/{filename};Version=3;");
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
            catch (SQLiteException ex)
            {
                if (ex.ResultCode == SQLiteErrorCode.Constraint)
                {
                    Console.WriteLine($"The comment {commentID} already exists in database");
                }
                else
                {
                    throw;
                }
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
            SQLiteCommand cmd = new SQLiteCommand(query, dbConnection);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            query = "create table if not exists comments (commentID text unique)"; // yes this is a table with one column and eventually along with the reply table won't even be needed at all
            cmd = new SQLiteCommand(query, dbConnection);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            query = "create table if not exists archives (originalURL text unique, numArchives integer)";
            cmd = new SQLiteCommand(query, dbConnection);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        void InitializeCommands()
        {
            SQLCmd_AddBotComment = new SQLiteCommand("insert or abort into replies(postID, botReplyID) values(@postID, @botReplyID)", dbConnection);
            SQLCmd_AddBotComment.Parameters.Add(new SQLiteParameter("@postID"));
            SQLCmd_AddBotComment.Parameters.Add(new SQLiteParameter("@botReplyID"));

            SQLCmd_AddCheckedComment = new SQLiteCommand("insert or abort into comments (commentID) values (@commentID)", dbConnection);
            SQLCmd_AddCheckedComment.Parameters.Add(new SQLiteParameter("@commentID"));

            SQLCmd_DoesBotCommentExist = new SQLiteCommand("select count(*) from replies where postID = @postID", dbConnection);
            SQLCmd_DoesBotCommentExist.Parameters.Add(new SQLiteParameter("@postID"));

            SQLCmd_GetBotComment = new SQLiteCommand("select botReplyID from replies where postID = @postID", dbConnection);
            SQLCmd_GetBotComment.Parameters.Add(new SQLiteParameter("@postID"));

            SQLCmd_HasCommentBeenChecked = new SQLiteCommand("select count(commentID) from comments where commentID = @commentID", dbConnection);
            SQLCmd_HasCommentBeenChecked.Parameters.Add(new SQLiteParameter("@commentID"));
        }
        public void AddBotComment(string postID, string commentID)
        {
            try
            {
                SQLCmd_AddBotComment.Parameters["@postID"].Value = postID;
                SQLCmd_AddBotComment.Parameters["@botReplyID"].Value = commentID;
                SQLCmd_AddBotComment.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                if (ex.ResultCode == SQLiteErrorCode.Constraint)
                {
                    throw new InvalidOperationException($"The post {postID} already exists in database");
                }
                else
                {
                    throw;
                }
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

        public void UpdateBotComment(string postID, string commentID)
        {
            throw new NotImplementedException();
        }

        public void AddCheckedPost(string postId)
        {
            throw new NotImplementedException();
        }

        public bool HasPostBeenChecked(string postId)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

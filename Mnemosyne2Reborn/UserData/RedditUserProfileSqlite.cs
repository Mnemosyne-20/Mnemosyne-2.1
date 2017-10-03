using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using RedditSharp.Things;
using System.IO;
using RedditSharp;
using System.Data;
using System.Data.SQLite.Generic;
namespace Mnemosyne2Reborn.UserData
{
    public class RedditUserProfileSqlite
    {
        static SQLiteCommand SQLiteSetUnarchived, SQLiteSetArchived, SQLiteSetExcluded, SQLiteAddUser, SQLiteGetArchived, SQLiteGetUnarchived, SQLiteGetExcluded, SQLiteGetOptOut, SQLiteSetOptOut, SQLiteGetUserExists;
        static SQLiteConnection connection;
        static bool Initialized = false;
        bool UserExists(RedditUser user)
        {
            SQLiteGetUserExists.Parameters["@Name"].Value = user.Name;
            return SQLiteGetUserExists.ExecuteScalar() as int? != null ? true : false;
        }
        public bool OptedOut
        {
            get
            {
                SQLiteGetOptOut.Parameters["@Name"].Value = User.Name;
                return SQLiteGetOptOut.ExecuteScalar() as int? == 0 ? false : true;
            }
            set
            {
                SQLiteSetOptOut.Parameters["@Name"].Value = User.Name;
                SQLiteSetOptOut.Parameters["@OptedOut"].Value = value ? 1 : 0;
                SQLiteSetOptOut.ExecuteNonQuery();
            }
        }
        public int Unarchived
        {
            get
            {
                SQLiteGetUnarchived.Parameters["@Name"].Value = User.Name;
                return SQLiteGetUnarchived.ExecuteScalar() as int? ?? 0;
            }
        }
        public int Archived
        {
            get
            {
                SQLiteGetArchived.Parameters["@Name"].Value = User.Name;
                return SQLiteGetArchived.ExecuteScalar() as int? ?? 0;
            }
            set
            {
                SQLiteSetArchived.Parameters["@Name"].Value = User.Name;

            }
        }
        public int Excluded
        {
            get
            {
                SQLiteGetExcluded.Parameters["@Name"].Value = User.Name;
                return SQLiteGetExcluded.ExecuteScalar() as int? ?? 0;
            }
            set
            {
                SQLiteSetExcluded.Parameters["@Name"].Value = User.Name;
                SQLiteSetExcluded.Parameters["@ExcludedUrls"].Value = value;
                SQLiteSetExcluded.ExecuteNonQuery();
            }
        }
        public RedditUser User;
        public void AddUrlUsed(string url)
        {
            
        }
        void InitDbTable()
        {
            string query = "create table if not exists Users (Name text unique, UnarchivedUrls integer, ArchivedUrls integer, ExcludedUrls integer, OptedOut integer)";
            using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
                cmd.ExecuteNonQuery();
        }
        void InitDbCommands()
        {
            SQLiteParameter UserNameParam = new SQLiteParameter("@Name", DbType.String);

            SQLiteGetUserExists = new SQLiteCommand("select count(*) from Users where Name = @Name", connection);
            SQLiteGetUserExists.Parameters.Add(UserNameParam);

            SQLiteAddUser = new SQLiteCommand("insert or abort into Users(Name, UnarchivedUrls, ArchivedUrls, ExcludedUrls, OptedOut) values (@Name, 0, 0, 0, 0)", connection);
            SQLiteAddUser.Parameters.Add(UserNameParam);

            SQLiteGetUnarchived = new SQLiteCommand("select UnarchivedUrls from Users where Name = @Name", connection);
            SQLiteGetUnarchived.Parameters.Add(UserNameParam);

            SQLiteGetArchived = new SQLiteCommand("select ArchivedUrls from Users where Name = @Name", connection);
            SQLiteGetArchived.Parameters.Add(UserNameParam);

            SQLiteGetOptOut = new SQLiteCommand("select OptedOut from Users where Name = @Name", connection);
            SQLiteGetOptOut.Parameters.Add(UserNameParam);

            SQLiteGetExcluded = new SQLiteCommand("select ExcludedUrls from Users where Name = @Name", connection);
            SQLiteGetExcluded.Parameters.Add(UserNameParam);

            SQLiteSetArchived = new SQLiteCommand("update Users set ArchivedUrls = @ArchivedUrls where Name = @Name", connection);
            SQLiteSetArchived.Parameters.Add(new SQLiteParameter("@ArchivedUrls", DbType.Int32));
            SQLiteSetArchived.Parameters.Add(UserNameParam);

            SQLiteSetExcluded = new SQLiteCommand("update Users set ExcludedUrls = @ExcludedUrls where Name = @Name", connection);
            SQLiteSetExcluded.Parameters.Add(new SQLiteParameter("@ExcludedUrls", DbType.Int32));
            SQLiteSetExcluded.Parameters.Add(UserNameParam);

            SQLiteSetUnarchived = new SQLiteCommand("update Users set UnarchivedUrls = @UnarchivedUrls where Name = @Name", connection);
            SQLiteSetUnarchived.Parameters.Add(new SQLiteParameter("@UnarchivedUrls", DbType.Int32));
            SQLiteSetUnarchived.Parameters.Add(UserNameParam);

            SQLiteSetOptOut = new SQLiteCommand("update Users set OptedOut = @OptedOut where Name = @Name", connection);
            SQLiteSetOptOut.Parameters.Add(new SQLiteParameter("@OptedOut", DbType.Int32));
            SQLiteSetOptOut.Parameters.Add(UserNameParam);
        }
        public RedditUserProfileSqlite(string filename = "redditusers.sqlite")
        {
            if (!File.Exists(filename))
            {
                SQLiteConnection.CreateFile($"{AppDomain.CurrentDomain.BaseDirectory.TrimEnd('/')}/Data/{filename}");
            }
            AppDomain.CurrentDomain.SetData("DataDirectory", $"{AppDomain.CurrentDomain.BaseDirectory.TrimEnd('/')}/Data/");
            connection = new SQLiteConnection($"Data Source=|DataDirectory|{filename};Version=3;");
            connection.Open();
            InitDbTable();
            InitDbCommands();
            Initialized = true;
        }
        public RedditUserProfileSqlite(RedditUser user)
        {
            if (!Initialized)
                throw new InvalidOperationException("You must initialize using the string based constructor first, then you may use the class later on");
            User = user;
        }
    }
}

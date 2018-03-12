using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
namespace Mnemosyne2Reborn.UserData
{
    public class RedditUserProfileSqlite
    {
        static SQLiteCommand SQLiteSetUnarchived, SQLiteSetArchived, SQLiteSetExcluded, SQLiteSetImage, SQLiteGetImage, SQLiteAddUser, SQLiteGetArchived, SQLiteGetUnarchived, SQLiteGetExcluded, SQLiteGetOptOut, SQLiteSetOptOut, SQLiteGetUserExists, SQLiteAvgExcluded, SQLiteAvgImage, SQLiteAvgArchived, SQLiteAvgUnarchived;
        public static SQLiteConnection Connection { get; private set; }
        static bool Initialized = false;
#pragma warning disable CS0618 // Type or member is obsolete
        public static void TransferProfilesToSqlite(Dictionary<string, RedditUserProfile> dict)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            CheckInitialized();
            var optedOut = from a in dict.AsParallel() where a.Value.OptedOut == true select a; // parallel because the dictionary can be absolutely enormous depending on length of runtime
            foreach (var user in optedOut)
            {
                try
                {
                    new RedditUserProfileSqlite(new Reddit().GetUser(user.Key)).OptedOut = true;
                }
                catch (System.Net.WebException e) when (e.Message.Contains("(404)"))
                {
                    Console.WriteLine($"User {user.Key} no longer exists");
                }
            }
        }
        static void CheckInitialized()
        {
            if (!Initialized)
                throw new InvalidOperationException("You must initialize using the string based constructor first, then you may use the class later on");
        }
        public static bool UserExists(string User)
        {
            CheckInitialized();
            SQLiteGetUserExists.Parameters["@Name"].Value = User;
            return Convert.ToBoolean(SQLiteGetUserExists.ExecuteScalar());
        }
        public static bool UserExists(RedditUser User) => UserExists(User.Name);
        public bool OptedOut
        {
            get
            {
                SQLiteGetOptOut.Parameters["@Name"].Value = User;
                return Convert.ToBoolean(SQLiteGetOptOut.ExecuteScalar());
            }
            set
            {
                SQLiteSetOptOut.Parameters["@Name"].Value = User;
                SQLiteSetOptOut.Parameters["@OptedOut"].Value = value ? 1 : 0;
                SQLiteSetOptOut.ExecuteNonQuery();
            }
        }
        public static float AverageImage
        {
            get
            {
                CheckInitialized();
                return Convert.ToSingle(SQLiteAvgImage.ExecuteScalar());
            }
        }
        public int Image
        {
            get
            {
                SQLiteGetImage.Parameters["@Name"].Value = User;
                return Convert.ToInt32(SQLiteGetImage.ExecuteScalar());
            }
            set
            {
                SQLiteSetImage.Parameters["@Name"].Value = User;
                SQLiteSetImage.Parameters["@ImageUrls"].Value = value;
                SQLiteSetImage.ExecuteNonQuery();
            }
        }
        public float AverageUnarchived
        {
            get
            {
                CheckInitialized();
                return Convert.ToSingle(SQLiteAvgUnarchived.ExecuteScalar());
            }
        }
        public int Unarchived
        {
            get
            {
                SQLiteGetUnarchived.Parameters["@Name"].Value = User;
                return Convert.ToInt32(SQLiteGetUnarchived.ExecuteScalar());
            }
            set
            {
                SQLiteSetUnarchived.Parameters["@Name"].Value = User;
                SQLiteSetUnarchived.Parameters["@UnarchivedUrls"].Value = value;
                SQLiteSetUnarchived.ExecuteNonQuery();
            }
        }
        public static float AverageArchived
        {
            get
            {
                CheckInitialized();
                return Convert.ToSingle(SQLiteAvgArchived.ExecuteScalar());
            }
        }
        public int Archived
        {
            get
            {
                SQLiteGetArchived.Parameters["@Name"].Value = User;
                return Convert.ToInt32(SQLiteGetArchived.ExecuteScalar());
            }
            set
            {
                SQLiteSetArchived.Parameters["@Name"].Value = User;
                SQLiteSetArchived.Parameters["@ArchivedUrls"].Value = value;
                SQLiteSetArchived.ExecuteNonQuery();
            }
        }
        public static float AverageExcluded
        {
            get
            {
                CheckInitialized();
                return Convert.ToSingle(SQLiteAvgExcluded.ExecuteScalar());
            }
        }
        public int Excluded
        {
            get
            {
                SQLiteGetExcluded.Parameters["@Name"].Value = User;
                return Convert.ToInt32(SQLiteGetExcluded.ExecuteScalar());
            }
            set
            {
                SQLiteSetExcluded.Parameters["@Name"].Value = User;
                SQLiteSetExcluded.Parameters["@ExcludedUrls"].Value = value;
                SQLiteSetExcluded.ExecuteNonQuery();
            }
        }
        string User;
        public void AddUrlUsed(string url)
        {
            if (OptedOut)
            {
                return;
            }
            if (Program.exclusions.IsMatch(url.ToString()) || Program.YoutubeRegex.IsMatch(url))
            {
                Excluded++;
                return;
            }
            if (Program.providers.IsMatch(url.ToString()))
            {
                Archived++;
            }
            else
            {
                Unarchived++;
            }
            if (Program.ImageRegex.IsMatch(url.ToString()) || Program.ImageRegex.IsMatch(new Uri(url).AbsolutePath))
            {
                Image++;
            }
        }
        static void InitDbTable()
        {
            using (SQLiteCommand cmd = new SQLiteCommand("create table if not exists Users (Name text unique, UnarchivedUrls integer, ImageUrls integer, ArchivedUrls integer, ExcludedUrls integer, OptedOut integer)", Connection))
                cmd.ExecuteNonQuery();
        }
        static void InitDbCommands()
        {
            SQLiteParameter UserNameParam = new SQLiteParameter("@Name", DbType.String);

            SQLiteGetUserExists = new SQLiteCommand("select count(*) from Users where Name = @Name", Connection);
            SQLiteGetUserExists.Parameters.Add(UserNameParam);

            SQLiteAddUser = new SQLiteCommand("insert or abort into Users(Name, UnarchivedUrls, ImageUrls, ArchivedUrls, ExcludedUrls, OptedOut) values(@Name, 0, 0, 0, 0, 0)", Connection);
            SQLiteAddUser.Parameters.Add(UserNameParam);

            SQLiteGetUnarchived = new SQLiteCommand("select UnarchivedUrls from Users where Name = @Name", Connection);
            SQLiteGetUnarchived.Parameters.Add(UserNameParam);

            SQLiteGetArchived = new SQLiteCommand("select ArchivedUrls from Users where Name = @Name", Connection);
            SQLiteGetArchived.Parameters.Add(UserNameParam);

            SQLiteGetOptOut = new SQLiteCommand("select OptedOut from Users where Name = @Name", Connection);
            SQLiteGetOptOut.Parameters.Add(UserNameParam);

            SQLiteGetExcluded = new SQLiteCommand("select ExcludedUrls from Users where Name = @Name", Connection);
            SQLiteGetExcluded.Parameters.Add(UserNameParam);

            SQLiteSetArchived = new SQLiteCommand("update Users set ArchivedUrls = @ArchivedUrls where Name = @Name", Connection);
            SQLiteSetArchived.Parameters.Add(new SQLiteParameter("@ArchivedUrls", DbType.Int32));
            SQLiteSetArchived.Parameters.Add(UserNameParam);

            SQLiteSetExcluded = new SQLiteCommand("update Users set ExcludedUrls = @ExcludedUrls where Name = @Name", Connection);
            SQLiteSetExcluded.Parameters.Add(new SQLiteParameter("@ExcludedUrls", DbType.Int32));
            SQLiteSetExcluded.Parameters.Add(UserNameParam);

            SQLiteSetUnarchived = new SQLiteCommand("update Users set UnarchivedUrls = @UnarchivedUrls where Name = @Name", Connection);
            SQLiteSetUnarchived.Parameters.Add(new SQLiteParameter("@UnarchivedUrls", DbType.Int32));
            SQLiteSetUnarchived.Parameters.Add(UserNameParam);

            SQLiteSetOptOut = new SQLiteCommand("update Users set OptedOut = @OptedOut where Name = @Name", Connection);
            SQLiteSetOptOut.Parameters.Add(new SQLiteParameter("@OptedOut", DbType.Int32));
            SQLiteSetOptOut.Parameters.Add(UserNameParam);

            SQLiteSetImage = new SQLiteCommand("update Users set ImageUrls = @ImageUrls where Name = @Name", Connection);
            SQLiteSetImage.Parameters.Add(new SQLiteParameter("@ImageUrls", DbType.Int32));
            SQLiteSetImage.Parameters.Add(UserNameParam);

            SQLiteGetImage = new SQLiteCommand("select ImageUrls from Users where Name = @Name", Connection);
            SQLiteGetImage.Parameters.Add(UserNameParam);

            SQLiteAvgArchived = new SQLiteCommand("select avg(ArchivedUrls) from Users");

            SQLiteAvgExcluded = new SQLiteCommand("select avg(ExcludedUrls) from Users");

            SQLiteAvgImage = new SQLiteCommand("select avg(ImageUrls) from Users");

            SQLiteAvgUnarchived = new SQLiteCommand("select avg(UnarchivedUrls) from Users");
        }
        public RedditUserProfileSqlite(string fileName = "redditusers.sqlite")
        {
            if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory.TrimEnd('/')}/Data/{fileName}"))
            {
                SQLiteConnection.CreateFile($"{AppDomain.CurrentDomain.BaseDirectory.TrimEnd('/')}/Data/{fileName}");
            }
            AppDomain.CurrentDomain.SetData("DataDirectory", $"{AppDomain.CurrentDomain.BaseDirectory.TrimEnd('/')}/Data/");
            Connection = new SQLiteConnection($"Data Source=|DataDirectory|{fileName};Version=3;");
            Connection.Open();
            InitDbTable();
            InitDbCommands();
            Initialized = true;
        }
        public RedditUserProfileSqlite(RedditUser user)
        {
            CheckInitialized();
            User = user.Name;
            if (!UserExists(user))
            {
                SQLiteAddUser.Parameters["@Name"].Value = user.Name;
                SQLiteAddUser.ExecuteNonQuery();
            }
        }
    }
}

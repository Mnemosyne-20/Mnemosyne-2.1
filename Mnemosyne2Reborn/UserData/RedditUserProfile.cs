using Newtonsoft.Json;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Data.SQLite.Linq;
namespace Mnemosyne2Reborn
{
    [JsonObject]
    public class RedditUserProfile
    {
        /// <summary>
        /// string is the name of the User, and the Profile is the profile
        /// </summary>
        [JsonIgnore]
        static bool UseSQLite;
        [JsonIgnore]
        public static Dictionary<string, RedditUserProfile> Users;
        [JsonIgnore]
        static SQLiteConnection dbConnection;
        [JsonIgnore]
        static SQLiteCommand SQLCmd_InsertNewListing, SQLCmd_AddExcluded, SQLCmd_GetExcluded, SQLCmd_AddImage, SQLCmd_GetImage, SQLCmd_AddArchived, SQLCmd_GetArchived, SQLCmd_AddUnarchived, SQLCmd_GetUnarchived, SQLCmd_OptOut;
        [JsonIgnore]
        public RedditUser User;
        [JsonProperty("Name")]
        public string Name;
        [JsonProperty("ArchiveUrlsUsed")]
        public int ArchivedUrlsUsed;
        [JsonProperty("UnArchivedUrlsUsed")]
        public int UnArchivedUrlsUsed;
        [JsonProperty("ExcludedUrlsUsed")]
        public int ExcludedUrlsUsed;
        [JsonProperty("ImageUrlsUsed")]
        public int ImageUrlsUsed;
        [JsonProperty("OptedOut")]
        public bool OptedOut;
        public void OptOut(bool val)
        {
            OptedOut = val;
            if (Users.ContainsKey(User.Name))
            {
                Users[User.Name] = this;
            }
            DumpUserData();
        }
        /// <summary> 
        /// Adds a url used to a user that you made
        /// </summary>
        /// <param name="url">The url to be added</param>
        public void AddUrlUsed(Uri url)
        {
            if (OptedOut)
            {
                return;
            }
            if (Program.exclusions.IsMatch(url.ToString()))
            {
                ExcludedUrlsUsed++;
                DumpUserData();
                return;
            }
            if (Program.providers.IsMatch(url.ToString()))
            {
                ArchivedUrlsUsed++;
            }
            else
            {
                UnArchivedUrlsUsed++;
            }
            if (Program.ImageRegex.IsMatch(url.ToString()))
            {
                ImageUrlsUsed++;
            }
            if (Users.ContainsKey(User.Name))
            {
                Users[User.Name] = this;
            }
            DumpUserData();
        }
        /// <summary> 
        /// Adds a url used to a user that you made
        /// </summary>
        /// <param name="Url">The url to be added</param>
        public void AddUrlUsed(string Url)
        {
            AddUrlUsed(new Uri(Url));
        }
        public static void DumpUserData()
        {
            string val = JsonConvert.SerializeObject(Users, Formatting.Indented);
            File.WriteAllText("./Data/Users.json", val);
        }
        static void InitDatabase()
        {
            string query = "create table if not exists Users (Name text unique, ArchiveUrlsUsed int, UnArchivedUrlsUsed int, ExcludedUrlsUsed int, ImageUrlsUsed int, OptedOut bool not null check (OptedOut in (0,1))";
            SQLiteCommand cmd = new SQLiteCommand(query, dbConnection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            finally
            {
                cmd.Dispose();
            }
        }
        static void InitCommands()
        {
            SQLCmd_OptOut = new SQLiteCommand("Update Users set OptedOut = @OptedOut where Name = @Name");
            SQLCmd_OptOut.Parameters.Add(new SQLiteParameter("@OptedOut"));
            SQLCmd_OptOut.Parameters.Add(new SQLiteParameter("@Name"));

            SQLCmd_GetArchived = new SQLiteCommand("select ArchivedUrlsUsed from Users where Name = @Name");
            SQLCmd_GetArchived.Parameters.Add(new SQLiteParameter("@Name"));

            SQLCmd_AddArchived = new SQLiteCommand("Update Users set ArchviedUrlsUsed = @ArchivedUrlsUsed where Name = @Name");
            SQLCmd_AddArchived.Parameters.Add(new SQLiteParameter("@ArchivedUrlsUsed"));
            SQLCmd_AddArchived.Parameters.Add(new SQLiteParameter("@Name"));

            SQLCmd_GetUnarchived = new SQLiteCommand("select UnArchivedUrlsUsed from Users where Name = @Name");
            SQLCmd_GetUnarchived.Parameters.Add(new SQLiteParameter("@Name"));

            SQLCmd_AddUnarchived = new SQLiteCommand("Update Users set UnArchivedUrlsUsed = @UnArchivedUrlsUsed where Name = @Name");
            SQLCmd_GetUnarchived.Parameters.Add(new SQLiteParameter("@UnArchivedUrlsUsed"));
            SQLCmd_GetUnarchived.Parameters.Add(new SQLiteParameter("@Name"));

            SQLCmd_GetImage = new SQLiteCommand("select ImageUrlsUsed from Users where Name = @Name");
            SQLCmd_GetImage.Parameters.Add(new SQLiteParameter("@Name"));

            SQLCmd_AddImage = new SQLiteCommand("Update Users set ImageUrlsUsed = @ImageUrlsUsed where Name = @Name");
            SQLCmd_AddImage.Parameters.Add(new SQLiteParameter("@ImageUrlsUsed"));
            SQLCmd_AddImage.Parameters.Add(new SQLiteParameter("@Name"));

            SQLCmd_GetExcluded = new SQLiteCommand("select ExcludedUrlsUsed from Users where Name = @Name");
            SQLCmd_GetExcluded.Parameters.Add(new SQLiteParameter("@Name"));

            SQLCmd_AddExcluded = new SQLiteCommand("Update Users set ExcludedUrlsUsed = @ExcludedUrlsUsed where Name = @Name");
            SQLCmd_AddExcluded.Parameters.Add(new SQLiteParameter("@ExcludedUrlsUsed"));
            SQLCmd_AddExcluded.Parameters.Add(new SQLiteParameter("@ExcludedUrlsUsed"));
        }
        static RedditUserProfile()
        {
            if (!Directory.Exists("./Data"))
                Directory.CreateDirectory("./Data/");
            if (File.Exists("./Data/Users.json"))
            {
                Users = JsonConvert.DeserializeObject<Dictionary<string, RedditUserProfile>>(File.ReadAllText("./Data/Users.json"));
            }
            else
            {
                Users = new Dictionary<string, RedditUserProfile>();
            }
        }
        /// <summary>
        /// ONLY EXISTS FOR JSON SERIALIZATION
        /// DO NOT USE
        /// </summary>
        public RedditUserProfile()
        {

        }
        public RedditUserProfile(RedditUser user, bool useSQLite)
        {
            this.User = user;
            this.Name = User?.Name;
            if (useSQLite)
            {
                if (!File.Exists("./Data/UserProfiles.sqlite"))
                {
                    SQLiteConnection.CreateFile("./Data/UserProfiles.sqlite");
                }
                string assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location + "/Data/");
                AppDomain.CurrentDomain.SetData("DataDirectory", assemblyPath);
                dbConnection = new SQLiteConnection($"Data Source=|DataDirectory|/UserProfiles.sqlite;Version=3;");
                dbConnection.Open();
                InitDatabase();
                InitCommands();
            }
            else
            {
                if (!Users.ContainsKey(User.Name))
                {
                    Users.Add(User.Name, new RedditUserProfile() { ArchivedUrlsUsed = 0, UnArchivedUrlsUsed = 0, User = user, Name = user.Name, ExcludedUrlsUsed = 0, OptedOut = false });
                }
            }
            this.ArchivedUrlsUsed = Users[User.Name].ArchivedUrlsUsed;
            this.UnArchivedUrlsUsed = Users[User.Name].UnArchivedUrlsUsed;
            this.OptedOut = Users[User.Name].OptedOut;
            this.ExcludedUrlsUsed = Users[user.Name].ExcludedUrlsUsed;
            this.ImageUrlsUsed = Users[user.Name].ExcludedUrlsUsed;
            DumpUserData();
        }
    }
}

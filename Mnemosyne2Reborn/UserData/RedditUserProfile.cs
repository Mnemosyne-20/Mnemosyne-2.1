using Newtonsoft.Json;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
namespace Mnemosyne2Reborn
{
    [JsonObject]
    public class RedditUserProfile
    {
        /// <summary>
        /// string is the name of the User, and the Profile is the profile
        /// </summary>
        [JsonIgnore]
        public static Dictionary<string, RedditUserProfile> Users;
        [JsonIgnore]
        static SQLiteConnection dbConnection;
        [JsonIgnore]
        static SQLiteCommand InsertCommand, ExtractCommand;
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
        public bool OptedOut { get => optedOut; set { optedOut = value; DumpUserData(); } }
        [JsonIgnore]
        private bool optedOut;
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
        void InitDatabase()
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
        void InitCommands()
        {
            InsertCommand = new SQLiteCommand("insert or abort into Users (Name, ArchiveUrlsUsed, UnArchivedUrlsUsed, ExcludedUrlsUsed, ImageUrlsUsed, OptedOut) values (@Name, @ArchiveUrlsUsed, @UnArchivedUrlsUsed, @ExcludedUrlsUsed, @ImageUrlsUsed, @OptedOut)");
            InsertCommand.Parameters.Add(new SQLiteParameter("@Name"));
            InsertCommand.Parameters.Add(new SQLiteParameter("@ArchiveUrlsUsed"));
            InsertCommand.Parameters.Add(new SQLiteParameter("@UnArchivedUrlsUsed"));
            InsertCommand.Parameters.Add(new SQLiteParameter("@ExcludedUrlsUsed"));
            InsertCommand.Parameters.Add(new SQLiteParameter("@ImageUrlsUsed"));
            InsertCommand.Parameters.Add(new SQLiteParameter("@OptedOut"));
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
                string assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                AppDomain.CurrentDomain.SetData("DataDirectory", assemblyPath);
                dbConnection = new SQLiteConnection($"Data Source=|DataDirectory|/Data/UserProfiles.sqlite;Version=3;");
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

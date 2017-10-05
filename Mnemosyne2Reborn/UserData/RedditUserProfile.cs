using Newtonsoft.Json;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.IO;
namespace Mnemosyne2Reborn.UserData
{
    [JsonObject]
    [Obsolete("Use the new RedditUserProfileSqlite class instead", false)]
    public class RedditUserProfile
    {
        [JsonIgnore]
        public static Dictionary<string, RedditUserProfile> Users;
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
        static RedditUserProfile()
        {
            if (!Directory.Exists("./Data"))
                Directory.CreateDirectory("./Data/");
            Users = File.Exists("./Data/Users.json") ? JsonConvert.DeserializeObject<Dictionary<string, RedditUserProfile>>(File.ReadAllText("./Data/Users.json")) : new Dictionary<string, RedditUserProfile>();
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
            if (!Users.ContainsKey(User.Name))
            {
                Users.Add(User.Name, new RedditUserProfile() { ArchivedUrlsUsed = 0, UnArchivedUrlsUsed = 0, User = user, Name = user.Name, ExcludedUrlsUsed = 0, OptedOut = false });
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
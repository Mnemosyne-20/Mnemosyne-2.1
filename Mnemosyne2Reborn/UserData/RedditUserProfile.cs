using Newtonsoft.Json;
using RedditSharp.Things;
using System.Collections.Generic;
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
        /// <summary> 
        /// Adds a url used to a user that you made
        /// </summary>
        /// <param name="Url">The url to be added</param>
        public void AddUrlUsed(string Url)
        {
            if (OptedOut)
            {
                return;
            }
            if (Program.exclusions.IsMatch(Url))
            {
                ExcludedUrlsUsed++;
                return;
            }
            if (Program.providers.IsMatch(Url))
            {
                ArchivedUrlsUsed++;
            }
            else
            {
                UnArchivedUrlsUsed++;
            }
            if (Program.ImageRegex.IsMatch(Url))
            {
                ImageUrlsUsed++;
            }
            if (Users.ContainsKey(User.Name))
            {
                Users[User.Name] = this;
            }
            DumpUserData();
        }
        public static void DumpUserData()
        {
            string val = JsonConvert.SerializeObject(Users, Formatting.Indented);
            File.WriteAllText("./Data/Users.json", val);
        }
        static RedditUserProfile()
        {
            if (Users == null)
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
        }
        /// <summary>
        /// ONLY EXISTS FOR JSON SERIALIZATION
        /// </summary>
        public RedditUserProfile()
        {

        }
        public RedditUserProfile(RedditUser user, bool UseSQLite)
        {
            //this.Init();
            this.User = user;
            this.Name = User.Name;
            if (!UseSQLite)
            {
                if (!Users.ContainsKey(User.Name))
                {
                    Users.Add(User.Name, new RedditUserProfile() { ArchivedUrlsUsed = 0, UnArchivedUrlsUsed = 0, User = user, Name = user.Name, ExcludedUrlsUsed = 0, OptedOut = false });
                }
            }
            else
            {

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

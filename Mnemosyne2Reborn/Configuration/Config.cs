using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
namespace Mnemosyne2Reborn.Configuration
{
    public class Config
    {
        [JsonProperty("OAuthSecert")]
        public string OAuthSecret;
        [JsonProperty("OAuthClientId")]
        public string OAuthClientId;
        [JsonProperty("FlavorText")]
        public string[] FlavorText;
        [JsonProperty("Username")]
        public string Username;
        [JsonProperty("Password")]
        public string Password;
        [JsonProperty("UseSQLite")]
        public bool Sqlite;
        [JsonProperty("Version")]
        public int Ver;
        [JsonProperty("Subreddit")]
        public string[] Subreddits;
        [JsonProperty("ArchiveLinks")]
        public bool ArchiveLinks;
        [JsonProperty("ArchiveService")]
        public string ArchiveService;
        [JsonProperty("UseOAuth")]
        public bool UseOAuth;
        
        /// <summary>
        /// EXISTS ONLY FOR JSONCONVERT
        /// </summary>
        public Config()
        {

        }
        public Config(bool SQLite, string Username, string[] Subreddits, string Password, bool UseOAuth, string OAuthSecret = null, string OAuthClientId = null, bool ArchiveLinks = false, string ArchiveService = "https://www.archive.is")
        {
            Sqlite = SQLite;
            this.Username = Username ?? throw new Exception("THIS IS A REQUIRED FEILD");
            this.OAuthClientId = OAuthClientId;
            this.OAuthSecret = OAuthSecret;
            this.Password = Password ?? throw new Exception("THIS IS A REQUIRED FIELD");
            this.Subreddits = Subreddits ?? throw new Exception("THIS IS A REQUIRED FIELD");
            this.ArchiveLinks = ArchiveLinks;
            this.UseOAuth = UseOAuth;
            FlavorText = new string[] { };
            this.ArchiveService = ArchiveService;
            Ver = 1;
            File.WriteAllText("./Data/Settings.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        public static Config GetConfig()
        {
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText("./Data/Settings.json"));
        }
    }
}

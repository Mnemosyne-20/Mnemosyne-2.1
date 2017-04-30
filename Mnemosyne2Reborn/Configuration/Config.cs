using Newtonsoft.Json;
using System;
using System.IO;
namespace Mnemosyne2Reborn.Configuration
{
    public class Config
    {
        [JsonProperty("OAuthSecert")]
        public string OAuthSecret { get; set; }
        [JsonProperty("OAuthClientId")]
        public string OAuthClientId { get; set; }
        [JsonProperty("FlavorText")]
        public string[] FlavorText { get; set; }
        [JsonProperty("Username")]
        public string UserName { get; set; }
        [JsonProperty("Password")]
        public string Password { get; set; }
        [JsonProperty("UseSQLite")]
        public bool SQLite { get; set; }
        [JsonProperty("Version")]
        public int Ver { get; private set; }
        [JsonProperty("Subreddit")]
        public string[] Subreddits { get; set; }
        [JsonProperty("ArchiveLinks")]
        public bool ArchiveLinks { get; set; }
        [JsonProperty("ArchiveService")]
        public string ArchiveService { get; set; }
        [JsonProperty("UseOAuth")]
        public bool UseOAuth { get; set; }

        /// <summary>
        /// EXISTS ONLY FOR JSONCONVERT
        /// DO NOT USE
        /// </summary>
        public Config()
        {

        }
        public Config(bool SQLite, string UserName, string[] Subreddits, string Password, bool UseOAuth = false, string OAuthSecret = null, string OAuthClientId = null, bool ArchiveLinks = false, string ArchiveService = "https://www.archive.is")
        {
            this.SQLite = SQLite;
            this.UserName = UserName ?? throw new ArgumentNullException("Username");
            this.OAuthClientId = OAuthClientId;
            this.OAuthSecret = OAuthSecret;
            this.Password = Password ?? throw new ArgumentNullException("Password");
            this.Subreddits = Subreddits ?? throw new ArgumentNullException("Subreddits");
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

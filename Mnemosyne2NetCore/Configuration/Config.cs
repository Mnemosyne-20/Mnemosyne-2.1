using Newtonsoft.Json;
using System;
using System.IO;
namespace Mnemosyne2Reborn.Configuration
{
    /// <summary>
    /// Made to be used to quickly giving away the fact that the configuration updated, and when it updated...
    /// </summary>
    public class ConfigEventArgs : EventArgs
    {
        public ConfigEventArgs(Config c) => Config = c;
        public Config Config { get; }
    }
    [Serializable]
    public class Config
    {
        [JsonProperty("OAuthSecert")]
        public string OAuthSecret { get; set; }
        [JsonProperty("OAuthClientId")]
        public string OAuthClientId { get; set; }
        [JsonProperty("FlavorText")]
        public string[] FlavorText { get; set; }
        [JsonRequired]
        [JsonProperty("Username")]
        public string UserName { get; set; }
        [JsonRequired]
        [JsonProperty("Password")]
        public string Password { get; set; }
        [JsonProperty("UseSQLite")]
        public bool SQLite { get; set; }
        /// <summary>
        /// This exists so that we know what version of the filesystem we're on, I'm going to attempt to figure out how to use multiple config versions and update them eventually :(
        /// </summary>
        [JsonRequired]
        [JsonProperty("Version")]
        public int Ver { get; private set; }
        [JsonRequired]
        [JsonProperty("Subreddit")]
        public ArchiveSubredditJson[] Subreddits { get; set; }
        [JsonProperty("ArchiveLinks")]
        public bool ArchiveLinks { get; set; }
        [JsonProperty("ArchiveService")]
        public string ArchiveService { get; set; }
        [JsonRequired]
        [JsonProperty("UseOAuth")]
        public bool UseOAuth { get; set; }
        [JsonProperty("RedirectURI")]
        public string RedirectURI { get; set; }
        [JsonProperty("ConvertToSQLite")]
        public bool ConvertToSQLite { get; set; }
        /// <summary>
        /// EXISTS ONLY FOR JSONCONVERT
        /// DO NOT USE
        /// </summary>
        public Config()
        {
        }
        public Config(bool SQLite, string UserName, ArchiveSubredditJson[] Subreddits, string Password, bool UseOAuth = true, string OAuthSecret = null, string OAuthClientId = null, bool ArchiveLinks = false, string ArchiveService = "http://www.archive.fo", string RedirectURI = "https://github.com/Mnemosyne-20/Mnemosyne-2.1")
        {
            if (!Directory.Exists("./Data/"))
            {
                Directory.CreateDirectory("./Data/");
            }
            this.SQLite = SQLite;
            this.UserName = UserName ?? throw new ArgumentNullException(nameof(UserName), "Username");
            this.UseOAuth = UseOAuth;
            this.OAuthClientId = OAuthClientId;
            this.OAuthSecret = OAuthSecret;
            this.Password = Password ?? throw new ArgumentNullException(nameof(Password), "Password is null");
            this.Subreddits = Subreddits ?? throw new ArgumentNullException(nameof(Subreddits), "Subreddits is null");
            this.ArchiveLinks = ArchiveLinks;
            FlavorText = new string[] { };
            this.ArchiveService = ArchiveService;
            Ver = 5;
            this.RedirectURI = RedirectURI;
            this.ConvertToSQLite = false;
            DumpConfig();
        }
        public void DumpConfig()
        {
            File.WriteAllText("./Data/Settings.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        public static Config GetConfig() => JsonConvert.DeserializeObject<Config>(File.ReadAllText("./Data/Settings.json"));
    }
}
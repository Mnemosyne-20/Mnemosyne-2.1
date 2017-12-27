using ArchiveApi;
using ArchiveApi.Interfaces;
using Mnemosyne2Reborn.BotState;
using Mnemosyne2Reborn.Commenting;
using Mnemosyne2Reborn.Configuration;
using Mnemosyne2Reborn.UserData;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
namespace Mnemosyne2Reborn
{
    public class Program
    {

        #region static values
        public readonly static string[] ArchiveBots = new string[]
        {
            "mnemosyne-0001",
            "mnemosyne-0002",// I've seen you!
            "SpootsTestBot", // hey I know you!
            "Mentioned_Videos",
            "AutoModerator",
            "TotesMessenger",
            "TweetPoster",
            "RemindMeBot",
            "thelinkfixerbot",
            "gifv-bot",
            "autourbanbot",
            "deepsalter-001",
            "GoodBot_BadBot",
            "PORTMANTEAU-BOT",
            "GoodBot_BadBot_Karma",
            "MTGCardFetcher"
        };
        /// <summary>
        /// Iterates each "thing" you make, subreddit is required for a few of them
        /// </summary>
        /// <param name="reddit"></param>
        /// <param name="state"></param>
        /// <param name="subbreddit"></param>
        public delegate void IterateThing(Reddit reddit, IBotState state, ArchiveSubreddit subbreddit);
        public delegate void IterateSeparateConfigThing(Reddit reddit, IBotState state, ArchiveSubreddit[] subreddits, Config config);
        public delegate void IterateSubredditThing(Reddit reddit, IBotState state, ArchiveSubreddit subreddit, Config config);
        public static IterateSubredditThing IteratePost;
        public static IterateSubredditThing IterateComment;
        public static IterateThing IterateMessage;
        public static IterateSeparateConfigThing Iterate24Hours;
        /// <summary>
        /// This is intentional to be this way, it's so that the editor can get the headers easily
        /// </summary>
        public static readonly string[] Headers = new string[] { "Archives for this post:\n\n", "Archive for this post:\n\n", "Archives for the links in comments:\n\n", "----\nI am Mnemosyne 2.1, {0} ^^^^/r/botsrights ^^^^[Contribute](https://github.com/Mnemosyne-20/Mnemosyne-2.1) ^^^^message ^^^^me ^^^^suggestions ^^^^at ^^^^any ^^^^time ^^^^Opt ^^^^out ^^^^of ^^^^tracking ^^^^by ^^^^messaging ^^^^me ^^^^\"Opt ^^^^Out\" ^^^^at ^^^^any ^^^^time", "Archives after 24 hours:\n\n" };
        /// <summary>
        /// These three being separate is important because it is used for data tracking
        /// </summary>
        public readonly static Regex exclusions = new Regex(@"(facebook\.com|giphy\.com|youtube\.com|streamable\.com|www\.gobrickindustry\.us|gyazo\.com|sli\.mg|imgur\.com|reddit\.com/message|youtube\.com|youtu\.be|wiki/rules|politics_feedback_results_and_where_it_goes_from|urbandictionary\.com)");
        public readonly static Regex providers = new Regex(@"(web-beta.archive.org|archive\.is|archive\.fo|archive\.org|archive\.today|megalodon\.jp|web\.archive\.org|webcache\.googleusercontent\.com|archive\.li)");
        public readonly static Regex ImageRegex = new Regex(@"(\.gif|\.jpg|\.png|\.pdf|\.webm|\.mp4)$");
        #region Locks
        static object LockConfigObject = new object();
        static object LockArchiveSubredditsObject = new object();
        #endregion
        #endregion
        #region Local Values
        Reddit reddit;
        public event EventHandler<ConfigEventArgs> UpdatedConfig;
        public event EventHandler<ArchiveSubredditEventArgs> UpdatedArchiveSubreddits;
        Config _config;
        public Config Config
        {
            get
            {
                return _config;
            }
            set
            {
                lock (LockConfigObject)
                {
                    _config = value;
                }
                UpdatedConfig?.Invoke(this, new ConfigEventArgs(_config));
            }
        }
        ArchiveSubreddit[] _archiveSubreddits;
        public ArchiveSubreddit[] ArchiveSubreddits
        {
            get
            {
                return _archiveSubreddits;
            }
            set
            {
                lock (LockArchiveSubredditsObject)
                {
                    _archiveSubreddits = value;
                }
                UpdatedArchiveSubreddits?.Invoke(this, new ArchiveSubredditEventArgs(_archiveSubreddits));
            }
        }
        #endregion
        public static void GetHelp()
        {
            Console.WriteLine("Mnemosyne - 2.1 by chugga_fan");
            Console.WriteLine("Currently no supported command line options, but future options will be:");
            Console.WriteLine("\t--server | -s\tWill be used to start a web hosted version, with an ASP.NET host");
        }
        static void Main(string[] args)
        {
            Console.Title = "Mnemosyne-2.1 by chugga_fan";
            foreach (string s in args)
            {
                switch (s)
                {
                    case "--server":
                    case "-s":
                        break;
                    case "--help":
                    case "-h":
                    case "-?":
                        GetHelp();
                        return;
                    default:
                        break;
                }
            }
            Program p = new Program();
        }
        public static ArchiveSubreddit[] InitializeArchiveSubreddits(Reddit reddit, Config config)
        {
            ArchiveSubreddit[] ArchiveSubreddits = new ArchiveSubreddit[config.Subreddits.Length];
            for (int i = 0; i < config.Subreddits.Length; i++)
            {
                ArchiveSubreddits[i] = reddit.GetArchiveSubreddit(config.Subreddits[i]);
            }
            return ArchiveSubreddits;
        }
        public Program()
        {
            Console.Clear();
            lock (LockConfigObject)
            {
                Config = !File.Exists("./Data/Settings.json") ? CreateNewConfig() : Config.GetConfig();
            }
            using (IBotState botstate = Config.SQLite ? (IBotState)new SQLiteBotState() : new FlatBotState())
            {
                WebAgent agent = null;
                if (Config.UseOAuth)
                {
                    agent = new BotWebAgent(Config.UserName, Config.Password, Config.OAuthClientId, Config.OAuthSecret, Config.RedirectURI);
                }
#pragma warning disable CS0618 // Type or member is obsolete
                lock (LockConfigObject)
                {
                    reddit = Config.UseOAuth ? new Reddit(agent) : new Reddit(Config.UserName, Config.Password);
                }
                reddit.InitOrUpdateUser();
                UpdatedConfig += (sender, e) => { ArchiveSubreddits = InitializeArchiveSubreddits(reddit, e.Config); };
                UpdatedArchiveSubreddits += (sender, e) => { Console.Title = "Updated Archive Subreddits"; };
                lock (LockConfigObject)
                {
                    ArchiveSubreddits = InitializeArchiveSubreddits(reddit, Config);
                }
                IteratePost = IteratePosts;
                IterateComment = IterateComments;
                IterateMessage = IterateMessages;
                Iterate24Hours = Iterate24HourArchive; // currently neutered so that it just does regular 24 hour passes
                new RedditUserProfileSqlite();
                if (File.Exists("./Data/Users.json"))
                {
                    RedditUserProfileSqlite.TransferProfilesToSqlite(RedditUserProfile.Users);
                    File.Delete("./Data/Users.json");
                }
#pragma warning restore CS0618 // Type or member is obsolete
                IArchiveService service = new ArchiveService(DefaultServices.ArchiveFo).CreateNewService();
                ArchiveLinks.SetArchiveService(service);
                PostArchives.SetArchiveService(service);
                MainLoop(reddit, botstate);
            }
        }
        public void MainLoop(Reddit reddit, IBotState botstate)
        {
            while (true) // main loop, calls delegates that move thrugh every subreddit allowed iteratively
            {
                try
                {
                    lock (LockArchiveSubredditsObject)
                    {
                        foreach (ArchiveSubreddit sub in ArchiveSubreddits) // Iterates allowed subreddits
                        {
                            IteratePost?.Invoke(reddit, botstate, sub, Config);
                            IterateComment?.Invoke(reddit, botstate, sub, Config);
                            IterateMessage?.Invoke(reddit, botstate, sub);
                        }
                        Iterate24Hours?.Invoke(reddit, botstate, ArchiveSubreddits, Config);
                    }
                    Console.Title = $"Sleeping, New messages: {reddit.User.UnreadMessages.Count() >= 1}";
                }
                catch (WebException e) when (e.Message.Contains("(404)") || e.Message.Contains("Cannot resolve hostname") && (int)((HttpWebResponse)e.Response).StatusCode <= 500 && (int)((HttpWebResponse)e.Response).StatusCode >= 600)
                {
                    Console.WriteLine("Connect to the internet, Error: " + e.Message);
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("(502)") || e.Message.Contains("(503)") || e.Message.Contains("The remote name could not be resolved"))
                    {
                        continue;
                    }
                    // Catches errors and documents them, I should switch to a System.Diagnostics logger but I have no experience with it
                    if (!Directory.Exists("./Errors"))
                    {
                        Directory.CreateDirectory("./Errors");
                    }
                    File.AppendAllText("./Errors/Failures.txt", $"{e.ToString()}{Environment.NewLine}");
                    Console.WriteLine($"Caught an exception of type {e.GetType()} output is in ./Errors/Failures.txt");
                }
                Thread.Sleep(1000); // sleeps for one second to help with the reddit calls
            }
        }
        public static Config CreateNewConfig()
        {
            Console.WriteLine("Would you like to store data using SQLite instead of JSON files? (Yes/No)");
            bool useSQLite = Console.ReadLine().ToLower()[0] == 'y';
            Console.WriteLine("Would you like to create a new account? (Yes/No)");
            string Username, Password;
            if (Console.ReadLine().ToLower()[0] == 'y')
            {
                Reddit red = new Reddit();
                Console.WriteLine("Input a username");
                Username = Console.ReadLine();
                Console.WriteLine("Input a password");
                Password = Console.ReadLine();
                AuthenticatedUser user = red.RegisterAccount(Username, Password);
            }
            Console.WriteLine("What is your username?");
            Username = Console.ReadLine();
            Console.WriteLine("What is your password? note: required and is stored in plaintext, suggest you use a secure system");
            Password = Console.ReadLine();
            Console.WriteLine("How many subreddits are you watching?");
            int len;
            while (!int.TryParse(Console.ReadLine(), out len))
                Console.WriteLine("Please input a valid integer.");
            ArchiveSubredditJson[] Subs = new ArchiveSubredditJson[len];
            for (int i = 0; i < len; i++)
            {
                Console.WriteLine("What is the name of the subreddit?");
                string name = Console.ReadLine();
                Console.WriteLine("Would you like to archive posts? (Yes/No)");
                bool ArcPost = Console.ReadLine().ToLower()[0] == 'y';
                bool Arc24Hours = false;
                if (ArcPost)
                {
                    Console.WriteLine("Would you like to archive posts again after 24 hours? (Yes/No)");
                    Arc24Hours = Console.ReadLine().ToLower()[0] == 'y';
                }
                Console.WriteLine("Would you like to archive links in comments? (Yes/No)");
                bool ArcComments = Console.ReadLine().ToLower()[0] == 'y';
                ArchiveSubredditJson arcSubJson = new ArchiveSubredditJson()
                {
                    ArchiveCommentLinks = ArcComments,
                    ArchivePost = ArcPost,
                    Name = name,
                    ArchiveWebsite = "archive.fo",
                    ArchiveAfter24Hours = Arc24Hours
                };
                Subs[i] = arcSubJson;
            }
            Console.WriteLine("Would you like to use OAuth? (Yes/No)");
            bool wantOAuth = Console.ReadLine().ToLower()[0] == 'y';
            string ClientID = null, ClientSecret = null;
            if (wantOAuth)
            {
                Console.WriteLine("Get an OAuth client ID and Secret");
                Console.WriteLine("What is your clientID?");
                ClientID = Console.ReadLine();
                Console.WriteLine("What is your client secret?");
                ClientSecret = Console.ReadLine();
            }
            Console.WriteLine("Do you want to archive post links? (Yes/No)");
            bool ArchiveLinks = Console.ReadLine().ToLower()[0] == 'y';
            Console.WriteLine("To add flavortext, you must manually add it in as an array in the ./Data/Settings.json file");
            Console.Title = "Sleeping for 10000 ms";
            Thread.Sleep(10000);
            return new Config(useSQLite, Username, Subs, Password, wantOAuth, ClientSecret, ClientID, ArchiveLinks);
        }
        #region IterateThings
        public static void IterateMessages(Reddit reddit, IBotState state, ArchiveSubreddit subreddit)
        {
            if (reddit == null || state == null || subreddit == null)
            {
                throw new ArgumentNullException(reddit == null ? nameof(reddit) : state == null ? nameof(state) : nameof(subreddit));
            }
            foreach (var message in reddit.User.PrivateMessages.Take(25))
            {
                if (!message.Unread)
                {
                    break;
                }
                switch (message.Body.ToLower())
                {
                    case "opt out":
                        Console.WriteLine($"User {message.Author} has opted out.");
                        new RedditUserProfileSqlite(reddit.GetUser(message.Author)).OptedOut = true;
                        message.SetAsRead();
                        break;
                    case "opt in":
                        Console.WriteLine($"User {message.Author} has opted in");
                        new RedditUserProfileSqlite(reddit.GetUser(message.Author)).OptedOut = false;
                        message.SetAsRead();
                        break;
                }
            }
        }
        public static void IteratePosts(Reddit reddit, IBotState state, ArchiveSubreddit subreddit, Config config)
        {
            if (reddit == null || state == null || subreddit == null || config == null)
            {
                throw new ArgumentNullException(reddit == null ? nameof(reddit) : state == null ? nameof(state) : config == null ? nameof(config) : nameof(subreddit));
            }
            Console.Title = $"Finding posts in {subreddit.Name} New messages: {reddit.User.UnreadMessages.Count() >= 1}";
            foreach (var post in subreddit.New.Take(25))
            {
                if (!state.DoesCommentExist(post.Id) && !state.HasPostBeenChecked(post.Id))
                {
                    List<ArchiveLink> ArchivedLinks = new List<ArchiveLink>();
                    List<string> Links = new List<string>();
                    if (post.IsSelfPost && !string.IsNullOrEmpty(post.SelfTextHtml))
                    {
                        Links = RegularExpressions.FindLinks(post.SelfTextHtml);
                    }
                    if (Links.Count == 0 && !subreddit.ArchivePost)
                    {
                        continue;
                    }
                    if (Links.Count > 0)
                    {
                        foreach (string s in Links)
                        {
                            Console.WriteLine($"Found {s} in post {post.Id}");
                        }
                    }
                    ArchivedLinks = ArchiveLinks.ArchivePostLinks(Links, new Regex[] { exclusions, providers, ImageRegex }, post.Author);
                    lock (LockConfigObject)
                    {
                        PostArchives.ArchivePostLinks(subreddit, config, state, post, ArchivedLinks);
                    }
                    state.AddCheckedPost(post.Id);
                    Console.WriteLine("Added post: " + post.Id);
                    if (!subreddit.ArchiveAfter24Hours)
                    {
                        Console.WriteLine("Checked post: " + post.Id);
                        state.Archive24Hours(post.Id);
                    }
                }
            }
        }
        public static void IterateComments(Reddit reddit, IBotState state, ArchiveSubreddit subreddit, Config config)
        {
            if (reddit == null || state == null || subreddit == null || config == null)
            {
                throw new ArgumentNullException(reddit == null ? nameof(reddit) : state == null ? nameof(state) : config == null ? nameof(config) : nameof(subreddit));
            }
            if (!subreddit.ArchiveCommentLinks)
            {
                return;
            }
            Console.Title = $"Finding comments in {subreddit.Name} New messages: {reddit.User.UnreadMessages.Count() >= 1}";
            foreach (var comment in subreddit.Comments.Take(25))
            {
                List<string> Links = RegularExpressions.FindLinks(comment.BodyHtml);
                if (state.HasCommentBeenChecked(comment.Id) || ArchiveBots.Contains(comment.AuthorName) || Links.Count == 0)
                {
                    continue;
                }
                foreach (string s in Links)
                {
                    Console.WriteLine($"Found {s} in comment {comment.Id}");
                }
                List<ArchiveLink> ArchivedLinks = ArchiveLinks.ArchivePostLinks(Links, new[] { exclusions, providers, ImageRegex }, reddit.GetUser(comment.AuthorName));
                lock (LockConfigObject)
                {
                    PostArchives.ArchiveCommentLinks(config, state, reddit, comment, ArchivedLinks);
                }
                state.AddCheckedComment(comment.Id);
            }
        }
        public static void Iterate24HourArchive(Reddit reddit, IBotState state, ArchiveSubreddit[] subreddits, Config config)
        {
            if (reddit == null || state == null || subreddits == null || config == null)
            {
                throw new ArgumentNullException(reddit == null ? nameof(reddit) : state == null ? nameof(state) : config == null ? nameof(config) : nameof(subreddits));
            }
            Console.Title = $"Archiving posts after 24 hours";
            // Shut the fuck up about the name, I know it's stupid long, but it exists for literally only this, so can it
            var compararer = new ArchiveSubredditEqualityCompararer();
            foreach (var postId in state.GetNon24HourArchivedPosts())
            {
                List<ArchiveLink> ArchivedLinks = new List<ArchiveLink>();
                List<string> Links = new List<string>();
                Post post = (Post)reddit.GetThingByFullname($"t3_{Regex.Replace(postId, "^(t[0-6]_)", "")}");
                if (DateTime.UtcNow.Subtract(new TimeSpan(TimeSpan.TicksPerDay)) < post.Created)
                {
                    continue;
                }
                Console.WriteLine("Got past the 24 hours marker");
                ArchiveSubreddit sub = subreddits.First((a) => a.Name == post.SubredditName);
                if (!sub.ArchiveAfter24Hours)
                {
                    state.Archive24Hours(post.Id);
                    continue;
                }
                if (post.IsSelfPost && !string.IsNullOrEmpty(post.SelfTextHtml))
                {
                    Links = RegularExpressions.FindLinks(post.SelfTextHtml);
                }
                if (Links.Count == 0 && !sub.ArchivePost)
                {
                    state.Archive24Hours(post.Id);
                    continue;
                }
                else if (Links.Count > 0)
                {
                    foreach (string s in Links)
                    {
                        Console.WriteLine($"Found {s} in post {post.Id} when rearchiving after 24 hours");
                    }
                }
#if POSTTEST
                ArchivedLinks = ArchiveLinks.ArchivePostLinks(Links, new Regex[] { exclusions, providers, ImageRegex }, post.Author);
                lock (LockConfigObject)
                {
                    PostArchives.ArchivePostLinks24Hours(sub, reddit, config, state, post, ArchivedLinks);
                }
#endif
                state.Archive24Hours(post.Id);
            }
        }
        #endregion
    }
}
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
        public static string[] ArchiveBots = new string[]
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
            "GoodBot_BadBot_Karma"
        };
        /// <summary>
        /// Iterates each "thing" you make, subreddit is required for a few of them
        /// </summary>
        /// <param name="reddit"></param>
        /// <param name="state"></param>
        /// <param name="subbreddit"></param>
        public delegate void IterateThing(Reddit reddit, IBotState state, ArchiveSubreddit subbreddit);
        public static IterateThing IteratePost;
        public static IterateThing IterateComment;
        public static IterateThing IterateMessage;
        /// <summary>
        /// This is intentional to be this way, it's so that the editor can get the headers easily
        /// </summary>
        public static readonly string[] Headers = new string[] { "Archives for this post:\n\n", "Archive for this post:\n\n", "Archives for the links in comments:\n\n", "----\nI am Mnemosyne 2.1, {0} ^^^^/r/botsrights ^^^^[Contribute](https://github.com/Mnemosyne-20/Mnemosyne-2.1) ^^^^message ^^^^me ^^^^suggestions ^^^^at ^^^^any ^^^^time ^^^^Opt ^^^^out ^^^^of ^^^^tracking ^^^^by ^^^^messaging ^^^^me ^^^^\"Opt ^^^^Out\" ^^^^at ^^^^any ^^^^time" };
        /// <summary>
        /// These three being separate is important because it is used for data tracking
        /// </summary>
        public static Regex exclusions = new Regex(@"(facebook\.com|giphy\.com|youtube\.com|streamable\.com|www\.gobrickindustry\.us|gyazo\.com|sli\.mg|imgur\.com|reddit\.com/message|youtube\.com|youtu\.be|wiki/rules|politics_feedback_results_and_where_it_goes_from|urbandictionary\.com)");
        public static Regex providers = new Regex(@"(web-beta.archive.org|archive\.is|archive\.fo|web\.archive\.org|archive\.today|megalodon\.jp|web\.archive\.org|webcache\.googleusercontent\.com|archive\.li)");
        public static Regex ImageRegex = new Regex(@"(\.gif|\.jpg|\.png|\.pdf|\.webm|\.mp4)$");
        public static Config Config = !File.Exists("./Data/Settings.json") ? CreateNewConfig() : Config.GetConfig();
        #endregion
        static void Main(string[] args)
        {
            foreach (string s in args)
            {
                switch (s)
                {
                    case "--server":
                    case "-s":
                        break;
                    case "--help":
                        Console.WriteLine("Mnemosyne - 2.1 by chugga_fan");
                        Console.WriteLine("Currently no supported command line options, but future options will be:");
                        Console.WriteLine("\t--server | -s\tWill be used to start a web hosted version, with an ASP.NET host");
                        return;
                    default:
                        break;
                }
            }
            Console.Title = "Mnemosyne-2.1 by chugga_fan";
            Console.Clear();
            IBotState botstate = Config.SQLite ? (IBotState)new SQLiteBotState() : new FlatBotState();
            WebAgent agent = null;
            if (Config.UseOAuth)
            {
                agent = new BotWebAgent(Config.UserName, Config.Password, Config.OAuthClientId, Config.OAuthSecret, Config.RedirectURI);
            }
#pragma warning disable CS0618 // Type or member is obsolete
            Reddit reddit = Config.UseOAuth ? new Reddit(agent) : new Reddit(Config.UserName, Config.Password);
#pragma warning restore CS0618 // Type or member is obsolete
            reddit.InitOrUpdateUser();
            ArchiveSubreddit[] subs = new ArchiveSubreddit[Config.Subreddits.Length];
            for (int i = 0; i < Config.Subreddits.Length; i++)
            {
                subs[i] = reddit.GetArchiveSubreddit(Config.Subreddits[i]);
            }
            IteratePost = IteratePosts;
            IterateComment = IterateComments;
            IterateMessage = IterateMessages;
            new RedditUserProfileSqlite();
#pragma warning disable CS0618 // Type or member is obsolete
            if (File.Exists("./Data/Users.json"))
            {
                RedditUserProfileSqlite.TransferProfilesToSqlite(RedditUserProfile.Users);
                File.Delete("./Data/Users.json");
            }
#pragma warning restore CS0618 // Type or member is obsolete
            IArchiveService service = new ArchiveService().CreateNewService();
            ArchiveLinks.SetArchiveService(service);
            PostArchives.SetArchiveService(service);
            while (true) // main loop, calls delegates that move thrugh every subreddit allowed iteratively
            {
                try
                {
                    foreach (ArchiveSubreddit sub in subs) // Iterates allowed subreddits
                    {
                        IteratePost(reddit, botstate, sub);
                        IterateComment(reddit, botstate, sub);
                        IterateMessage(reddit, botstate, sub);
                    }
                    Console.Title = $"Sleeping, New messages: {reddit.User.UnreadMessages.Count() >= 1}";
                }
                catch (WebException e) when (e.Message.Contains("(404)") || !e.Message.Contains("Cannot resolve hostname") && (int)((HttpWebResponse)e.Response).StatusCode <= 500 && (int)((HttpWebResponse)e.Response).StatusCode >= 600)
                {
                    Console.WriteLine("Connect to the internet, Error: " + e.Message);
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Cannot resolve hostname") || e.Message.Contains("(502)") || e.Message.Contains("(503)") || e.Message.Contains("The remote name could not be resolved")) continue;
                    // Catches errors and documents them, I should switch to a System.Diagnostics logger but I have no experience with it
                    if (!Directory.Exists("./Errors"))
                    {
                        Directory.CreateDirectory("./Errors");
                    }
                    File.AppendAllText("./Errors/Failures.txt", $"{e.ToString()}\n");
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
                Console.WriteLine("Would you like to archive links in comments? (Yes/No)");
                bool ArcComments = Console.ReadLine().ToLower()[0] == 'y';
                ArchiveSubredditJson arcSubJson = new ArchiveSubredditJson()
                {
                    ArchiveCommentLinks = ArcComments,
                    ArchivePost = ArcPost,
                    Name = name
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
                    case "out out":
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
        public static void IteratePosts(Reddit reddit, IBotState state, ArchiveSubreddit subreddit)
        {
            if (reddit == null || state == null || subreddit == null)
            {
                throw new ArgumentNullException(reddit == null ? nameof(reddit) : state == null ? nameof(state) : nameof(subreddit));
            }
            Console.Title = $"Finding posts in {subreddit.Name} New messages: {reddit.User.UnreadMessages.Count() >= 1}";
            foreach (var post in subreddit.New.Take(25))
            {
                if (!state.DoesCommentExist(post.Id) && !state.HasCommentBeenChecked(post.Id) && !state.HasPostBeenChecked(post.Id))
                {
                    Dictionary<string, int> ArchivedLinks = new Dictionary<string, int>();
                    List<string> Links = new List<string>();
                    if (post.IsSelfPost && post.SelfTextHtml != null && post.SelfTextHtml.Length != 0)
                    {
                        Links = RegularExpressions.FindLinks(post.SelfTextHtml);
                    }
                    if (Links.Count == 0 && !subreddit.ArchivePost)
                    {
                        continue;
                    }
                    else if (Links.Count == 0 && subreddit.ArchivePost)
                    {
                        goto ArchivePost;
                    }
                    foreach (string s in Links)
                    {
                        Console.WriteLine($"Found {s} in post {post.Id}");
                    }
                    ArchivedLinks = ArchiveLinks.ArchivePostLinks(ref Links, new Regex[] { exclusions, providers, ImageRegex }, reddit.GetUser(post.AuthorName), false);
                    ArchivePost:;
                    PostArchives.ArchivePostLinks(subreddit, Config, state, post, Links, ArchivedLinks);
                    state.AddCheckedPost(post.Id);
                }
            }
        }
        public static void IterateComments(Reddit reddit, IBotState state, ArchiveSubreddit subreddit)
        {
            if (!subreddit.ArchiveCommentLinks)
            {
                return;
            }
            if (reddit == null || state == null || subreddit == null)
            {
                throw new ArgumentNullException(reddit == null ? nameof(reddit) : state == null ? nameof(state) : nameof(subreddit));
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
                List<string> ArchivedLinks = ArchiveLinks.ArchivePostLinks(ref Links, new Regex[] { exclusions, providers, ImageRegex }, reddit.GetUser(comment.AuthorName));
                PostArchives.ArchiveCommentLinks(Config, state, reddit, comment, ArchivedLinks, Links);
                state.AddCheckedComment(comment.Id);
            }
        }
        #endregion
    }
}
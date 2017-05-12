using ArchiveApi;
using Mnemosyne2Reborn.BotState;
using Mnemosyne2Reborn.Commenting;
using Mnemosyne2Reborn.Configuration;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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
            "deepsalter-001"
        };
        /// <summary>
        /// Iterates each "thing" you make, subreddit is required for a few of them
        /// </summary>
        /// <param name="reddit"></param>
        /// <param name="state"></param>
        /// <param name="subbreddit"></param>
        public delegate void IterateThing(Reddit reddit, IBotState state, Subreddit subbreddit);
        public static IterateThing IteratePost;
        public static IterateThing IterateComment;
        public static IterateThing IterateMessage;
        public static string[] Headers = new string[] { "Archives for links in this post:\n\n", "Archive for this post:\n\n", "Archives for the links in comments:\n\n", "----\nI am Mnemosyne 2.1, {0} ^^^^/r/botsrights ^^^^[Contribute](https://github.com/Mnemosyne-20/Mnemosyne-2.1) ^^^^message ^^^^me ^^^^suggestions ^^^^at ^^^^any ^^^^time ^^^^Opt ^^^^out ^^^^of ^^^^tracking ^^^^by ^^^^messaging ^^^^me ^^^^\"Opt ^^^^Out\" ^^^^at ^^^^any ^^^^time" };
        public static Regex exclusions = new Regex(@"(youtube\.com|streamable\.com|www\.gobrickindustry\.us|gyazo\.com|sli\.mg|imgur\.com|reddit\.com/message|youtube\.com|youtu\.be|wiki/rules|politics_feedback_results_and_where_it_goes_from|urbandictionary\.com)");
        public static Regex providers = new Regex(@"archive\.is|archive\.fo|web\.archive\.org|archive\.today|megalodon\.jp|web\.archive\.org|webcache\.googleusercontent\.com|archive\.li");
        public static Regex ImageRegex = new Regex(@"(\.gif|\.jpg|\.png|\.pdf|\.webm|\.mp4)$");
        public static Config Config = !File.Exists("./Data/Settings.json") ? CreateNewConfig() : Config.GetConfig();
        #endregion
        static void Main()
        {
            Console.Title = "Mnemosyne-2.1 by chugga_fan";
            Console.Clear();
            IBotState botstate = Config.SQLite ? (IBotState)new SQLiteBotState() : new FlatBotState();
            string AccessToken = "";
            WebAgent agent = new WebAgent();
            if (Config.UseOAuth)
            {
                AuthProvider provider = new AuthProvider(Config.OAuthClientId, Config.OAuthSecret, Config.RedirectURI);
                AccessToken = provider.GetOAuthToken(Config.UserName, Config.Password);
                agent = new BotWebAgent(Config.UserName, Config.Password, Config.OAuthClientId, Config.OAuthSecret, Config.RedirectURI);
            }
#pragma warning disable CS0618 // Type or member is obsolete
            Reddit reddit = Config.UseOAuth ? new Reddit(agent) : new Reddit(Config.UserName, Config.Password);
#pragma warning restore CS0618 // Type or member is obsolete
            reddit.InitOrUpdateUser();
            Subreddit[] subs = new Subreddit[Config.Subreddits.Length];
            for (int i = 0; i < Config.Subreddits.Length; i++)
            {
                subs[i] = reddit.GetSubreddit(Config.Subreddits[i]);
            }
            IteratePost = IteratePosts;
            IterateComment = IterateComments;
            IterateMessage = IterateMessages;
            while (true) // main loop, calls delegates that move through every subreddit allowed iteratively
            {
                try
                {
                    foreach (Subreddit sub in subs) // Iterates allowed subreddits
                    {
                        IteratePost(reddit, botstate, sub);
                        IterateComment(reddit, botstate, sub);
                        IterateMessages(reddit, botstate, sub);
                    }
                    Console.Title = $"Sleeping, New messages: {reddit.User.UnreadMessages.Count() >= 1}";
                }
                catch (WebException)
                {
                    Console.WriteLine("Connect to the internet");
                }
                catch (Exception e)
                {
                    // Catches errors and documents them, I should switch to a System.Diagnostics logger but I have no experience with it
                    if (!Directory.Exists("./Errors"))
                    {
                        Directory.CreateDirectory("./Errors");
                    }
                    File.AppendAllText("./Errors/Failures.txt", $"{e.ToString()}\n");
                    Console.WriteLine($"Caught an exception of type {e.GetType()} output is in ./Errors/Failures.txt");
                }
                System.Threading.Thread.Sleep(1000); // sleeps for one second to help with the reddit calls
            }
        }

        public static Config CreateNewConfig()
        {
            Console.WriteLine("Would you like to store data using SQLite instead of JSON files? (Yes/No)");
            bool useSQLite = Console.ReadLine().ToLower()[0] == 'y';
            Console.WriteLine("What is your username?");
            string Username = Console.ReadLine();
            Console.WriteLine("What is your password? note: required and is stored in plaintext, suggest you use a secure system");
            string Password = Console.ReadLine();
            Console.WriteLine("What subreddits do you want to patroll? note: comma separated names without spaces");
            string[] Subs = Console.ReadLine().Split(',');
            // TODO: MAKE THIS AVAILIBLE
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
            System.Threading.Thread.Sleep(10000);
            return new Config(useSQLite, Username, Subs, Password, wantOAuth, ClientSecret, ClientID, ArchiveLinks);
        }
        #region IterateThings
        public static void IterateMessages(Reddit reddit, IBotState state, Subreddit subreddit)
        {
            if (reddit == null || state == null || subreddit == null)
            {
                throw new ArgumentNullException(reddit == null ? "reddit" : state == null ? "state" : "subreddit");
            }
            foreach (var message in reddit.User.PrivateMessages.Take(25))
            {
                if (!message.Unread)
                {
                    break;
                }
                if (message.Body.ToLower().Contains("opt out"))
                {
                    new RedditUserProfile(reddit.GetUser(message.Author), false).OptedOut = true;
                    message.SetAsRead();
                }
            }
        }
        public static void IteratePosts(Reddit reddit, IBotState state, Subreddit subreddit)
        {
            if (reddit == null || state == null || subreddit == null)
            {
                throw new ArgumentNullException(reddit == null ? "reddit" : state == null ? "state" : "subreddit");
            }
            Console.Title = $"Finding posts in {subreddit.Name} New messages: {reddit.User.UnreadMessages.Count() >= 1}";
            foreach (var post in subreddit.Posts.Take(25))
            {
                if (!state.DoesCommentExist(post.Id) && state.HasCommentBeenChecked(post.Id))
                {
                    List<string> Links = RegularExpressions.FindLinks(post.SelfTextHtml);
                    if (Links.Count == 0)
                    {
                        continue;
                    }
                    Dictionary<string, int> ArchivedLinks = ArchiveLinks.ArchivePostLinks(ref Links, new Regex[] { exclusions, providers, ImageRegex }, reddit.GetUser(post.AuthorName), new ArchiveService(Config.ArchiveService), false);
                    PostArchives.ArchivePostLinks(Config, state, post, Links, ArchivedLinks);
                    state.AddCheckedComment(post.Id);
                }
            }
        }
        public static void IterateComments(Reddit reddit, IBotState state, Subreddit subreddit)
        {
            if (reddit == null || state == null || subreddit == null)
            {
                throw new ArgumentNullException(reddit == null ? nameof(reddit) : state == null ? nameof(state) : nameof(subreddit));
            }
            Console.Title = $"Finding comments in {subreddit.Name} New messages: {reddit.User.UnreadMessages.Count() >= 1}";
            foreach (var comment in subreddit.Comments.Take(25))
            {
                if (state.HasCommentBeenChecked(comment.Id) || ArchiveBots.Contains(comment.AuthorName))
                {
                    continue;
                }
                List<string> Links = RegularExpressions.FindLinks(comment.BodyHtml);
                if (Links.Count == 0)
                {
                    continue;
                }
                foreach (string s in Links)
                {
                    Console.WriteLine($"Found {s} in comment {comment.Id}");
                }
                List<string> ArchivedLinks = ArchiveLinks.ArchivePostLinks(ref Links, new Regex[] { exclusions, providers, ImageRegex }, reddit.GetUser(comment.AuthorName), new ArchiveService(Config.ArchiveService));
                PostArchives.ArchiveCommentLinks(Config, state, reddit, comment, ArchivedLinks, Links);
                state.AddCheckedComment(comment.Id);
            }
        }
        #endregion
    }
}

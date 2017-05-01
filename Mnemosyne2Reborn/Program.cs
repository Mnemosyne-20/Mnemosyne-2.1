using ArchiveApi;
using Mnemosyne2Reborn.BotState;
using Mnemosyne2Reborn.Commenting;
using Mnemosyne2Reborn.Configuration;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public delegate void IterateThing(Reddit reddit, IBotState state, Subreddit subbreddit);
        public static IterateThing IteratePost;
        public static IterateThing IterateComment;
        public static IterateThing IterateMessage;
        public static string[] Headers = new string[] { "Archives for links in this post:\n\n", "Archive for this post:\n\n", "Archives for the links in comments:\n\n", "----\nI am Mnemosyne 2.1, {0} ^^^^/r/botsrights ^^^^[Contribute](https://github.com/Mnemosyne-20/Mnemosyne-2.1) ^^^^(message me suggestions at any time) ^^^^(Opt out of tracking by messaging me \"Opt Out\" at any time)" };
        public static Regex exclusions = new Regex(@"(streamable\.com|www\.gobrickindustry\.us|gyazo\.com|sli\.mg|imgur\.com|reddit\.com/message|youtube\.com|youtu\.be|wiki/rules|politics_feedback_results_and_where_it_goes_from|urbandictionary\.com)");
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
            if (Config.UseOAuth)
            {
                AuthProvider provider = new AuthProvider(Config.OAuthClientId, Config.OAuthSecret, "https://www.github.com/Memosyne/Mnemosyne-2.1");
                AccessToken = provider.GetOAuthToken(Config.UserName, Config.Password);
                System.Diagnostics.Process.Start(provider.GetAuthUrl(Config.UserName, AuthProvider.Scope.edit | AuthProvider.Scope.submit | AuthProvider.Scope.read | AuthProvider.Scope.privatemessages));
            }
#pragma warning disable CS0618 // Type or member is obsolete
            Reddit reddit = Config.UseOAuth ? new Reddit(AccessToken) : new Reddit(Config.UserName, Config.Password);
#pragma warning restore CS0618 // Type or member is obsolete
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
                foreach (Subreddit sub in subs) // Iterates allowed subreddits
                {
                    IteratePost(reddit, botstate, sub);
                    IterateComment(reddit, botstate, sub);
                    IterateMessages(reddit, botstate, sub);
                }
                Console.Title = $"Sleeping, New messages: {reddit.User.UnreadMessages.Count() >= 1}";
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
            /*Console.WriteLine("If you do not want to use OAuth, input Y");
            bool wantOAuth = Console.ReadLine().ToLower()[0] == 'y';
            if(!wantOAuth)
            {

            }*/
            Console.WriteLine("Do you want to archive post links? (Yes/No)");
            bool ArchiveLinks = Console.ReadLine().ToLower()[0] == 'y';
            Console.WriteLine("To add flavortext, you must manually add it in as an array in the ./Data/Settings.json file");
            System.Threading.Thread.Sleep(10000);
            return new Config(useSQLite, Username, Subs, Password, false, ArchiveLinks: ArchiveLinks);
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
                    new RedditUserProfile(reddit.GetUser(message.Author), false).SetOptedOut();
                    message.SetAsRead();
                }
            }
        }
        public static void IteratePosts(Reddit reddit, IBotState state, Subreddit subreddit)
        {
            if(reddit == null || state == null || subreddit == null)
            {
                throw new ArgumentNullException(reddit == null ? "reddit" : state == null ?  "state" : "subreddit");
            }
            Console.Title = $"Finding posts in {subreddit.Name} New messages: {reddit.User.UnreadMessages.Count() >= 1}";
            foreach (var post in subreddit.Posts.Take(25))
            {
                if (!state.DoesCommentExist(post.Id) && state.HasCommentBeenChecked(post.Id))
                {
                    List<string> Links = new List<string>();
                    if (post.IsSelfPost)
                    {
                        Links.AddRange(RegularExpressions.FindLinks(post.SelfTextHtml));
                    }
                    List<string> ArchivedLinks = ArchiveLinks.ArchivePostLinks(ref Links, new Regex[] { exclusions, providers, ImageRegex }, reddit.GetUser(post.AuthorName), new ArchiveService(Config.ArchiveService));
                    PostArchives.ArchivePostLinks(Config, state, post, Links, ArchivedLinks);
                    state.AddCheckedComment(post.Id);
                }

            }
        }
        public static void IterateComments(Reddit reddit, IBotState state, Subreddit subreddit)
        {
            if (reddit == null || state == null || subreddit == null)
            {
                throw new ArgumentNullException(reddit == null ? "reddit" : state == null ? "state" : "subreddit");
            }
            Console.Title = $"Finding comments in {subreddit.Name} New messages: {reddit.User.UnreadMessages.Count() >= 1}";
            foreach (var comment in subreddit.Comments.Take(25))
            {
                if (state.HasCommentBeenChecked(comment.Id) || ArchiveBots.Contains(comment.AuthorName))
                {
                    continue;
                }
                List<string> Links = RegularExpressions.FindLinks(comment.BodyHtml);
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

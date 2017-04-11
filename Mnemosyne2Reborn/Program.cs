using System;
using Mnemosyne2Reborn.BotState;
using Mnemosyne2Reborn.Commenting;
using Mnemosyne2Reborn.Configuration;
using RedditSharp;
using RedditSharp.Things;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
namespace Mnemosyne2Reborn
{
    class Program
    {
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
        string[] tempReminder = new string[] { "Watch out for moon rocks!", "My face is tired.", "#FREEKEKISTAN", "THE KEKISTANI PEOPLE MUST BE FREE!" };
        public static string[] Headers = new string[] { "Archives for links in this post:\n\n", "Archive for this post:\n\n", "Archives for the links in comments:\n\n", $"----\nI am Mnemosyne 2.0, {0} ^^^^/r/botsrights ^^^^[Contribute](https://github.com/chuggafan/Mnemosyne-2.1) ^^^^[Website](https://mnemosyne-20.github.io/Mnemosyne-2.1/)" };
        public static Regex exclusions = new Regex(@"(streamable\.com|www\.gobrickindustry\.us|gyazo\.com|sli\.mg|imgur\.com|reddit\.com/message|youtube\.com|youtu\.be|wiki/rules|politics_feedback_results_and_where_it_goes_from|urbandictionary\.com)");
        public static Regex providers = new Regex(@"archive\.is|archive\.fo|web\.archive\.org|archive\.today|megalodon\.jp|web\.archive\.org|webcache\.googleusercontent\.com|archive\.li");
        public static Regex ImageRegex = new Regex(@"(\.gif|\.jpg|\.png|\.pdf|\.webm)$");
        public static Config Config = File.Exists("./Data/Settings.json") ? CreateNewConfig() : Config.GetConfig();
        static void Main(string[] args)
        {
            IBotState botstate = Config.Sqlite ? (IBotState)new SQLiteBotState() : new FlatBotState();
            string AccessToken = "";
            if (!Config.PassOrOAuth)
            {
                AuthProvider provider = new AuthProvider(Config.OAuthClientId, Config.OAuthSecret, "www.github.com/Memosyne/Mnemosyne-2.1");
                AccessToken = provider.GetOAuthToken(Config.Username, Config.Password);
                provider.GetAuthUrl(Config.Username, AuthProvider.Scope.edit | AuthProvider.Scope.submit);
            }
#pragma warning disable CS0618 // Type or member is obsolete
            Reddit reddit = Config.PassOrOAuth ? new Reddit(Config.Username, Config.Password) : new Reddit(AccessToken);
#pragma warning restore CS0618 // Type or member is obsolete
            if (false)
            {
                Subreddit[] subs = new Subreddit[Config.Subreddits.Length];
                for (int i = 0; i < Config.Subreddits.Length; i++)
                {
                    subs[i] = reddit.GetSubreddit(Config.Subreddits[i]);
                }
                foreach (Subreddit sub in subs)
                {
                    IteratePosts(reddit, botstate, sub, false);
                    IterateComments(reddit, botstate, sub);
                }
            }
        }
        public static void IteratePosts(Reddit reddit, IBotState state, Subreddit subreddit, bool ArchivePost)
        {
            foreach (var post in subreddit.Posts.Take(25))
            {
                if (!state.DoesCommentExist(post.Id))
                {
                    List<string> Links = new List<string>();
                    if (Config.ArchiveLinks)
                    {
                        Links.Add(post.Url.ToString());
                    }
                    if (post.IsSelfPost)
                    {
                        Links.AddRange(RegularExpressions.FindLinks(post.SelfTextHtml));
                    }
                    List<string> ArchivedLinks = ArchiveLinks.ArchivePostLinks(Links, exclusions, new RedditUserProfile(new Reddit().GetUser(post.AuthorName), Config.Sqlite));
                    PostArchives.ArchivePostLinks(Config, state, post, Links, ArchivedLinks, ArchivePost, new ArchiveApi.ArchiveService("www.archive.is"));

                }
            }
        }
        public static Config CreateNewConfig()
        {
            Console.WriteLine("Would you like to store data using SQLite instead of JSON files?");
            bool useSQLite = bool.Parse(Console.ReadLine().ToLower());
            Console.WriteLine("What is your username?");
            string Username = Console.ReadLine();
            Console.WriteLine("What is your password? note: required & in plaintext, suggest you use a secure system");
            string Password = Console.ReadLine();
            Console.WriteLine("What subreddits do you want to patroll? note: comma separated names without spaces");
            string[] Subs = Console.ReadLine().Split(',');
            /*Console.WriteLine("If you do not want to use OAuth, input Y");
            bool wantOAuth = Console.ReadLine().ToLower()[0] == 'y';
            if(!wantOAuth)
            {

            }*/
            Console.WriteLine("If you want to archive links, input \"Y\"");
            bool ArchiveLinks = Console.ReadLine().ToLower()[0] == 'y';
            Console.WriteLine("To add flavortext, you must manually add it in as an array in the ./Data/Settings.json file");
            return new Config(useSQLite, Username, Subs, Password, ArchiveLinks: ArchiveLinks);
        }
        public static void IterateComments(Reddit reddit, IBotState state, Subreddit subreddit)
        {
            foreach (var comment in subreddit.Comments.Take(25))
            {
                if (state.HasCommentBeenChecked(comment.Id) || ArchiveBots.Contains(comment.AuthorName))
                {
                    continue;
                }
                List<string> Links = RegularExpressions.FindLinks(comment.BodyHtml);
                List<string> ArchivedLinks = ArchiveLinks.ArchivePostLinks(Links, exclusions, new RedditUserProfile(new Reddit().GetUser(comment.AuthorName), Config.Sqlite));
                PostArchives.ArchiveCommentLinks(Config, state, reddit, comment, ArchivedLinks, Links);
            }
        }
    }
}

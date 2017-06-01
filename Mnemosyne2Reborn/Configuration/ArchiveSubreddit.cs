using Newtonsoft.Json;
using RedditSharp;
using RedditSharp.Things;
namespace Mnemosyne2Reborn.Configuration
{
    public static class Extensions
    {
        /// <summary>
        /// Takes an <seealso cref="ArchiveSubredditJson"/> and makes a <see cref="ArchiveSubreddit"/>
        /// </summary>
        /// <param name="reddit">Reddit used to get the subreddit instance used in ArchiveSubreddit</param>
        /// <param name="json">ArchiveSubredditJson used to pass data to the ArchiveSubreddit</param>
        /// <returns>An initialized instance of ArchiveSubreddit</returns>
        public static ArchiveSubreddit GetArchiveSubreddit(this Reddit reddit, ArchiveSubredditJson json)
        {
            ArchiveSubreddit sub = new ArchiveSubreddit(reddit.GetSubreddit(json.Name))
            {
                ArchivePost = json.ArchivePost,
                ArchiveCommentLinks = json.ArchiveCommentLinks
            };
            return sub;
        }
    }
    /// <summary>
    /// Exists to be used in <see cref="Extensions.GetArchiveSubreddit(Reddit, ArchiveSubredditJson)"/> with no other purpose, as it it how the json is taken to convert to an ArchiveSubreddit object
    /// </summary>
    public class ArchiveSubredditJson
    {
        [JsonRequired]
        [JsonProperty("ArchiveCommentLinks")]
        public bool ArchiveCommentLinks { get; set; }
        [JsonRequired]
        [JsonProperty("ArchivePostLink")]
        public bool ArchivePost { get; set; }
        [JsonRequired]
        [JsonProperty("SubredditName")]
        public string Name { get; set; }
    }
    /// <summary>
    /// This class creates a wrapper for a subreddit so that it has two properties to be pass around readily
    /// </summary>
    public class ArchiveSubreddit
    {
        public readonly Subreddit sub;
        public ArchiveSubreddit(Subreddit sub)
        {
            this.sub = sub;
        }
        public Listing<Post> Posts => sub.Posts;
        public Listing<Comment> Comments => sub.Comments;
        public string Name => sub.Name;
        public bool ArchiveCommentLinks { get; set; }
        public bool ArchivePost { get; set; }
    }
}

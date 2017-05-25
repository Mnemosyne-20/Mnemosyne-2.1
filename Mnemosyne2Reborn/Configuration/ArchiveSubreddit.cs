using Newtonsoft.Json;
using RedditSharp;
using RedditSharp.Things;
namespace Mnemosyne2Reborn.Configuration
{
    public static class Extensions
    {
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
    public class ArchiveSubredditJson
    {
        [JsonProperty("ArchiveCommentLinks")]
        public bool ArchiveCommentLinks { get; set; }
        [JsonProperty("ArchivePostLink")]
        public bool ArchivePost { get; set; }
        [JsonProperty("SubredditName")]
        public string Name { get; set; }
    }
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

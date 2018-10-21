using ArchiveApi;
using ArchiveApi.Interfaces;
using Newtonsoft.Json;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
namespace Mnemosyne2Reborn.Configuration
{
    public class ArchiveSubredditEqualityCompararer : IEqualityComparer<ArchiveSubreddit>
    {
        public bool Equals(ArchiveSubreddit x, ArchiveSubreddit y) => this.GetHashCode(x) == this.GetHashCode(y);

        public int GetHashCode(ArchiveSubreddit obj)
        {
            return obj.sub.GetHashCode();
        }
    }
    public class ArchiveSubredditEventArgs : EventArgs
    {
        public ArchiveSubredditEventArgs(ArchiveSubreddit[] archiveSubreddits)
        {
            ArchiveSubreddits = archiveSubreddits;
        }
        public ArchiveSubreddit[] ArchiveSubreddits { get; }
    }
    public static class ArchiveSubredditExtensions
    {
        /// <summary>
        /// Takes an <seealso cref="ArchiveSubredditJson"/> and makes a <seealso cref="ArchiveSubreddit"/>
        /// </summary>
        /// <param name="reddit">Reddit used to get the subreddit instance used in ArchiveSubreddit</param>
        /// <param name="json">ArchiveSubredditJson used to pass data to the ArchiveSubreddit</param>
        /// <returns>An initialized instance of ArchiveSubreddit</returns>
        public static ArchiveSubreddit GetArchiveSubreddit(this Reddit reddit, ArchiveSubredditJson json) => new ArchiveSubreddit(reddit, json);
    }
    /// <summary>
    /// Exists to be used in <see cref="Extensions.GetArchiveSubreddit(Reddit, ArchiveSubredditJson)"/> with no other purpose, as it it how the json is taken to convert to an ArchiveSubreddit object
    /// </summary>
    [Serializable]
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
        [JsonRequired]
        [JsonProperty("ArchiveWebsite")]
        public string ArchiveWebsite { get; set; }
        [JsonRequired]
        [JsonProperty("ArchiveAfter24Hours")]
        public bool ArchiveAfter24Hours { get; set; }
    }
    /// <summary>
    /// This class creates a wrapper for a subreddit so that it has two properties to be pass around readily
    /// </summary>
    public class ArchiveSubreddit
    {
        public readonly Subreddit sub;
        public ArchiveSubreddit(Reddit reddit, ArchiveSubredditJson json) : this(reddit.GetSubreddit(json.Name))
        {
            SubredditArchiveService = new ArchiveService(json.ArchiveWebsite).CreateNewService();
            ArchivePost = json.ArchivePost;
            ArchiveCommentLinks = json.ArchiveCommentLinks;
            ArchiveAfter24Hours = json.ArchiveAfter24Hours;
        }
        public ArchiveSubreddit(Subreddit sub) => this.sub = sub;
        public Listing<Post> New { get => sub.New; }
        public Listing<Post> Posts { get => sub.Posts; }
        public Listing<Comment> Comments => sub.Comments;
        public string Name => sub.Name;
        public bool ArchiveCommentLinks { get; set; }
        public bool ArchivePost { get; set; }
        public bool ArchiveAfter24Hours { get; set; }
        public IArchiveService SubredditArchiveService { get; set; }
    }
}
using ArchiveApi.Interfaces;
using Mnemosyne2Reborn.UserData;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace Mnemosyne2Reborn.Commenting
{
    /// <summary>
    /// This class is used for archiving links in comments, not used for posts
    /// </summary>
    public class UserLinks
    {
        /// <summary>
        /// Used for determining what the formatted links look like
        /// </summary>
        public enum UserLinkType
        {
            /// <summary>
            /// Denotes that the passed through value is a "post"
            /// </summary>
            Post,
            /// <summary>
            /// Denotes that the passed through value is a "comment"
            /// </summary>
            Comment
        }
        private Thing Thing;
        public UserLinkType UserLinksType { get; private set; }
        /// <summary>
        /// Username of the reddit user, used to build the archive listing comments later
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Location url of the post/comment
        /// </summary>
        public Uri Location { get; set; }
        /// <summary>
        /// This contains a list of original links, archived links, and their positions in the system
        /// </summary>
        public List<ArchiveLink> ArchiveLinks { get; private set; }
        public static IArchiveService service;
        public UserLinks(string Location, Reddit reddit) : this(new Uri(Location), reddit)
        { }
        public UserLinks(Uri Location, Reddit reddit)
        {
            Comment c = reddit.GetComment(Location);
            this.Thing = reddit.GetThingByFullname(c.FullName);
            this.UserLinksType = this.Thing.Kind.Contains("t1") ? UserLinkType.Post : UserLinkType.Comment;
            this.Name = this.UserLinksType == UserLinkType.Post ? ((Post)Thing).AuthorName : ((Comment)Thing).AuthorName;
            this.Location = Location;
        }
        public UserLinks(string Name, Uri Location, Reddit reddit) : this(Location, reddit) => this.Name = Name;
        public UserLinks(string Name, string Location, Reddit reddit) : this(Name, new Uri(Location), reddit) { }
        /// <summary>
        /// Initalizes the UserLinks class
        /// </summary>
        /// <param name="Name">A name for the user</param>
        /// <param name="Location">The location (Uri) of the post/comment</param>
        public UserLinks(string Name, string Location, Reddit reddit, UserLinkType type) : this(Name, new Uri(Location), reddit, type)
        {
        }
        /// <summary>
        /// Initalizes the UserLinks class
        /// </summary>
        /// <param name="Name">A name for the user</param>
        /// <param name="Location">The location (Uri) of the post/comment</param>
        public UserLinks(string Name, Uri Location, Reddit reddit, UserLinkType type) => this.UserLinksType = type;
        /// <summary>
        /// Makes a Userlinks class from a comment, a list of regexes, and a Reddit
        /// </summary>
        /// <param name="comment">A <see cref="Comment"/> used to get the links and several other things</param>
        /// <param name="regexes">A list of <see cref="Regex"/> used for excluding links</param>
        /// <param name="reddit">A <see cref="Reddit"/> literally only used for getting a RedditUser</param>
        public UserLinks(Comment comment, Regex[] regexes, Reddit reddit) : this(comment.AuthorName, comment.Shortlink, reddit, UserLinkType.Comment) => ArchiveLinks = Mnemosyne2Reborn.ArchiveLinks.ArchivePostLinks2(RegularExpressions.FindLinks(comment.BodyHtml), regexes, reddit.GetUser(comment.AuthorName));
        /// <summary>
        /// Initializes the UserLinks class with Post items determining nessecary things
        /// </summary>
        /// <param name="post">A <see cref="Post"/> of which you give in</param>
        /// <param name="regexes">A list of <see cref="Regex"/>es that you use for excluding links</param>
        public UserLinks(Post post, Regex[] regexes, Reddit reddit) : this(post.AuthorName, post.Permalink, reddit, UserLinkType.Post) => ArchiveLinks = Mnemosyne2Reborn.ArchiveLinks.ArchivePostLinks2(RegularExpressions.FindLinks(post.SelfTextHtml), regexes, post.Author);
        /// <summary>
        /// Add an <seealso cref="ArchiveLink"/> to <see cref="ArchiveLinks"/>
        /// </summary>
        /// <param name="link">An <seealso cref="ArchiveLink"/> to add</param>
        public void AddLinks(ArchiveLink link)
        {
            ArchiveLinks.Add(link);
            ArchiveLinks.Sort();
        }
        /// <summary>
        /// Removes all links that match the given <seealso cref="Regex"/>
        /// </summary>
        /// <param name="r">A <seealso cref="Regex"/> to filter by</param>
        public void RemoveOnRegex(Regex r) => RemoveOnRegex(new[] { r });
        /// <summary>
        /// A list of <see cref="Regex"/> to filter with
        /// </summary>
        /// <param name="r">A list of <see cref="Regex"/> that you use to filter with</param>
        public void RemoveOnRegex(Regex[] r)
        {
            var stuff = from a in r.AsParallel() from b in ArchiveLinks where !a.IsMatch(b.OriginalLink) select b;
            ArchiveLinks = stuff.ToList();
            ArchiveLinks.Sort();
        }
        /// <summary>
        /// Adds all original links to a <see cref="RedditUserProfileSqlite"/>
        /// </summary>
        /// <param name="r">A <see cref="Reddit"/> used for getting user information, cheifly the name of a user</param>
        public void AddToProfile(Reddit r)
        {
            var profile = new RedditUserProfileSqlite(r.GetUser(Name));
            foreach (var a in ArchiveLinks)
            {
                profile.AddUrlUsed(a.OriginalLink);
            }
        }
        public string[] GetFormatedLinks()
        {
            throw new NotImplementedException();
#pragma warning disable
            string[] temp = new string[ArchiveLinks.Count];
            if (UserLinksType == UserLinkType.Post)
            {

            }
#pragma warning restore
        }
    }
}
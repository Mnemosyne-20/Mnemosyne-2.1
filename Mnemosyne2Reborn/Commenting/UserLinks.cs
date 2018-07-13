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
        private readonly Thing Thing;
        public UserLinkType UserLinksType { get; private set; }
        /// <summary>
        /// Username of the reddit user, used to build the archive listing comments later
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// This contains a list of original links, archived links, and their positions in the system
        /// </summary>
        public List<ArchiveLink> ArchiveLinks { get; private set; }
        public static IArchiveService Service { get; private set; }
        /// <summary>
        /// Makes a Userlinks class from a comment, a list of regexes, and a Reddit
        /// </summary>
        /// <param name="comment">A <see cref="Comment"/> used to get the links and several other things</param>
        /// <param name="regexes">A list of <see cref="Regex"/> used for excluding links</param>
        /// <param name="reddit">A <see cref="Reddit"/> literally only used for getting a RedditUser</param>
        public UserLinks(Comment comment, Regex[] regexes, Reddit reddit)
        {
            this.Thing = comment;
            Name = comment.AuthorName;
            UserLinksType = UserLinkType.Comment;
            ArchiveLinks = Mnemosyne2Reborn.ArchiveLinks.ArchivePostLinks(RegularExpressions.FindLinks(comment.BodyHtml), regexes, reddit.GetUser(comment.AuthorName));
        }
        /// <summary>
        /// Initializes the UserLinks class with Post items determining nessecary things
        /// </summary>
        /// <param name="post">A <see cref="Post"/> of which you give in</param>
        /// <param name="regexes">A list of <see cref="Regex"/>es that you use for excluding links</param>
        /// <param name="reddit">A <see cref="Reddit"/> used for parameter parity with <see cref="UserLinks(Comment, Regex[], Reddit)"/></param>
        public UserLinks(Post post, Regex[] regexes, Reddit reddit)
        {
            this.Thing = post;
            this.UserLinksType = UserLinkType.Post;
            Name = post.AuthorName;
            ArchiveLinks = Mnemosyne2Reborn.ArchiveLinks.ArchivePostLinks(RegularExpressions.FindLinks(post.SelfTextHtml), regexes, post.Author);
        }
        /// <summary>
        /// Sets the internal <see cref="IArchiveService"/>
        /// </summary>
        /// <param name="service">An <see cref="IArchiveService"/> to use</param>
        public static void SetArchiveService(IArchiveService service) => Service = service;
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
        public string[] GetFormatedLinks(Configuration.ArchiveSubreddit sub)
        {
            throw new NotImplementedException();
#pragma warning disable
            string[] temp = new string[ArchiveLinks.Count + (sub.ArchivePost ? 1 : 0)];
            if (UserLinksType == UserLinkType.Post)
            {
                if (sub.ArchivePost)
                {
                    temp[0] = $"* **Post:** {sub.SubredditArchiveService.Save(((Post)Thing).Url)}\n";
                }
                for (int i = (sub.ArchivePost ? 1 : 0); i < temp.Length; i++)
                {
                    ArchiveLink link = ArchiveLinks[i];
                    if (ArchiveLinks.Count != 0)
                    {
                        if (link.IsExcluded)
                            continue;
                        temp[i] = $"* **Link: {link.Position}** ([{link.Hostname}]({link.OriginalLink})): {link.ArchivedLink}\n";
                    }
                }
            }
            else
            {
                for (int i = 0; i < temp.Length; i++)
                {
                    ArchiveLink link = ArchiveLinks[i];
                    if (ArchiveLinks.Count != 0)
                    {
                        if (link.IsExcluded)
                            continue;

                    }
                }
            }
            return temp;
#pragma warning restore
        }
    }
}
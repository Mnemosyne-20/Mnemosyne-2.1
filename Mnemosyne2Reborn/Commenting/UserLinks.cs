using ArchiveApi.Interfaces;
using Mnemosyne2Reborn.UserData;
using RedditSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RedditSharp.Things;
namespace Mnemosyne2Reborn.Commenting
{
    /// <summary>
    /// This class is used for archiving links in comments, not used for posts
    /// </summary>
    public class UserLinks
    {
        /// <summary>
        /// Username of the reddit user, used to build the archive listing comments later
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Location url of the post/comment
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// This contains a list of original links, archived links, and their positions in the system
        /// </summary>
        public List<ArchiveLink> ArchiveLinks { get; private set; }
        public static IArchiveService service;
        /// <summary>
        /// Initalizes the UserLinks class
        /// </summary>
        /// <param name="Name">A name for the user</param>
        /// <param name="Location">The location (Uri) of the post/comment</param>
        public UserLinks(string Name, string Location)
        {
            this.Name = Name;
            this.Location = Location;
        }
        /// <summary>
        /// Initalizes the UserLinks class
        /// </summary>
        /// <param name="Name">A name for the user</param>
        /// <param name="Location">The location (Uri) of the post/comment</param>
        public UserLinks(string Name, Uri Location) : this(Name, Location.ToString()) { }
        public UserLinks(Comment comment)
        {
            Name = comment.AuthorName;
            Location = comment.Shortlink;
        }
        public UserLinks(Post post)
        {
            Name = post.AuthorName;
            Location = post.Permalink.ToString();
        }
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
        }
    }
}
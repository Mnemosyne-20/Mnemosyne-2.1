using RedditSharp;
using RedditSharp.Things;
using System.Collections.Generic;
namespace Mnemosyne2Reborn.Commenting
{
    public static class Extensions
    {
        /// <summary>
        /// Gets a post the comment replies to
        /// </summary>
        /// <param name="comment">A comment to get the post from</param>
        /// <returns>Post obtained <see cref="Post"/></returns>
        public static Post GetCommentPost(this Comment comment, Reddit reddit) => (Post)reddit.GetThingByFullname(comment.LinkId);
        static string[] types = new string[] { "*", "^", "~~", "[", "]", "_" };
        static string[] replacement = new string[] { "\\*", "\\^", "\\~~", "\\[", "\\]", "\\_" };
        /// <summary>
        /// Removes (Reddit) markup from a string
        /// </summary>
        /// <param name="str">String to remove markup from</param>
        /// <returns>anti-markup backslashes inside of the string</returns>
        public static string DeMarkup(this string str)
        {
            for (int i = 0; i < types.Length; i++)
            {
                str = str.Contains(types[i]) ? str.Replace(types[i], replacement[i]) : str;
            }
            return str;
        }
    }
}
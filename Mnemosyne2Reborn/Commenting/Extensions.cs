using RedditSharp;
using RedditSharp.Things;
namespace Mnemosyne2Reborn.Commenting
{
    public static class Extensions
    {
        /// <summary>
        /// Gets a post the comment replies to
        /// </summary>
        /// <param name="comment"></param>
        /// <returns>Post obtained <see cref="Post"/></returns>
        public static Post GetCommentPost(this Comment comment, Reddit reddit) => (Post)reddit.GetThingByFullname(comment.LinkId);
        static string[] types = new string[] { "*", "^", "~~", "[", "]", "_" };
        static string[] replacement = new string[] { "\\*", "\\^", "\\~~", "\\[", "\\]", "\\_" };
        /// <summary>
        /// Removes markup names
        /// </summary>
        /// <param name="str">String to remove markup from</param>
        /// <returns>anti-markup backslashes</returns>
        public static string DeMarkup(this string str)
        {
            for (int i = 0; i < types.Length; i++)
            {
                if (str.Contains(types[i]))
                {
                    str = str.Replace(types[i], replacement[i]);
                }
            }
            return str;
        }
    }
}

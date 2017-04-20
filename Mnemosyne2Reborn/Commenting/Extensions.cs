using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedditSharp.Things;
using RedditSharp;
namespace Mnemosyne2Reborn.Commenting
{
    public static class Extensions
    {
        /// <summary>
        /// Gets a post the comment replies to
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        public static Post GetCommentPost(this Comment comment)
        {
            return (Post)new Reddit().GetThingByFullname(comment.LinkId);
        }
        static string[] types = new string[] { "*", "^", "~~", "[", "]", "_" };
        static string[] replacement = new string[] { "\\*", "\\^", "\\~~", "\\[", "\\]", "\\_" };
        /// <summary>
        /// Removes markup names
        /// </summary>
        /// <param name="str">String to remove markup from</param>
        /// <returns>anti-markup backslashes</returns>
        internal static string DeMarkup(this string str)
        {
            for (int i = 0; i < types.Length; i++)
            {
                if (str.Contains(types[i]))
                {
                    str.Replace(types[i], replacement[i]);
                }
            }
            return str;
        }
    }
}

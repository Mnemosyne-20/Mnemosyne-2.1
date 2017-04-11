using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
namespace Mnemosyne2Reborn
{
    public static class RegularExpressions
    {

        /// <summary>
        /// This finds links in a post/comment
        /// </summary>
        /// <param name="PostBody">This is the post body</param>
        /// <returns>the list of URLS we're archiving</returns>
        public static List<string> FindLinks(string PostBody)
        {
            List<string> LinksList = new List<string>();
            var match = Regex.Match(PostBody, @"""(http|ftp|https)://([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?"""); // Same voodoo black demon magic from last repo
            while (match.Success)
            {
                string foundlink = match.Value.TrimStart('"').TrimEnd('"');
                if (!LinksList.Contains(foundlink))
                {
                    LinksList.Add(foundlink);
                }

                match = match.NextMatch();
            }
            return LinksList;
        }
    }
}

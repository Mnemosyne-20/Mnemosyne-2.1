using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
namespace Mnemosyne2Reborn
{
    public static class RegularExpressions
    {

        /// <summary>
        /// This finds links in a post/comment
        /// </summary>
        /// <param name="PostBody">This is the post body</param>
        /// <returns>the list of URLS we're archiving</returns>
        public static List<string> FindLinks(string PostBody) => FindLinksE(PostBody).ToList();
        public static IEnumerable<string> FindLinksE(string PostBody)
        {
            Match match = Regex.Match(PostBody, @"""(http|ftp|https)://([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?"""); // Same voodoo black demon magic from last repo
            while (match.Success)
            {
                yield return match.Value.TrimStart('"').TrimEnd('"');
                match = match.NextMatch(); // iterates it so it yeilds a new result each time
            }
        }
    }
}
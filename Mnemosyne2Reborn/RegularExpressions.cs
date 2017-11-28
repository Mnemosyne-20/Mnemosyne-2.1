using System.Collections.Generic;
using System.Linq;
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
        public static List<string> FindLinks(string PostBody) => FindLinksE(PostBody).ToList();
        /// <summary>
        /// A method to find links inside of HTML
        /// </summary>
        /// <param name="PostBody">A string of HTML characters</param>
        /// <returns>An <see cref="IEnumerable{string}"/> of URLS that were found</returns>
        public static IEnumerable<string> FindLinksE(string PostBody)
        {
            MatchCollection matches = Regex.Matches(PostBody, @"""(http|ftp|https)://([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?"""); // Same voodoo black demon magic from last repo
            foreach (Match match in matches)
            {
                yield return match.Value.TrimStart('"').TrimEnd('"');
            }
        }
    }
}
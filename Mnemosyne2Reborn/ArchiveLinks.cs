using ArchiveApi;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace Mnemosyne2Reborn
{
    public static class ArchiveLinks
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FoundLinks"></param>
        /// <param name="exclusions"></param>
        /// <param name="user"></param>
        /// <returns>A tuple, the first part of the tuple is archived links, second part is the links you put in minus the regexes</returns>
        public static List<string> ArchivePostLinks(ref List<string> FoundLinks, Regex exclusions, RedditSharp.Things.RedditUser user, ArchiveService service)
        {
            List<string> ArchiveLinks = new List<string>();
            bool removed1 = false;
            for (int i = 0; i < FoundLinks.Count; i++)
            {
                if (removed1) // yes I know this code is stupid but it works
                {
                    i--;
                    removed1 = false;
                }
                string link = FoundLinks[i];
                new RedditUserProfile(user, false).AddUrlUsed(link);
                if (!exclusions.IsMatch(link) && !Program.ImageRegex.IsMatch(link) && !Program.providers.IsMatch(link))
                {
                    string check = service.Save(link);
                    int retries = 0;
                    while (!service.Verify(link) && retries < 10)
                    {
                        retries++;
                        System.Threading.Thread.Sleep(5000);
                        check = service.Save(link);
                    }
                    ArchiveLinks.Add(check);
                }
                else
                {
                    FoundLinks.RemoveAt(i);
                    if (i != 0)
                    {
                        i--;
                    }
                    else
                    {
                        removed1 = true;
                    }
                }
            }
            return ArchiveLinks;
        }
        /// <summary>
        /// Returns two lists, one contains the links that have been archived, the other the links found that aren't excluded
        /// </summary>
        /// <param name="FoundLinks"></param>
        /// <param name="exclusions">This is an array of regexes that you use to not archive certain types, exists so that I have orginized exlusions</param>
        /// <param name="user"></param>
        /// <returns>A tuple, the first part of the tuple is archived links, second part is the links you put in minus the regexes</returns>
        public static List<string> ArchivePostLinks(ref List<string> FoundLinks, Regex[] exclusions, RedditSharp.Things.RedditUser user, ArchiveService service)
        {
            List<string> ArchiveLinks = new List<string>();
            bool removed1 = false;
            for (int i = 0; i < FoundLinks.Count; i++)
            {
                if (removed1) // yes I know this code is stupid but it works
                {
                    i--;
                    removed1 = false;
                }
                string link = FoundLinks[i];
                new RedditUserProfile(user, false).AddUrlUsed(link);
                if (exclusions.Sum(a => a.IsMatch(link) ? 1 : 0) == 0)
                {
                    string check = service.Save(link);
                    int retries = 0;
                    while (!service.Verify(link) && retries < 10)
                    {
                        retries++;
                        System.Threading.Thread.Sleep(5000);
                        check = service.Save(link);
                    }
                    ArchiveLinks.Add(check);
                }
                else
                {
                    FoundLinks.RemoveAt(i);
                    if (i != 0)
                    {
                        i--;
                    }
                    else
                    {
                        removed1 = true;
                    }
                }
            }
            return ArchiveLinks;
        }
    }
}

using ArchiveApi;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;
namespace Mnemosyne2Reborn
{
    public static class ArchiveLinks
    {
        public static List<string> ArchivePostLinks(ref List<string> FoundLinks, Regex[] exclusions, RedditSharp.Things.RedditUser user, string service) =>
            ArchivePostLinks(ref FoundLinks, exclusions, user, new ArchiveService(service));
        /// <summary>
        /// Returns two lists, one contains the links that have been archived, the other the links found that aren't excluded
        /// </summary>
        /// <param name="FoundLinks"></param>
        /// <param name="exclusions">This is an array of regexes that you use to not archive certain types, exists so that I have orginized exlusions</param>
        /// <param name="user"></param>
        /// <returns>A list of archives in order, no tuple anymore!</returns>
        public static List<string> ArchivePostLinks(ref List<string> FoundLinks, Regex[] exclusions, RedditSharp.Things.RedditUser user, ArchiveService service)
        {
            List<string> ArchiveLinks = new List<string>();
            for (int i = 0; i < FoundLinks.Count; i++)
            {
                string link = FoundLinks[i];
                new RedditUserProfile(user, false).AddUrlUsed(link);
                if (exclusions.Sum(b => b.IsMatch(link) ? 1 : 0) == 0)
                {
                    string check = service.Save(link).Result;
                    int retries = 0;
                    while (!service.Verify(check) && retries < 10)
                    {
                        retries++;
                        System.Threading.Thread.Sleep(5000);
                        check = service.Save(link).Result;
                    }
                    ArchiveLinks.Add(check);
                }
                else
                {
                    FoundLinks.Remove(link);
                }
            }
            return ArchiveLinks;
        }
        /// <summary>
        /// Returns a dictionary instead of a list, this dictionary has the string + it's order in the post
        /// </summary>
        /// <param name="FoundLinks"></param>
        /// <param name="exclusions"></param>
        /// <param name="user"></param>
        /// <param name="service"></param>
        /// <param name="removeCollisions">Exists to remove name collisions</param>
        /// <returns></returns>
        public static Dictionary<string, int> ArchivePostLinks(ref List<string> FoundLinks, Regex[] exclusions, RedditSharp.Things.RedditUser user, ArchiveService service, bool removeCollisions)
        {
            Dictionary<string, int> ArchiveLinks = new Dictionary<string, int>();
            int counter = 1;
            for (int i = 0; i < FoundLinks.Count; i++)
            {
                string link = FoundLinks[i];
                new RedditUserProfile(user, false).AddUrlUsed(link);
                if (exclusions.Sum(b => b.IsMatch(link) ? 1 : 0) == 0)
                {
                    string check = service.Save(link).Result;
                    int retries = 0;
                    while (!service.Verify(check) && retries < 10)
                    {
                        retries++;
                        System.Threading.Thread.Sleep(5000);
                        check = service.Save(link).Result;
                    }
                    ArchiveLinks.Add(check, counter);
                }
                else
                {
                    FoundLinks.Remove(link);
                }
                counter++;
            }
            return ArchiveLinks;
        }
    }
}

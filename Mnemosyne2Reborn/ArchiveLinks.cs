using ArchiveApi;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;
namespace Mnemosyne2Reborn
{
    public static class ArchiveLinks
    {
        /// <summary>
        /// This exists purely because I wanted to make the unit tests work
        /// </summary>
        /// <param name="FoundLinks"></param>
        /// <param name="exclusions"></param>
        /// <param name="user"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        public static List<string> ArchivePostLinks(ref List<string> FoundLinks, Regex exclusions, RedditSharp.Things.RedditUser user, string service) =>
            ArchivePostLinks(ref FoundLinks, exclusions, user, new ArchiveService(service));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FoundLinks"></param>
        /// <param name="exclusions"></param>
        /// <param name="user"></param>
        /// <returns>A tuple, the first part of the tuple is archived links, second part is the links you put in minus the regexes</returns>
        public static List<string> ArchivePostLinks(ref List<string> FoundLinks, Regex exclusions, RedditSharp.Things.RedditUser user, ArchiveService service)
        {
            Console.WriteLine("New test");
            foreach(var b in FoundLinks)
            {
                Console.WriteLine(b);
            }
            Console.WriteLine(FoundLinks.Count);
            List<string> ArchiveLinks = new List<string>();
            for(int i = 0; i < FoundLinks.Count; i++)
            {
                string link = FoundLinks[i];
                if (!exclusions.IsMatch(link) && !Program.ImageRegex.IsMatch(link) && !Program.providers.IsMatch(link))
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
            Console.WriteLine("Test: ");
            for (int i = 0; i < FoundLinks.Count; i++)
            {
                Console.WriteLine(FoundLinks[i]);
                Console.WriteLine(ArchiveLinks[i]);
            }
            Console.WriteLine(FoundLinks.Count);
            Console.WriteLine(ArchiveLinks.Count);
            return ArchiveLinks;
        }
        public static List<string> ArchivePostLinks(ref List<string> FoundLinks, Regex[] exclusions, RedditSharp.Things.RedditUser user, string service) =>
            ArchivePostLinks(ref FoundLinks, exclusions, user, new ArchiveService(service));
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
            for(int i = 0; i < FoundLinks.Count; i++)
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
            Console.WriteLine("Test: ");
            Console.WriteLine(FoundLinks.Count);
            Console.WriteLine(ArchiveLinks.Count);
            for (int i = 0; i < FoundLinks.Count; i++)
            {
                Console.WriteLine(FoundLinks[i]);
                Console.WriteLine(ArchiveLinks[i]);
            }
            return ArchiveLinks;
        }
    }
}

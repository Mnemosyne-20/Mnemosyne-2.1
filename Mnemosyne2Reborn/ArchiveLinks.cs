using ArchiveApi;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;
namespace Mnemosyne2Reborn
{
    public static class ArchiveLinks
    {
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
            foreach(var a in FoundLinks)
            {
                if (!exclusions.IsMatch(a) && !Program.ImageRegex.IsMatch(a) && !Program.providers.IsMatch(a))
                {
                    string check = service.Save(a);
                    int retries = 0;
                    while (!service.Verify(check) && retries < 10)
                    {
                        retries++;
                        System.Threading.Thread.Sleep(5000);
                        check = service.Save(a);
                    }
                    ArchiveLinks.Add(check);
                }
                else
                {
                    FoundLinks.Remove(a);
                }
            }
            for (int i = 0; i < ArchiveLinks.Count; i++)
            {
                Console.WriteLine(ArchiveLinks[i]);
                Console.WriteLine(FoundLinks[i]);
            }
            Console.WriteLine(ArchiveLinks.Count);
            Console.WriteLine(FoundLinks.Count);
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
            Console.WriteLine("New test");
            foreach (var b in FoundLinks)
            {
                Console.WriteLine(b);
            }
            Console.WriteLine(FoundLinks.Count);
            List<string> ArchiveLinks = new List<string>();
            foreach(var a in FoundLinks)
            {
                new RedditUserProfile(user, false).AddUrlUsed(a);
                if(exclusions.Sum(b => b.IsMatch(a) ? 1 : 0) == 0)
                {
                    string check = service.Save(a);
                    int retries = 0;
                    while (!service.Verify(check) && retries < 10)
                    {
                        retries++;
                        System.Threading.Thread.Sleep(5000);
                        check = service.Save(a);
                    }
                    ArchiveLinks.Add(check);
                }
                else
                {
                    FoundLinks.Remove(a);
                }
            }
            for(int i = 0; i < ArchiveLinks.Count; i++)
            {
                Console.WriteLine(ArchiveLinks[i]);
                Console.WriteLine(FoundLinks[i]);
            }
            Console.WriteLine(ArchiveLinks.Count);
            Console.WriteLine(FoundLinks.Count);
            return ArchiveLinks;
        }
    }
}

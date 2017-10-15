using ArchiveApi.Interfaces;
using Mnemosyne2Reborn.UserData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace Mnemosyne2Reborn
{
    public static class ArchiveLinks
    {
        static IArchiveService service;
        public static void SetArchiveService(IArchiveServiceFactory factory) => service = factory.CreateNewService();
        public static void SetArchiveService(IArchiveService service) => ArchiveLinks.service = service;
        /// <summary>
        /// Returns two lists, one contains the links that have been archived, the other the links found that aren't excluded
        /// </summary>
        /// <param name="FoundLinks"></param>
        /// <param name="exclusions">This is an array of regexes that you use to not archive certain types, exists so that I have orginized exlusions</param>
        /// <param name="user"></param>
        /// <returns>A list of archives in order, no tuple anymore!</returns>
        public static List<string> ArchivePostLinks(ref List<string> FoundLinks, Regex[] exclusions, RedditSharp.Things.RedditUser user)
        {
            List<string> ArchiveLinks = new List<string>();
            for (int i = 0; i < FoundLinks.Count; i++)
            {
                string link = FoundLinks[i];
                new RedditUserProfileSqlite(user).AddUrlUsed(link);
                if (exclusions.Sum(b => b.IsMatch(link) ? 1 : 0) == 0)
                {
                    string check = service.Save(link);
                    int retries = 0;
                    while (!service.Verify(check) && retries < 10) // rechecks in case archive.is failed
                    {
                        retries++;
                        System.Threading.Thread.Sleep(5000); // waits so we don't spam archive.is
                        check = service.Save(link);
                    }
                    ArchiveLinks.Add(check);
                }
                else
                {
                    FoundLinks.Remove(link); // removes links that are exlcuded
                }
            }
            return ArchiveLinks;
        }
        /// <summary>
        /// THE TUPLE IS IN PLACE OF A REF PARAMETER
        /// </summary>
        /// <param name="FoundLinks"></param>
        /// <param name="exclusions"></param>
        /// <param name="user"></param>
        /// <param name="service"></param>
        /// <param name="removeCollisions">Exists to remove name collisions</param>
        /// <returns></returns>
        public static async Task<Tuple<Dictionary<string, int>, List<string>>> ArchivePostLinksAsync(List<string> FoundLinks, Regex[] exclusions, RedditSharp.Things.RedditUser user)
        {
            Dictionary<string, int> ArchiveLinks = new Dictionary<string, int>();
            int counter = 1;
            for (int i = 0; i < FoundLinks.Count; i++)
            {
                string link = FoundLinks[i];
                new RedditUserProfileSqlite(user).AddUrlUsed(link);
                if (exclusions.Sum(b => b.IsMatch(link) ? 1 : 0) == 0)
                {
                    Task<string> check = service.SaveAsync(link);
                    int retries = 0;
                    while (!service.Verify(await check) && retries < 10)
                    {
                        retries++;
                        System.Threading.Thread.Sleep(5000);
                        check = service.SaveAsync(link);
                    }
                    ArchiveLinks.Add(await check, counter);
                }
                else
                {
                    FoundLinks.Remove(link);
                }
                counter++;
            }
            return Tuple.Create(ArchiveLinks, FoundLinks);
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
        public static Dictionary<string, int> ArchivePostLinks(ref List<string> FoundLinks, Regex[] exclusions, RedditSharp.Things.RedditUser user, bool removeCollisions)
        {
            Dictionary<string, int> ArchiveLinks = null;
            for (int i = 0, counter = 1; i < FoundLinks.Count; i++)
            {
                ArchiveLinks = new Dictionary<string, int>();
                string link = FoundLinks[i];
                new RedditUserProfileSqlite(user).AddUrlUsed(link);
                if (exclusions.Sum(b => b.IsMatch(link) ? 1 : 0) == 0)
                {
                    string check = service.Save(link);
                    int retries = 0;
                    while (!service.Verify(check) && retries < 10)
                    {
                        retries++;
                        System.Threading.Thread.Sleep(5000);
                        check = service.Save(link);
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
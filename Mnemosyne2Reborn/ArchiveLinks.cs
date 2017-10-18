using ArchiveApi.Interfaces;
using Mnemosyne2Reborn.UserData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace Mnemosyne2Reborn
{
    public struct ArchiveLink : IComparable<ArchiveLink>
    {
        public string OriginalLink;
        public string ArchivedLink;
        public int Position;
        public bool IsExcluded;
        public ArchiveLink(string OriginalLink, int Position)
        {
            this.OriginalLink = OriginalLink;
            this.ArchivedLink = null;
            this.Position = Position;
            this.IsExcluded = false;
        }
        public ArchiveLink(string OriginalLink, string ArchivedLink, int Position) : this(OriginalLink, Position)
        {
            this.ArchivedLink = ArchivedLink;
        }

        public int CompareTo(ArchiveLink other) => this.Position.CompareTo(other.Position);

        public void SetArchivedLink(string ArchivedLink)
        {
            this.ArchivedLink = ArchivedLink;
        }
        public void SetExcluded()
        {
            IsExcluded = true;
        }
    }
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
            for (int i = 0; i < FoundLinks.Count; i++)
            {
                string link = FoundLinks[i];
                new RedditUserProfileSqlite(user).AddUrlUsed(link);
                if (exclusions.Sum(a => a.IsMatch(link) ? 1 : 0) != 0)
                {
                    FoundLinks.Remove(link);
                    i--;
                }
            }
            List<string> ArchiveLinks = new List<string>();
            foreach (var link in FoundLinks)
            {
                string check = service.Save(link);
                int retries = 0;
                for (int i = 0; i < 10 && !service.Verify(check); check = service.Save(check))
                {
                    retries++;
                    System.Threading.Thread.Sleep(5000);
                }
                ArchiveLinks.Add(check);
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
            for (int i = 0; i < FoundLinks.Count; i++)
            {
                string link = FoundLinks[i];
                new RedditUserProfileSqlite(user).AddUrlUsed(link);
                if (exclusions.Sum(a => a.IsMatch(link) ? 1 : 0) != 0)
                {
                    FoundLinks.Remove(link);
                    i--;
                }
            }
            ArchiveLinks = new Dictionary<string, int>();
            int i2 = 1;
            foreach (var link in FoundLinks)
            {
                string check = service.Save(link);
                for (int i = 0; i < 10 && !service.Verify(check); check = service.Save(check), i++)
                {
                    System.Threading.Thread.Sleep(5000);
                }
                ArchiveLinks.Add(check, i2 + 1);
                i2++;
            }
            return ArchiveLinks;
        }
        public static List<ArchiveLink> ArchivePostLinks2(List<string> FoundLinks, Regex[] exclusions, RedditSharp.Things.RedditUser user)
        {
            List<ArchiveLink> ArchivedLinks = new List<ArchiveLink>();
            for (int i = 0, i2 = 1; i < FoundLinks.Count && i2 <= FoundLinks.Count; i++, i2++)
            {
                string link = FoundLinks[i];
                ArchivedLinks.Add(new ArchiveLink(link, i2));
                new RedditUserProfileSqlite(user).AddUrlUsed(link);
                if (exclusions.Sum(a => a.IsMatch(link) ? 1 : 0) != 0)
                {
                    ArchiveLink link2 = ArchivedLinks[ArchivedLinks.Count - 1];
                    link2.SetExcluded();
                    ArchivedLinks[ArchivedLinks.Count - 1] = link2;
                    i--;
                }
            }
            ArchivedLinks.Sort();
            for (int i = 0; i < ArchivedLinks.Count; i++)
            {
                if (ArchivedLinks[i].IsExcluded)
                    continue;
                string link = ArchivedLinks[i].OriginalLink;
                string check = service.Save(link);
                for (int i2 = 0; i2 < 10 && !service.Verify(check); check = service.Save(check), i2++)
                {
                    System.Threading.Thread.Sleep(5000);
                }
                ArchivedLinks[i].SetArchivedLink(check);
            }
            return ArchivedLinks;
        }
    }
}
using ArchiveApi.Interfaces;
using Mnemosyne2Reborn.UserData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace Mnemosyne2Reborn
{
#pragma warning disable
    public struct ArchiveLink : IComparable<ArchiveLink>, IEquatable<ArchiveLink>
#pragma warning restore 
    {
        public string OriginalLink;
        public string ArchivedLink;
        public int Position;
        public bool IsExcluded;
        public string HostName => new Uri(OriginalLink).Host.Replace("www.", "");
        public ArchiveLink(string originalLink, int position)
        {
            OriginalLink = originalLink;
            ArchivedLink = null;
            Position = position;
            IsExcluded = false;
        }
        public bool Equals(ArchiveLink other)
        {
            if(other.OriginalLink != this.OriginalLink)
            {
                return false;
            }
            if(other.ArchivedLink != this.ArchivedLink)
            {
                return false;
            }
            return true;
        }
        public static bool operator ==(ArchiveLink a, ArchiveLink b) => a.Equals(b);
        public static bool operator !=(ArchiveLink a, ArchiveLink b) => !a.Equals(b);
        public ArchiveLink(string OriginalLink, string ArchivedLink, int Position) : this(OriginalLink, Position) => this.ArchivedLink = ArchivedLink;
        public int CompareTo(ArchiveLink other) => this.Position.CompareTo(other.Position);
        public void SetArchivedLink(string ArchivedLink) => this.ArchivedLink = ArchivedLink;
        public override bool Equals(object obj) => base.Equals(obj);
    }
    public static class ArchiveLinks
    {
        static IArchiveService service;
        public static void SetArchiveService(IArchiveServiceFactory factory) => service = factory.CreateNewService();
        public static void SetArchiveService(IArchiveService service) => ArchiveLinks.service = service;
        public static List<ArchiveLink> ArchivePostLinks(List<string> FoundLinks, Regex[] exclusions, RedditSharp.Things.RedditUser user)
        {
            List<ArchiveLink> ArchivedLinks = new List<ArchiveLink>();
            for (int i = 0, i2 = 1; i < FoundLinks.Count && i2 <= FoundLinks.Count; i++, i2++)
            {
                string link = FoundLinks[i];
                ArchivedLinks.Add(new ArchiveLink(link, i2));
                new RedditUserProfileSqlite(user).AddUrlUsed(link);
                if (exclusions.Sum(a => (a.IsMatch(link) ? 1 : 0) + (a.IsMatch(new Uri(link).AbsolutePath) ? 1 : 0)) != 0)
                {
                    ArchiveLink link2 = ArchivedLinks[ArchivedLinks.Count - 1];
                    link2.IsExcluded = true;
                    ArchivedLinks[ArchivedLinks.Count - 1] = link2;
                    i--;
                }
            }
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
                ArchiveLink l = ArchivedLinks[i];
                l.SetArchivedLink(check);
                ArchivedLinks[i] = l;
            }
            ArchivedLinks.Sort();
            return ArchivedLinks;
        }
    }
}
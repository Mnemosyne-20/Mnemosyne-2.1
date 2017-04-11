using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using ArchiveApi;
namespace Mnemosyne2Reborn
{
    public static class ArchiveLinks
    {
        public static List<string> ArchivePostLinks(List<string> FoundLinks, Regex exclusions, RedditUserProfile profile)
        {
            List<string> ArchiveLinks = new List<string>();
            for(int i = 0; i < FoundLinks.Count; i++)
            {
                string link = FoundLinks[i];
                ArchiveService service = new ArchiveService("www.archive.is");
                profile.AddUrlUsed(link);
                if (!exclusions.IsMatch(link))
                {
                    ArchiveLinks.Add(service.Save(link));
                }
                else
                {
                    FoundLinks.RemoveAt(i);
                }
            }
            return ArchiveLinks;
        }
    }
}

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
        public static Tuple<List<string>,List<string>> ArchivePostLinks(List<string> FoundLinks, Regex exclusions, RedditSharp.Things.RedditUser user)
        {
            List<string> ArchiveLinks = new List<string>();
            bool removed1 = false;
            for(int i = 0; i < FoundLinks.Count; i++)
            {
                if(removed1)
                {
                    i--;
                    removed1 = false;
                }
                string link = FoundLinks[i];
                ArchiveService service = new ArchiveService("https://www.archive.is");
                new RedditUserProfile(user, false).AddUrlUsed(link);
                if (!exclusions.IsMatch(link) && !Program.ImageRegex.IsMatch(link) && !Program.providers.IsMatch(link))
                {
                    ArchiveLinks.Add(FoundLinks[i]);//ArchiveLinks.Add(service.Save(link));
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
            return Tuple.Create(ArchiveLinks, FoundLinks);
        }
    }
}

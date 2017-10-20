using ArchiveApi.Interfaces;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using RedditSharp;
using RedditSharp.Things;
namespace Mnemosyne2Reborn.Commenting
{
    /// <summary>
    /// This class is used for archiving links in comments, not used for posts
    /// </summary>
    public class UserLinks
    {
        #region Properties
        /// <summary>
        /// Username of the reddit user, used to build the archive listing comments later
        /// </summary>
        public string Name { get; private set; }
        public List<ArchiveLink> ArchiveLinks { get; private set; }
        private IArchiveService service;
        #endregion
        #region Constructors
        public UserLinks(IArchiveService service) => this.service = service;
        public UserLinks(string Name, IArchiveService service) : this(service) => this.Name = Name;
        #endregion
        #region LinkTransformation
        #region AddLinks
        public void AddArchiveLink(ArchiveLink link) => ArchiveLinks.Add(link);
        public void AddArchiveLink(string Original, int Position) => ArchiveLinks.Add(new ArchiveLink(Original, Position));
        #endregion
        #region FilterLinks
        public void FilterLinks(Regex r) => FilterLinks(new Regex[] { r });
        public void FilterLinks(Regex[] r)
        {
            List<ArchiveLink> newList = (from a in r from b in ArchiveLinks where !a.IsMatch(b.OriginalLink) select b).ToList();
            newList.Sort();
            ArchiveLinks = newList;
        }
        #endregion
        #region ArchvieLinks
        public void ArchiveContainedLinks(IArchiveService service = null)
        {
            service = service ?? this.service;
            for (int i = 0; i < ArchiveLinks.Count; i++)
            {
                if(ArchiveLinks[i].ArchivedLink == null)
                {
                    continue;
                }
                ArchiveLinks[i].SetArchivedLink(service.Save(ArchiveLinks[i].OriginalLink));
            }
        }
        #endregion
        #endregion
        #region GetUser
        public RedditUser GetUser(Reddit reddit) => reddit.GetUser(Name);
        #endregion
    }
}
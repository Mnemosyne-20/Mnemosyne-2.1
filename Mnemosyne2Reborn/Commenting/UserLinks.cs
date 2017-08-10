using ArchiveApi;
using System.Collections.Generic;

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
        public string Name { get; set; }
        public List<string> UnarchivedLinks { get; set; }
        public List<string> ArchivedLinks { get; set; }
        public List<int> ArchivedFoundNumber { get; set; }
        public Dictionary<string, int> ArchivedWithPlace
        {
            get
            {
                Dictionary<string, int> temp = new Dictionary<string, int>();
                for (int i = 0; i < ArchivedLinks.Count; i++)
                {
                    temp.Add(ArchivedLinks[i], ArchivedFoundNumber[i]);
                }
                return temp;
            }
            set
            {
                ArchivedLinks = new List<string>();
                ArchivedFoundNumber = new List<int>();
                foreach (var keyval in value)
                {
                    ArchivedLinks.Add(keyval.Key);
                    ArchivedFoundNumber.Add(keyval.Value);
                }
            }
        }
        private ArchiveService service;
        #endregion
        #region Constructors
        public UserLinks(string Name, ArchiveService service)
        {
            this.Name = Name;
            this.service = service;
            UnarchivedLinks = UnarchivedLinks ?? new List<string>();
            ArchivedLinks = ArchivedLinks ?? new List<string>();
            ArchivedFoundNumber = ArchivedFoundNumber ?? new List<int>();
        }
        public UserLinks(string Name, List<string> UnarchivedLinks, ArchiveService service) : this(Name, service)
        {
            this.UnarchivedLinks = UnarchivedLinks;
        }
        public UserLinks(string Name, List<string> UnarchivedLinks, List<string> ArchivedLinks, ArchiveService service) : this(Name, UnarchivedLinks, service)
        {
            this.ArchivedLinks = ArchivedLinks;
        }
        #endregion
        #region Adds
        #region UnArchivedAdds
        public void AddUnarchived(string UnarchivedLink)
        {
            UnarchivedLinks.Add(UnarchivedLink);
        }
        public void AddUnarchived(string[] UnarchivedLinks)
        {
            this.UnarchivedLinks.AddRange(UnarchivedLinks);
        }
        public void AddUnarchived(List<string> UnarchivedLinks)
        {
            this.UnarchivedLinks.AddRange(UnarchivedLinks);
        }
        #endregion
        #region ArchivedAdds
        public void AddArchived(string ArchivedLink)
        {
            ArchivedLinks.Add(ArchivedLink);
            ArchivedFoundNumber.Add(ArchivedFoundNumber.Count);
        }
        public void AddArchived(string[] ArchivedLinks)
        {
            this.ArchivedLinks.AddRange(ArchivedLinks);
            foreach (string s in ArchivedLinks)
            {
                ArchivedFoundNumber.Add(ArchivedFoundNumber.Count);
            }
        }
        public void AddArchived(List<string> ArchivedLinks)
        {
            this.ArchivedLinks.AddRange(ArchivedLinks);
            foreach (string s in ArchivedLinks)
            {
                ArchivedFoundNumber.Add(ArchivedFoundNumber.Count);
            }
        }
        #endregion
        #region DictAdds
        public void AddArchived(Dictionary<string, int> ArchivedIntDict)
        {
            foreach (KeyValuePair<string, int> keyval in ArchivedIntDict)
            {
                ArchivedFoundNumber.Add(keyval.Value);
                ArchivedLinks.Add(keyval.Key);
            }
        }
        public void AddArchived(KeyValuePair<string, int> ArchivedIntKeyVal)
        {
            ArchivedFoundNumber.Add(ArchivedIntKeyVal.Value);
            ArchivedLinks.Add(ArchivedIntKeyVal.Key);
        }
        public void AddArchived(string archive, int appearance)
        {
            ArchivedFoundNumber.Add(appearance);
            ArchivedLinks.Add(archive);
        }
        #endregion
        #endregion
        #region InteractiveMethods
        /// <summary>
        /// Archives links and then makes the ArchivedLinks property be the archived links
        /// </summary>
        /// <returns>ArchivedLinks, just in case you want it</returns>
        public List<string> ArchiveUnarchivedLinks()
        {
            for(int i = 0; i < UnarchivedLinks.Count; i++)
            {
                ArchivedFoundNumber.Add(i);
                ArchivedLinks.Add(service.Save(UnarchivedLinks[i]));
            }
            return ArchivedLinks;
        }
        #endregion 
    }
}
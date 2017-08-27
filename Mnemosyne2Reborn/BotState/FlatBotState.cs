using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
namespace Mnemosyne2Reborn.BotState
{
    public class FlatBotState : IBotState
    {
        /// <summary>
        /// Returns a dictionary the exact same as the new format from the old format
        /// </summary>
        /// <param name="file">File to old format for the original bot</param>
        /// <returns>New dictionary</returns>
        static Dictionary<string, string> ReadReplyTrackingFile(string file)
        {
            Dictionary<string, string> replyDict = new Dictionary<string, string>();
            string fileIn = File.ReadAllText(file);
            string[] elements = fileIn.Split(new char[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < elements.Length; i += 2)
            {
                string postID = elements[i];
                string botCommentID = elements[i + 1];
                replyDict.Add(postID, botCommentID);
            }
            return replyDict;
        }
        /// <summary>
        /// Main constructor, creates all data files used within this class
        /// </summary>
        public FlatBotState()
        {
            if (File.Exists("./Data/ReplyTracker.txt"))
            { // takes the old reply checking file and updates it to the new format
                CommentDictionary = ReadReplyTrackingFile("./Data/ReplyTracker.txt");
                File.Delete("./Data/ReplyTracker.txt");
            }
            else
            { //Dictonary of replies
                if (!File.Exists("./Data/Dictionary.json"))
                {
                    CommentDictionary = new Dictionary<string, string>();
                    File.Create("./Data/Dictionary.json").Dispose();
                }
                else
                {
                    CommentDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("./Data/Dictionary.json")) ?? new Dictionary<string, string>();
                }
            }
            if (!File.Exists("./Data/CheckedComments.json"))
            {
                CheckedComments = new List<string>();
                File.Create("./Data/CheckedComments.json").Dispose();
            }
            else
            {
                CheckedComments = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("./Data/CheckedComments.json")) ?? new List<string>();
            }
            if(!File.Exists("./Data/CheckedPosts.json"))
            {
                CheckedPosts = new List<string>();
                File.Create("./Data/CheckedPosts.json");
            }
            else
            {
                CheckedPosts = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("./Data/CheckedPosts.json")) ?? new List<string>();
            }
        }
        private void DumpDictionary()
        {
            File.WriteAllText("./Data/Dictionary.json", JsonConvert.SerializeObject(CommentDictionary));
            File.WriteAllText("./Data/CheckedComments.json", JsonConvert.SerializeObject(CheckedComments));
            File.WriteAllText("./Data/CheckedPosts.json", JsonConvert.SerializeObject(CheckedPosts));
        }
        [JsonProperty("CheckedComments")]
        List<string> CheckedComments;
        [JsonProperty("CommentDictionary")]
        Dictionary<string, string> CommentDictionary;
        [JsonProperty("CheckedPosts")]
        List<string> CheckedPosts;
        /// <inheritdoc />
        public void AddBotComment(string postID, string commentID)
        {
            CommentDictionary.Add(postID, commentID);
            DumpDictionary();
        }
        /// <inheritdoc />
        public void UpdateBotComment(string postID, string commentID)
        {
            CommentDictionary[postID] = commentID;
            DumpDictionary();
        }
        /// <summary>
        /// Adds a comment to the checked list
        /// </summary>
        /// <param name="commentID">Comment Id to add <see cref="RedditSharp.Things.Thing.Id"/></param>
        public void AddCheckedComment(string commentID)
        {
            CheckedComments.Add(commentID);
            DumpDictionary();
        }
        /// <summary>
        /// COMMENT EXISTING FOR POST
        /// </summary>
        /// <param name="postID"></param>
        /// <returns></returns>
        public bool DoesCommentExist(string postID) => CommentDictionary.ContainsKey(postID);
        /// <summary>
        /// Gets the comment for post
        /// </summary>
        /// <param name="postID">Post that you replied to <seealso cref="RedditSharp.Things.Comment.LinkId"/></param>
        /// <returns>A comment ID <seealso cref="RedditSharp.Things.Thing.Id"/></returns>
        public string GetCommentForPost(string postID) => CommentDictionary[postID];
        /// <summary>
        /// Checks if the comment is checked
        /// </summary>
        /// <param name="CommentID">Comment ID of comment to check</param>
        /// <returns>If the comment exists in the checked comments dictionary</returns>
        public bool HasCommentBeenChecked(string CommentID) => CheckedComments.Contains(CommentID);

        /// <summary>
        /// Adds a post to the checked list
        /// </summary>
        /// <param name="postId">Post ID to add to checked list</param>
        public void AddCheckedPost(string postId)
        {
            CheckedPosts.Add(postId);
            DumpDictionary();
        }
        /// <summary>
        /// Checks if a post has been checked
        /// </summary>
        /// <param name="postId">Post ID to check</param>
        /// <returns>If the post has been checked</returns>
        public bool HasPostBeenChecked(string postId) => CheckedPosts.Contains(postId);
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DumpDictionary();
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~FlatBotState() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

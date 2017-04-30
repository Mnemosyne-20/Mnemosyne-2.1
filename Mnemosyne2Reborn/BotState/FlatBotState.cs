﻿using Newtonsoft.Json;
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
        /// Main constructor, should setup everything on it's own fine
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
        }
        private void DumpDictionary()
        {
            File.WriteAllText("./Data/Dictionary.json", JsonConvert.SerializeObject(CommentDictionary));
            File.WriteAllText("./Data/CheckedComments.json", JsonConvert.SerializeObject(CheckedComments));
        }
        [JsonProperty("CheckedComments")]
        List<string> CheckedComments;
        [JsonProperty("CommentDictionary")]
        Dictionary<string, string> CommentDictionary;
        public void AddBotComment(string postID, string commentID)
        {
            CommentDictionary.Add(postID, commentID);
            DumpDictionary();
        }

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
        /// <param name="postID">Post to check if we commented on it.</param>
        /// <returns>A comment ID</returns>
        public string GetCommentForPost(string postID) => CommentDictionary[postID];
        /// <summary>
        /// Checks if the comment is checked
        /// </summary>
        /// <param name="CommentID">Comment ID of comment to check</param>
        /// <returns></returns>
        public bool HasCommentBeenChecked(string CommentID) => CheckedComments.Contains(CommentID);
    }
}

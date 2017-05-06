using ArchiveApi;
using Mnemosyne2Reborn.BotState;
using Mnemosyne2Reborn.Configuration;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace Mnemosyne2Reborn.Commenting
{
    public static class PostArchives
    {
        static Random rand = new Random();
        public static void ArchivePostLinks(Config conf, IBotState state, Post post, List<string> OriginalLinks, List<string> ArchivedLinks)
        {
            ArchiveService serv = new ArchiveService(conf.ArchiveService);
            List<string> LinksToPost = new List<string>();
            if (conf.ArchiveLinks)
            {
                LinksToPost.Add($"* **Post:** {serv.Save(post.Url)}\n"); // saves post if you want to archive something
            }
            for (int i = 0; i < OriginalLinks.Count; i++)
            {
                string hostname = new Uri(OriginalLinks[i]).Host.Replace("www.", "");
                LinksToPost.Add($"* **Link: {(i + 1).ToString()}** ([{hostname}]({OriginalLinks[i]})): {ArchivedLinks[i]}\n");
            }
            PostArchiveLinks(conf, state, Program.Headers[0], post, LinksToPost);
        }
        public static void ArchiveCommentLinks(Config conf, IBotState state, Reddit reddit, Comment comment, List<string> ArchiveLinks, List<string> OriginalLinks)
        {
            if(ArchiveLinks.Count < 1)
            {
                return;
            }
            List<string> Links = new List<string>();
            string commentID = comment.Id;
            string postID = comment.LinkId.Substring(3);
            for (int i = 0; i < ArchiveLinks.Count; i++)
            {
                string hostname = new Uri(OriginalLinks[i]).Host.Replace("www.", "");
                string commentLink = $"https://www.reddit.com/comments/{postID}/_/{comment.Id}";
                Links.Add($"* **By [{comment.AuthorName.DeMarkup()}]({commentLink})** ([{hostname}]({OriginalLinks[i]})): {ArchiveLinks[i]}\n");
            }
            if (Links.Count >= 1)
            {
                bool HasPostITT = state.DoesCommentExist(postID);
                if (HasPostITT)
                {
                    string botCommentThingID = state.GetCommentForPost(postID);
                    Console.WriteLine($"Already have post in {postID}, getting comment {botCommentThingID.Substring(3)}");
                    EditArchiveComment((Comment)reddit.GetThingByFullname(botCommentThingID), Links);
                }
                else
                {
                    Console.WriteLine($"No comment in {postID} to edit, making new one");
                    PostArchiveLinks(conf, state, Program.Headers[2], (Post)reddit.GetThingByFullname(comment.LinkId), Links);
                }
                state.AddCheckedComment(commentID);
            }
        }
        /// <summary>
        /// Posts the archives as a comment, works great
        /// </summary>
        public static void PostArchiveLinks(Config conf, IBotState state, string head, Post post, List<string> ArchiveList)
        { 
            if(conf == null || state == null || head == null || post == null || ArchiveList == null)
            {
                throw new ArgumentNullException(conf == null ? "conf" : state == null ? "state" : head == null ? "head" : post == null ? "post" : "ArchiveList");
            }
            Console.Title = $"Posting new comment to post {post.Id}";
            string LinksListBody = "";
            foreach (string str in ArchiveList)
            {
                LinksListBody += str;
            }
            string c = head + LinksListBody + "\n" + string.Format(Program.Headers[3], conf.FlavorText[rand.Next(0, conf.FlavorText.Length)]);
            Comment botComment = post.Comment(c);
            try
            {
                state.AddBotComment(post.Id, botComment.Id);
                Console.WriteLine(c);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine($"Caught exception replying to {post.Id} with new comment  {Regex.Replace(botComment.Id, "t1_", "")}: {e.Message}");
                botComment.Del();
            }
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="targetComment">Comment to edit, checks what is current and updates it with new archives</param>
        /// <param name="ArchivedToInsert">The list of items to insert, not the archives themselves, this is usually used internally</param>
        public static void EditArchiveComment(Comment targetComment, List<string> ArchivesToInsert)
        {
            if (ArchivesToInsert.Count > 0)
            {
                Console.Title = $"Editing comment {targetComment.Id}";
                bool bEditGood = false;
                string newCommentText = "";
                string[] oldCommentLines = targetComment.Body.Split("\n".ToArray());
                if (oldCommentLines.Length >= 1)
                {
                    string[] head = oldCommentLines.Take(oldCommentLines.Length - 3).ToArray();
                    string[] tail = oldCommentLines.Skip(oldCommentLines.Length - 3).ToArray();
                    newCommentText += string.Join("\n", head);
                    if (head.Length >= 1)
                    {
                        if (head[head.Length - 1].StartsWith("* **By")) // a comment
                        {
                            foreach (string str in ArchivesToInsert)
                            {
                                newCommentText += "\n" + str;
                            }
                            bEditGood = true;
                        }
                        else if (head[head.Length - 1].StartsWith("* **Link")) // links in a post
                        {
                            newCommentText += "\n\n----\nArchives for links in comments: \n\n";
                            foreach (string str in ArchivesToInsert)
                            {
                                newCommentText += str;
                            }
                            bEditGood = true;
                        }
                        else if (head[head.Length - 1].StartsWith("* **Post")) // POST
                        {
                            newCommentText += "\n\n----\nArchives for links in comments: \n\n";
                            foreach (string str in ArchivesToInsert)
                            {
                                newCommentText += str;
                            }
                            bEditGood = true;
                        }
                        else
                        {
                            throw new Exception($"Unexpected end of head: {head[head.Length - 1]}"); // more appropriate, as that's not supposed to happen
                        }
                        newCommentText += string.Join("\n", tail);
                    }
                }
                if (bEditGood)
                {
                    targetComment.EditText(newCommentText);
                }
            }
        }
    }
}

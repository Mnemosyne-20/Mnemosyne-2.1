using ArchiveApi.Services;
using Mnemosyne2Reborn.BotState;
using Mnemosyne2Reborn.Configuration;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ArchiveApi.Interfaces;
namespace Mnemosyne2Reborn.Commenting
{
    public static class PostArchives
    {
        #region Static values and setters
        static IArchiveService service;
        public static void SetArchiveServiceFromFactory(IArchiveServiceFactory factory)
        {
            service = factory.GetArchiveService();
        }
        public static void SetArchiveService(IArchiveService service)
        {
            PostArchives.service = service;
        }
        static Random rand = new Random();
        #endregion
        /// <summary>
        /// Archives post links
        /// </summary>
        /// <param name="conf">Any configuration file</param>
        /// <param name="state">An IBotstate <see cref="IBotState"/></param>
        /// <param name="post">A reddit post to reply to <see cref="Post"/></param>
        /// <param name="OriginalLinks">A list of the original links</param>
        /// <param name="ArchivedLinks">A list of pre-archived links that gets formatted for posting</param>
        public static void ArchivePostLinks(Config conf, IBotState state, Post post, int[] LinkNumber, List<string> OriginalLinks, List<string> ArchivedLinks)
        {
            List<string> LinksToPost = new List<string>();
            if (conf.ArchiveLinks)
            {
                LinksToPost.Add($"* **Post:** {service.Save(post.Url)}\n"); // saves post if you want to archive something
            }
            for (int i = 0; i < OriginalLinks.Count; i++)
            {
                string hostname = new Uri(OriginalLinks[i]).Host.Replace("www.", "");
                LinksToPost.Add($"* **Link: {(i + 1).ToString()}** ([{hostname}]({OriginalLinks[i]})): {ArchivedLinks[i]}\n");
            }
            if (LinksToPost.Count == 0)
            {
                return;
            }
            PostArchiveLinks(conf, state, Program.Headers[0], post, LinksToPost);
        }
        /// <summary>
        /// Archives post links
        /// </summary>
        /// <param name="conf">Any configuration file</param>
        /// <param name="state">An IBotstate <see cref="IBotState"/></param>
        /// <param name="post">A reddit post to reply to <see cref="Post"/></param>
        /// <param name="OriginalLinks">A list of the original links</param>
        /// <param name="ArchivedLinks">A list of pre-archived links that gets formatted for posting</param>
        public static void ArchivePostLinks(ArchiveSubreddit sub, Config conf, IBotState state, Post post, int[] LinkNumber, List<string> OriginalLinks, List<string> ArchivedLinks)
        {
            List<string> LinksToPost = new List<string>();
            if (sub.ArchivePost)
            {
                LinksToPost.Add($"* **Post:** {service.Save(post.Url)}\n"); // saves post if you want to archive something
            }
            for (int i = 0; i < OriginalLinks.Count; i++)
            {
                string hostname = new Uri(OriginalLinks[i]).Host.Replace("www.", "");
                LinksToPost.Add($"* **Link: {(i + 1).ToString()}** ([{hostname}]({OriginalLinks[i]})): {ArchivedLinks[i]}\n");
            }
            if (LinksToPost.Count == 0)
            {
                return;
            }
            PostArchiveLinks(conf, state, Program.Headers[0], post, LinksToPost);
        }
        /// <summary>
        /// Posts links that were gotten from a post
        /// </summary>
        /// <param name="conf">Configuration <see cref="Config"/></param>
        /// <param name="state">Used to track replies</param>
        /// <param name="post">Post to reply to</param>
        /// <param name="OriginalLinks">Original links so that we format this properly</param>
        /// <param name="ArchivedLinks">Dictionary of links and the position found</param>
        public static void ArchivePostLinks(ArchiveSubreddit sub, Config conf, IBotState state, Post post, List<string> OriginalLinks, Dictionary<string, int> ArchivedLinks)
        {
            List<string> LinksToPost = new List<string>();
            if (sub.ArchivePost)
            {
                LinksToPost.Add($"* **Post:** {service.Save(post.Url)}\n"); // saves post if you want to archive something
            }
            int i = 0;
            foreach (var val in ArchivedLinks)
            {
                string hostname = new Uri(OriginalLinks[i]).Host.Replace("www.", "");
                LinksToPost.Add($"* **Link: {val.Value.ToString()}** ([{hostname}]({OriginalLinks[i]})): {val.Key}\n");
                i++;
            }
            if (LinksToPost.Count == 0)
            {
                return;
            }
            PostArchiveLinks(conf, state, Program.Headers[0], post, LinksToPost);
        }
        /// <summary>
        /// Posts links that were gotten from a post
        /// </summary>
        /// <param name="conf">Configuration <see cref="Config"/></param>
        /// <param name="state">Used to track replies</param>
        /// <param name="post">Post to reply to</param>
        /// <param name="OriginalLinks">Original links so that we format this properly</param>
        /// <param name="ArchivedLinks">Dictionary of links and the position found</param>
        public static void ArchivePostLinks(Config conf, IBotState state, Post post, List<string> OriginalLinks, Dictionary<string, int> ArchivedLinks)
        {
            List<string> LinksToPost = new List<string>();
            if (conf.ArchiveLinks)
            {
                LinksToPost.Add($"* **Post:** {service.Save(post.Url)}\n"); // saves post if you want to archive something
            }
            int i = 0;
            foreach (var val in ArchivedLinks)
            {
                string hostname = new Uri(OriginalLinks[i]).Host.Replace("www.", "");
                LinksToPost.Add($"* **Link: {val.Value.ToString()}** ([{hostname}]({OriginalLinks[i]})): {val.Key}\n");
                i++;
            }
            if (LinksToPost.Count == 0)
            {
                return;
            }
            PostArchiveLinks(conf, state, Program.Headers[0], post, LinksToPost);
        }
        /// <summary>
        /// Archives all of the links in a comment
        /// </summary>
        /// <param name="conf">Configuartion <see cref="Config"/></param>
        /// <param name="state">An IBotstate tracker <see cref="IBotState"/></param>
        /// <param name="reddit">A reddit user to post with <see cref="Reddit"/></param>
        /// <param name="comment">A comment to track the post to reply to <see cref="Comment"/></param>
        /// <param name="ArchiveLinks">A list of the archived links</param>
        /// <param name="OriginalLinks">A list of the original links</param>
        public static void ArchiveCommentLinks(Config conf, IBotState state, Reddit reddit, Comment comment, List<string> ArchiveLinks, List<string> OriginalLinks)
        {
            if (ArchiveLinks.Count < 1)
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
            bool HasPostITT = state.DoesCommentExist(postID);
            if (HasPostITT)
            {
                string botCommentThingID = state.GetCommentForPost(postID);
                if (!botCommentThingID.Contains("t1_"))
                {
                    botCommentThingID = "t1_" + botCommentThingID;
                }
                Console.WriteLine($"Already have post in {postID}, getting comment {botCommentThingID.Substring(3)}");
                EditArchiveComment((Comment)reddit.GetThingByFullname(botCommentThingID), Links);
            }
            else
            {
                Console.WriteLine($"No comment in {postID} to edit, making new one");
                PostArchiveLinks(conf, state, Program.Headers[2], comment.GetCommentPost(reddit), Links);
            }
            state.AddCheckedComment(commentID);
        }
        /// <summary>
        /// Archives all of the links in a comment
        /// </summary>
        /// <param name="conf">Configuartion <see cref="Config"/></param>
        /// <param name="state">An IBotstate tracker <see cref="IBotState"/></param>
        /// <param name="reddit">A reddit user to post with <see cref="Reddit"/></param>
        /// <param name="comment">A comment to track the post to reply to <see cref="Comment"/></param>
        /// <param name="ArchiveLinks">A list of the archived links</param>
        /// <param name="OriginalLinks">A list of the original links</param>
        public static async Task ArchiveCommentLinksAsync(Config conf, IBotState state, Reddit reddit, Comment comment, List<string> ArchiveLinks, List<string> OriginalLinks)
        {
            if (ArchiveLinks.Count < 1)
            {
                return;
            }
            string commentID = comment.Id;
            string postID = comment.LinkId.Substring(3);
            Task<List<string>> t = Task.Run(() => // async as this doesn't need t run immediately
            {
                List<string> Links = new List<string>();
                for (int i = 0; i < ArchiveLinks.Count; i++)
                {
                    string hostname = new Uri(OriginalLinks[i]).Host.Replace("www.", "");
                    string commentLink = $"https://www.reddit.com/comments/{postID}/_/{comment.Id}";
                    Links.Add($"* **By [{comment.AuthorName.DeMarkup()}]({commentLink})** ([{hostname}]({OriginalLinks[i]})): {ArchiveLinks[i]}\n");
                }
                return Links;
            });
            bool HasPostITT = state.DoesCommentExist(postID);
            if (HasPostITT)
            {
                string botCommentThingID = state.GetCommentForPost(postID);
                Console.WriteLine($"Already have post in {postID}, getting comment {botCommentThingID.Substring(3)}");
                #region Fixes 10k character limit
                Comment commentToEdit = (Comment)reddit.GetThingByFullname(botCommentThingID);
                List<string> ArchivesToInsert = await t;
                string newCommentText = "";
                string[] oldCommentLines = commentToEdit.Body.Split("\n".ToArray());
                string[] head = oldCommentLines.Take(oldCommentLines.Length - 3).ToArray();
                string[] tail = oldCommentLines.Skip(oldCommentLines.Length - 3).ToArray();
                newCommentText += string.Join("\n", head);
                foreach (string str in ArchivesToInsert)
                {
                    newCommentText += "\n" + str;
                }
                if (newCommentText.Length > 10000)
                {
                    Console.WriteLine($"Too many lines for comment {botCommentThingID} making new comment for post {postID}");
                    PostArchiveLinksToComment(conf, state, Program.Headers[2], commentToEdit, await t);
                    state.AddCheckedComment(commentID);
                    return;
                }
                #endregion
                EditArchiveComment((Comment)reddit.GetThingByFullname(botCommentThingID), ArchivesToInsert);
            }
            else
            {
                Console.WriteLine($"No comment in {postID} to edit, making new one");
                PostArchiveLinks(conf, state, Program.Headers[2], comment.GetCommentPost(reddit), await t);
            }
            state.AddCheckedComment(commentID);
        }
        /// <summary>
        /// Posts all links archived, throws <see cref="ArgumentNullException"/> if you attempt to call this with any null arguments
        /// </summary>
        /// <param name="conf"></param>
        /// <param name="state"></param>
        /// <param name="head"></param>
        /// <param name="post"></param>
        /// <param name="ArchiveList"></param>
        public static void PostArchiveLinksToComment(Config conf, IBotState state, string head, Comment comment, List<string> ArchiveList)
        {
            if (conf == null || state == null || head == null || comment == null || ArchiveList == null)
            {
                throw new ArgumentNullException(conf == null ? "conf" : state == null ? "state" : head == null ? "head" : comment == null ? "post" : "ArchiveList");
            }
            Console.Title = $"Posting new comment to comment {comment.Id}";
            string LinksListBody = "";
            foreach (string str in ArchiveList)
            {
                LinksListBody += str;
            }
            string c = head + LinksListBody + "\n" + string.Format(Program.Headers[3], conf.FlavorText[rand.Next(0, conf.FlavorText.Length)]);
            Comment botComment = comment.Reply(c);
            try
            {
                state.AddBotComment(comment.Id, botComment.Id);
                Console.WriteLine(c);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine($"Caught exception replying to comment {comment.Id} with new comment  {Regex.Replace(botComment.Id, "t1_", "")}: {e.Message}");
                botComment.Del();
            }
        }
        /// <summary>
        /// Posts all links archived, throws <see cref="ArgumentNullException"/> if you attempt to call this with any null arguments
        /// </summary>
        /// <param name="conf"></param>
        /// <param name="state"></param>
        /// <param name="head"></param>
        /// <param name="post"></param>
        /// <param name="ArchiveList"></param>
        public static void PostArchiveLinks(Config conf, IBotState state, string head, Post post, List<string> ArchiveList)
        {
            if (conf == null || state == null || head == null || post == null || ArchiveList == null)
            {
                throw new ArgumentNullException(conf == null ? nameof(conf) : state == null ? nameof(state) : head == null ? nameof(head) : post == null ? nameof(post) : nameof(ArchiveList));
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
                Console.WriteLine($"Caught exception replying to post {post.Id} with new comment  {Regex.Replace(botComment.Id, "t1_", "")}: {e.Message}");
                botComment.Del();
            }
        }
        /// <summary>
        /// Edits a comment containing found archives
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
                            throw new Exception($"Unexpected end of head: {head[head.Length - 1]}, comment: {targetComment.Id}"); // more appropriate, as that's not supposed to happen
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

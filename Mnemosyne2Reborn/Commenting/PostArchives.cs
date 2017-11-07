﻿using ArchiveApi.Interfaces;
using Mnemosyne2Reborn.BotState;
using Mnemosyne2Reborn.Configuration;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace Mnemosyne2Reborn.Commenting
{
    public static class PostArchives
    {
        #region Static values and setters
        static IArchiveService service;
        public static void SetArchiveService(IArchiveServiceFactory factory) => service = factory.CreateNewService();
        public static void SetArchiveService(IArchiveService service) => PostArchives.service = service;
        static Random rand = new Random();
        #endregion
        /// <summary>
        /// Archives post links for a <see cref="ArchiveSubreddit"/>
        /// </summary>
        /// <param name="sub">An <see cref="ArchiveSubreddit"/> used for archiving the post with a specific <see cref="IArchiveService"/></param>
        /// <param name="conf">A <see cref="Config"/> for flavortext</param>
        /// <param name="state">An <see cref="IBotState"/> used to keep track of comments and other things</param>
        /// <param name="post">A <see cref="Post"/> that you are replying to</param>
        /// <param name="archivedLinks">A <see cref="List{T}"/> of <see cref="ArchiveLink"/> used for keeping track of original and archived links</param>
        public static void ArchivePostLinks(ArchiveSubreddit sub, Config conf, IBotState state, Post post, List<ArchiveLink> archivedLinks)
        {
            List<string> LinksToPost = new List<string>();
            if (sub.ArchivePost)
            {
                LinksToPost.Add($"* **Post:** {sub.SubredditArchiveService.Save(post.Url)}\n"); // saves post if you want to archive something
            }
            if (archivedLinks.Count != 0)
            {
                foreach (var link in archivedLinks)
                {
                    if (link.IsExcluded)
                    {
                        continue;
                    }
                    LinksToPost.Add($"* **Link: {link.Position}** ([{link.Hostname}]({link.OriginalLink})): {link.ArchivedLink}\n");
                }
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
                Links.Add($"* **By [{comment.AuthorName.DeMarkup()}]({comment.Shortlink.Replace("oauth", "www")})** ([{hostname}]({OriginalLinks[i]})): {ArchiveLinks[i]}\n");
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
                if (!EditArchiveComment((Comment)reddit.GetThingByFullname(botCommentThingID), Links))
                {
                    PostArchiveLinksToComment(conf, state, Program.Headers[2], comment, Links);
                }
            }
            else
            {
                Console.WriteLine($"No comment in {postID} to edit, making new one");
                PostArchiveLinks(conf, state, Program.Headers[2], comment.GetCommentPost(reddit), Links);
            }
            state.AddCheckedComment(commentID);
        }
        /// <summary>
        /// An asyncronous version of <seealso cref="ArchiveCommentLinks(Config, IBotState, Reddit, Comment, List{string}, List{string})"/> with <see cref="List{ArchiveLink}"/> support instead
        /// </summary>
        /// <param name="config">A <see cref="Config"/> used for flavortext</param>
        /// <param name="state">An <see cref="IBotState"/> used for keeping track of things</param>
        /// <param name="reddit">A <see cref="Reddit"/> used for getting things</param>
        /// <param name="comment">The <see cref="Comment"/> you are archiving</param>
        /// <param name="archiveLinks">A <see cref="List{ArchiveLink}"/> used for archived links</param>
        /// <returns>A <see cref="Task"/> for asyncronous using</returns>
        public static async Task ArchiveCommentLinksAsync(Config config, IBotState state, Reddit reddit, Comment comment, List<ArchiveLink> archiveLinks)
        {
            if (archiveLinks.Count < 1)
            {
                return;
            }
            List<string> Links = new List<string>();
            string commentID = comment.Id;
            string postID = comment.LinkId.Substring(3);
            Task<List<string>> linksTask = Task.Run(() =>
            {
                List<string> links = new List<string>();
                foreach (ArchiveLink link in archiveLinks)
                {
                    if (link.IsExcluded)
                        continue;
                    links.Add($"* **By [{comment.AuthorName.DeMarkup()}]({comment.Shortlink.Replace("oauth", "www")})** ([{link.Hostname}]({link.OriginalLink})): {link.ArchivedLink}\n");
                }
                return links;
            });
            if (state.DoesCommentExist(postID))
            {
                string botCommentThingID = state.GetCommentForPost(postID);
                if (!botCommentThingID.Contains("t1_"))
                {
                    botCommentThingID = "t1_" + botCommentThingID;
                }
                Console.WriteLine($"Already have post in {postID}, getting comment {botCommentThingID.Substring(3)}");
                Links = await linksTask;
                if (!EditArchiveComment((Comment)reddit.GetThingByFullname(botCommentThingID), Links))
                {
                    PostArchiveLinksToComment(config, state, Program.Headers[2], comment, Links);
                }
            }
            else
            {
                Links = await linksTask;
                Console.WriteLine($"No comment in {postID} to edit, making new one");
                PostArchiveLinks(config, state, Program.Headers[2], comment.GetCommentPost(reddit), Links);
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
                    string hostname = new Uri(OriginalLinks[i]).Host.Replace("www.", ""); // comment shortlink is literally the thing I had here
                    Links.Add($"* **By [{comment.AuthorName.DeMarkup()}]({comment.Shortlink.Replace("oauth", "www")})** ([{hostname}]({OriginalLinks[i]})): {ArchiveLinks[i]}\n");
                }
                return Links;
            });
            if (state.DoesCommentExist(postID))
            {
                string botCommentThingID = state.GetCommentForPost(postID);
                if (!botCommentThingID.Contains("t1_"))
                {
                    botCommentThingID = "t1_" + botCommentThingID;
                }
                Console.WriteLine($"Already have post in {postID}, getting comment {botCommentThingID.Substring(3)}");
                List<string> Links = await t;
                if (!EditArchiveComment((Comment)reddit.GetThingByFullname(botCommentThingID), Links))
                {
                    PostArchiveLinksToComment(conf, state, Program.Headers[2], comment, Links);
                }
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
        /// <param name="conf">A <see cref="Config"/> that is used for flavortext and nothing else</param>
        /// <param name="state">An <see cref="IBotState"/> that will update the list with the replies</param>
        /// <param name="head">A header used for a header for the comment</param>
        /// <param name="comment">A <see cref="Comment"/> that you're replying to</param>
        /// <param name="ArchiveList">A <see cref="List{string}"/> for a list of archives to post to a comment</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void PostArchiveLinksToComment(Config conf, IBotState state, string head, Comment comment, List<string> ArchiveList)
        {
            if (conf == null || state == null || head == null || comment == null || ArchiveList == null)
            {
                throw new ArgumentNullException(conf == null ? nameof(conf) : state == null ? nameof(state) : head == null ? nameof(head) : comment == null ? nameof(comment) : nameof(ArchiveList));
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
                state.UpdateBotComment(comment.Id, botComment.Id);
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
        /// Edits a comment containing found archives to add more
        /// </summary>
        /// <param name="targetComment">A <see cref="Comment"/> to edit</param>
        /// <param name="ArchivedToInsert">The <see cref="List{string}"/> of items to insert, not the archives themselves, this is usually used internally</param>
        public static bool EditArchiveComment(Comment targetComment, List<string> ArchivesToInsert)
        {
            bool bEditGood = false;
            if (ArchivesToInsert.Count > 0)
            {
                Console.Title = $"Editing comment {targetComment.Id}";
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
                        else if (head[head.Length - 1].StartsWith("* **Link") || head[head.Length - 1].StartsWith("* **Post")) // links in a post
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
                    if (newCommentText.Length > 10000)
                        bEditGood = false;
                }
                if (bEditGood)
                {
                    targetComment.EditText(newCommentText);
                }
            }
            return bEditGood;
        }
    }
}
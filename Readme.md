[![Build status](https://ci.appveyor.com/api/projects/status/jjt5d1fech57nu09/branch/master?svg=true)](https://ci.appveyor.com/project/chuggafan/mnemosyne-2-1/branch/master)

# FAQ (That I made up)
## What is this?

Well, since you asked, this is a project that is made for archiving links on subreddits.

### Can you be more specific?

Yes! I can infact, what this is is a project that crawls through subreddits and archives any comment (and sometimes post) that has a non-blacklisted link. Currently it runs on three reddit subreddits: KotakuInAction, KiAChatroom, and freemagic, I may add more later.

## Can I add this bot to my subreddit?

Yes, you can also set this bot up yourself for your own subreddit, and you'll get your own bot that doesn't go down nearly as often as mine (might).

## Can I do translation work?

If you want to do translation for me (for whatever reason, idk) you can go ahead, I currently don't have a way to do that since all I know is english and I'm the only one who is using this thing so far, so.

## Can I run this myself?

YES! You can definitely do so, in fact it is designed for you to be able to compile and run on your own. Just don't run it on the same subreddits as I do.

## What is the ArchiveApi library and why does it exist?

Well, it is a library designed specifically to wrap around sites used for archiving, say, web.archive.org (currently unsupported), or the currently supported archive.is (in the flavors of archive.is and archive.fo), add as much functionality as you want, and if you want to help me make it it's own NuGet package, that's fine and great.

## What's all this about SQLite and FlatFiles in your code?

I have two methods of storing data currently: The flatfile system (using separate JSON files to read/write massive amounts of data for storage) and the SQLite system: one (well, two, one for BotState and one for Users) file that stores all the data and makes for neat Queryable systems, fun, right?
No matter what you need SQLite in some way for everything to work, and not having it is much more of a pain than having it (trust me, prefer SQLite based no matter what, it's simpler to query and is much better overall, the original profile system was deprecated because of how much better it is)

## There appears to be data collection, what kind of data is collected?

Well, thanks for noticing that! The data collected is simple, what kinds of links has a username posted, and how many times.

### Can I opt out?

Yes! Just message the bot the text "opt out" (without quotes). It will still have the data previously collected but will mark your username with an "opt-out" marking and then stop tracking it further. Though to be honest the tracking is entirely useless except for people interesting in link-kinds, and the data is so non-granular it's stupid.
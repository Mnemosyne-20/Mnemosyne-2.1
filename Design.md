Design Choices:

[Program.cs](https://github.com/Mnemosyne-20/Mnemosyne-2.1/blob/master/Mnemosyne2Reborn/Program.cs): I use delegates here because I want to later move this into it's own library, as I want to be able to have anyone build off of this code for their own bot, and reuse it in later rewrites so that it is boilerplate code

[RedditUserProfile.cs](https://github.com/Mnemosyne-20/Mnemosyne-2.1/blob/master/Mnemosyne2Reborn/UserData/RedditUserProfile.cs): this works by on starting up, having the users gotten, then this static field dumps it's data every time it does something, works great for my purposes, despite being stupid
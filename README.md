
```
  _____                          _ _           _
 /  ___|                        | | |         | |
 \ `--.  ___  ___ ___  _ __   __| | |__   ___ | |_
  `--. \/ _ \/ __/ _ \| '_ \ / _` | '_ \ / _ \| __|
 /\__/ /  __/ (_| (_) | | | | (_| | |_) | (_) | |_
 \____/ \___|\___\___/|_| |_|\__,_|_.__/ \___/ \__|
```
# Secondbot
Secondbot is a CommandLine based bot for SecondLife based on libremetaverse retargeted for .net core.

### Status
---
[![Docker Cloud Build Status](https://img.shields.io/docker/cloud/build/madpeter/secondbot?style=flat-square)](https://hub.docker.com/r/madpeter/secondbot)

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/1945bad2070d4421adc9c6266dadb237)](https://www.codacy.com/manual/madpeter/SecondBot?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=Madpeterz/SecondBot&amp;utm_campaign=Badge_Grade)

[![CII Best Practices](https://bestpractices.coreinfrastructure.org/projects/3765/badge)](https://bestpractices.coreinfrastructure.org/projects/3765)

#### Helpfull stuff
---
[Commands Wiki](https://wiki.magicmadpeter.xyz/)

## Releases
These are pre compiled versions for the most upto date please use docker or compile it yourself.

[View releases](https://github.com/Madpeterz/SecondBot/releases)
 
 
### Starting multiple bots from the same executable in windows
---
Create a shortcut (rename it for the bot) and 'madpeterbot' after the end of the target

so the target would look something like this: BetterSecondBot.exe" madpeterbot

save changes to the shortcut and your ready to go.


### Custom commands
---
Custom commands allow you to create a chain of commands triggered by one command

Format

    <Custom command name>[!!!]<corelib command 1>[{-}]<corelib command 2>[{-}]...

example command

    sayexample!!!say|||Hello{-}delay|||2500{-}say|||Bye


if your using docker to create a custom command
you would create a new env value that starts with "cmd_" followed by the custom command name
example: `cmd_sayexample`

and the value would be the actions
example: `say|||Hello{-}say|||Bye`

If you want to pass custom args
set `[C_ARG_1]` (this goes up to 5)

and the command would look something like this

    sayexample!!!say|||Hello [C_ARG_1]{-}delay|||2500{-}say|||Bye [C_ARG_2]

### HTTP web interface

by setting

> Http_Enable to true
> Http_Host to http://*:8080
> Security_WebUIKey to a vaild code (12 letters+numbers long or more)

you will be able to connect to the bot via HTTP and use the webUI
to control the bot!

[Dev server UI](http://webui.magicmadpeter.xyz/)
or 
[Host it yourself](https://github.com/Madpeterz/secondbot_web_folders)

###  HTTP scoped tokens
---
Scoped tokens allow you to hardcode access to the HTTP interface (if enabled)
and set more detailed control over what areas can be accessed
plus no need to give the full http 

> Security_WebUIKey 

You get set these tokens up by file or Environment Variables.
##### File

> {   
> "ScopedTokens": [
>     "t:[10charcode],ws:core,ws:group",
>     "t:[10charcode],cm:chat/localchathistory",   
>     ]
>   }


##### Environment Variables
|Name  |  Value|
|--|--|
| scoped_token_1 |  "t:[10charcode],ws:core,ws:group" |
| scoped_token_2 |  "t:[10charcode],cg:chat" |
| scoped_token_3 |  "t:[10charcode],cm:chat/localchathistory" |


###  HTTP scoped tokens (info)
---

    command [cm] "example: `cm:chat/localchathistory`
    
    workspaces [ws] "example: ws: core, ws: groups"
    
    commandgroups [cg] "example: `cg:chat`"
    	chat
    		chat/localchathistory
    		chat/localchatsay
    		groups/getgroupchat	
    		groups/sendgroupchat
    		groups/listgroups
    		im/chatwindows
    		im/listwithunread
    		im/getimchat
    		im/sendimchat
    
    	giver
    		inventory/send
    		inventory/folders
    		inventory/contents
    
    
    	groupinvite
    		[WIP]
    
    	movement
    		core/walkto
    		core/teleport
    		core/gesture



###  BetterRelay system
---
this is to replace the old broken relay thats currently built into the bot


> customrelay_1  = "source-type:discord,source-filter:123451231235@12351312321,target-type:localchat,target-config:4"
> customrelay_2  = "source-type:discord,source-filter:123451231235@12351312321,target-type:localchat,target-config:4"

or via the config file
customrelays

>{
>"CustomRelays": [
>	source-type:discord,source-filter:123451231235@12351312321,target-type:chat,target-config:4
>]
}

to config the relay please use the settings below


    source-type
	    discord
		    source-filter: serverid@serverchannel
	
	    localchat
		    source-filter: talker uuid or "all"

	    avatarim
		    source-filter: avatar uuid or "all"

	    objectim
		    source-filter: object uuid or "all"

	    groupchat
		    source-filter: group uuid or "all"


    source-type
	    discord
		    target-config: serverid@serverchannel
	
	    localchat
		    target-config: channel (default is 0)

	    avatarchat
		    target-config: avatar uuid

	    groupchat
		    target-config: group uuid
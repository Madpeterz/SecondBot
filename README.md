
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

[Windows x64](https://github.com/Madpeterz/SecondBot/releases/tag/windows-x64)

[Linux (Tested on Deb) x64](https://github.com/Madpeterz/SecondBot/releases/tag/linux-x64)
 
 
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




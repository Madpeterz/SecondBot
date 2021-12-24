## animation
 
### AddToAllowAnimations
 
http://localhost:8080/animation/AddToAllowAnimations/{avatar}/{token}
Method: Get
OR
AddToAllowAnimations|||{avatar}
OR
Toggles if animation requests from this avatar (used for remote poseballs) are accepted
***Args helper***
 
- [:heavy_check_mark:] Granted perm animation
- [:heavy_check_mark:] Removed perm animation
- [:x:] avatar lookup
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Granted perm animation
- [:heavy_check_mark:] Removed perm animation
- [:x:] avatar lookup
- [:x:] Token not accepted

 
### PlayGesture
 
http://localhost:8080/animation/PlayGesture/{gesture}/{token}
Method: Get
OR
PlayGesture|||{gesture}
OR
Attempts to play a gesture
***Args helper***
 
- [:heavy_check_mark:] Accepted
- [:x:] Error with gesture
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Accepted
- [:x:] Error with gesture
- [:x:] Token not accepted

 
### ResetAnimations
 
http://localhost:8080/animation/ResetAnimations/{token}
Method: Get
OR
ResetAnimations
OR
Resets the animation stack for the bot
***Args helper***
 
- [:heavy_check_mark:] Accepted
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Accepted
- [:x:] Token not accepted

 
## avatars
 
### NearmeWithDetails
 
http://localhost:8080/avatars/NearmeWithDetails/{token}
Method: Get
OR
NearmeWithDetails
OR
an improved version of near me with extra details<br/>NearMeDetails is a object formated as follows<br/><ul><li>id</li><li>name</li><li>x</li><li>y</li><li>z</li><li>range</li></ul>
***Args helper***
 
- [:heavy_check_mark:] array NearMeDetails
- [:x:] Error not in a sim
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] array NearMeDetails
- [:x:] Error not in a sim
- [:x:] Token not accepted

 
### Nearme
 
http://localhost:8080/avatars/Nearme/{token}
Method: Get
OR
Nearme
OR
returns a list of all known avatars nearby
***Args helper***
 
- [:heavy_check_mark:] array UUID = Name
- [:x:] Error not in a sim
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] array UUID = Name
- [:x:] Error not in a sim
- [:x:] Token not accepted

 
## chat
 
### LocalChatHistory
 
http://localhost:8080/chat/LocalChatHistory/{token}
Method: Get
OR
LocalChatHistory
OR
fetchs the last 20 localchat messages
***Args helper***
 
- [:heavy_check_mark:] array string
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] array string
- [:x:] Token not accepted

 
### Say
 
http://localhost:8080/chat/Say/{channel}/{token}
Method: Post
OR
Say|||{channel}~#~{message}
OR
sends a message to localchat
***Args helper***
 
- [:heavy_check_mark:] array string
- [:x:] Message empty
- [:x:] Invaild channel
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] array string
- [:x:] Message empty
- [:x:] Invaild channel
- [:x:] Token not accepted

 
### IM
 
http://localhost:8080/chat/IM/{avatar}/{token}
Method: Post
OR
IM|||{avatar}~#~{message}
OR
sends a im to the selected avatar
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Message empty
- [:x:] avatar lookup
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Message empty
- [:x:] avatar lookup
- [:x:] Token not accepted

 
### chatwindows
 
http://localhost:8080/chat/chatwindows/{token}
Method: Get
OR
chatwindows
OR
gets a full list of all chat windows
***Args helper***
 
- [:heavy_check_mark:] array UUID = Name
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] array UUID = Name
- [:x:] Token not accepted

 
### listwithunread
 
http://localhost:8080/chat/listwithunread/{token}
Method: Get
OR
listwithunread
OR
gets a list of chat windows with unread messages
***Args helper***
 
- [:heavy_check_mark:] array of UUID
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] array of UUID
- [:x:] Token not accepted

 
### haveunreadims
 
http://localhost:8080/chat/haveunreadims/{token}
Method: Get
OR
haveunreadims
OR
gets if there are any unread im messages at all
***Args helper***
 
- [:heavy_check_mark:] True|False
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] True|False
- [:x:] Token not accepted

 
### getimchat
 
http://localhost:8080/chat/getimchat/{window}/{token}
Method: Get
OR
getimchat|||{window}
OR
gets the chat from the selected window
***Args helper***
 
- [:heavy_check_mark:] Array of text
- [:x:] Window UUID invaild
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Array of text
- [:x:] Window UUID invaild
- [:x:] Token not accepted

 
## core
 
### GetToken
 
http://localhost:8080/core/GetToken/
Method: Post
OR
GetToken|||{authcode}~#~{unixtimegiven}
OR
Requests a new token (Vaild for 10 mins) <br/>to use with all other requests
***Args helper***
 
- [:heavy_check_mark:] A new token with full system scope
- [:x:] Authcode not accepted

***Replys***
 
- [:heavy_check_mark:] A new token with full system scope
- [:x:] Authcode not accepted

 
### Hello
 
http://localhost:8080/core/Hello/
Method: Get
OR
Hello
OR
Used to check HTTP connections
***Args helper***
 
- [:heavy_check_mark:] world

***Replys***
 
- [:heavy_check_mark:] world

 
### Delay
 
http://localhost:8080/core/Delay/{token}
Method: Get
OR
Delay
OR
Delays a thead by X ms<br/>Mostly pointless but good if your doing custom commands
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Invaild amount
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Invaild amount
- [:x:] Token not accepted

 
### LogoutUI
 
http://localhost:8080/core/LogoutUI/{token}
Method: Get
OR
LogoutUI
OR
Removes the given token from the accepted token pool
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Token not accepted

 
## dialogs
 
### DialogRelay
 
http://localhost:8080/dialogs/DialogRelay/{target}/{token}
Method: Get
OR
DialogRelay|||{target}
OR
Updates the relay target (you can have 1 of each type)<br/>Clear will disable them all
***Args helper***
 
- [:heavy_check_mark:] cleared
- [:heavy_check_mark:] set/avatar [ok]
- [:heavy_check_mark:] set/http [ok]
- [:heavy_check_mark:] set/channel [ok]
- [:x:] Not a vaild option
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] cleared
- [:heavy_check_mark:] set/avatar [ok]
- [:heavy_check_mark:] set/http [ok]
- [:heavy_check_mark:] set/channel [ok]
- [:x:] Not a vaild option
- [:x:] Token not accepted

 
### DialogResponce
 
http://localhost:8080/dialogs/DialogResponce/{dialogid}/{buttontext}/{token}
Method: Get
OR
DialogResponce|||{dialogid}~#~{buttontext}
OR
Makes the bot interact with the dialog [dialogid] with the button [buttontext]
***Args helper***
 
- [:heavy_check_mark:] true
- [:heavy_check_mark:] false
- [:x:] bad dialog id
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] true
- [:heavy_check_mark:] false
- [:x:] bad dialog id
- [:x:] Token not accepted

 
### DialogTrack
 
http://localhost:8080/dialogs/DialogTrack/{status}/{token}
Method: Get
OR
DialogTrack|||{status}
OR
Should the bot track dialogs and send them to the relays setup?
***Args helper***
 
- [:heavy_check_mark:] updated
- [:x:] bad status
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] updated
- [:x:] bad status
- [:x:] Token not accepted

 
## discord
 
### Discord_AddRole
 
http://localhost:8080/discord/Discord_AddRole/{serverid}/{roleid}/{memberid}/{token}
Method: Get
OR
Discord_AddRole|||{serverid}~#~{roleid}~#~{memberid}
OR
Adds a discord server role to the selected member
***Args helper***
 
- [:heavy_check_mark:] true|false
- [:x:] Discord client not ready
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] true|false
- [:x:] Discord client not ready
- [:x:] Token not accepted

 
### Discord_BanMember
 
http://localhost:8080/discord/Discord_BanMember/{serverid}/{memberid}/{token}
Method: Post
OR
Discord_BanMember|||{serverid}~#~{memberid}~#~{why}
OR
Adds the selected user to the ban list - Disallows rejoining untill they are removed from the list
***Args helper***
 
- [:heavy_check_mark:] true|false
- [:x:] Discord client not ready
- [:x:] Why empty
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] true|false
- [:x:] Discord client not ready
- [:x:] Why empty
- [:x:] Token not accepted

 
### Discord_BulkClear_Messages
 
http://localhost:8080/discord/Discord_BulkClear_Messages/{serverid}/{memberid}/{token}
Method: Get
OR
Discord_BulkClear_Messages|||{serverid}~#~{memberid}
OR
Clears messages on the server sent by the member in the last 13 days, 22hours 59mins
***Args helper***
 
- [:heavy_check_mark:] true|false
- [:x:] Discord client not ready
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] true|false
- [:x:] Discord client not ready
- [:x:] Token not accepted

 
### Discord_Dm_Member
 
http://localhost:8080/discord/Discord_Dm_Member/{serverid}/{memberid}/{token}
Method: Post
OR
Discord_Dm_Member|||{serverid}~#~{memberid}~#~{message}
OR
Sends a message directly to the user [They must be in the server]
 This command requires the SERVER MEMBERS INTENT found in discord app dev
***Args helper***
 
- [:heavy_check_mark:] true|false
- [:x:] Discord client not ready
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] true|false
- [:x:] Discord client not ready
- [:x:] Token not accepted

 
### Discord_MembersList
 
http://localhost:8080/discord/Discord_MembersList/{serverid}/{token}
Method: Get
OR
Discord_MembersList|||{serverid}
OR
Returns a list of members in a server 
 collection is userid: username 
 if the user has set a nickname: userid: nickname|username 
 This command requires Discord full client mode enabled and connected
 !!!! This command also requires: Privileged Gateway Intents / SERVER MEMBERS INTENT set to true on the discord bot api area !!!
***Args helper***
 
- [:heavy_check_mark:] mixed array of userid: nickname|username  or   userid:username
- [:x:] Discord client not ready
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] mixed array of userid: nickname|username  or   userid:username
- [:x:] Discord client not ready
- [:x:] Token not accepted

 
### Discord_MessageChannel
 
http://localhost:8080/discord/Discord_MessageChannel/{serverid}/{channelid}/{tts}/{token}
Method: Post
OR
Discord_MessageChannel|||{serverid}~#~{channelid}~#~{tts}~#~{message}
OR
Sends a message to the selected channel - Optional TTS usage
***Args helper***
 
- [:heavy_check_mark:] true|false
- [:x:] Discord client not ready
- [:x:] message empty
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] true|false
- [:x:] Discord client not ready
- [:x:] message empty
- [:x:] Token not accepted

 
### Discord_MuteMember
 
http://localhost:8080/discord/Discord_MuteMember/{serverid}/{memberid}/{mode}/{token}
Method: Get
OR
Discord_MuteMember|||{serverid}~#~{memberid}~#~{mode}
OR
Sends a message to the selected channel - Optional TTS usage
***Args helper***
 
- [:heavy_check_mark:] mixed array of userid: nickname|username  or   userid:username
- [:x:] Discord client not ready
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] mixed array of userid: nickname|username  or   userid:username
- [:x:] Discord client not ready
- [:x:] Token not accepted

 
### Discord_Role_GetSettings
 
http://localhost:8080/discord/Discord_Role_GetSettings/{serverid}/{memberid}/{mode}/{token}
Method: Get
OR
Discord_Role_GetSettings|||{serverid}~#~{memberid}~#~{mode}
OR
returns a collection of settings for the given role 
 This command requires Discord full client mode enabled and connected
***Args helper***
 
- [:heavy_check_mark:] KeyPair of status: KeyPair[] item = value
- [:x:] Discord client not ready
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] KeyPair of status: KeyPair[] item = value
- [:x:] Discord client not ready
- [:x:] Token not accepted

 
### Discord_Role_UpdatePerms
 
http://localhost:8080/discord/Discord_Role_UpdatePerms/{serverid}/{roleid}/{token}
Method: Post
OR
Discord_Role_UpdatePerms|||{serverid}~#~{roleid}~#~{flagscsv}
OR
Updates perm flags for the selected role 
 example CSV format: Speak=True,SendMessages=False 
 for a full list of perms see output of Discord_Role_GetSettings 
 This command requires Discord full client mode enabled and connected
***Args helper***
 
- [:heavy_check_mark:] true|false
- [:x:] Discord client not ready
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] true|false
- [:x:] Discord client not ready
- [:x:] Token not accepted

 
### Discord_RoleCreate
 
http://localhost:8080/discord/Discord_RoleCreate/{serverid}/{role}/{token}
Method: Get
OR
Discord_RoleCreate|||{serverid}~#~{role}
OR
Updates perm flags for the selected role 
 example CSV format: Speak=True,SendMessages=False 
 for a full list of perms see output of Discord_Role_GetSettings 
 This command requires Discord full client mode enabled and connected
***Args helper***
 
- [:heavy_check_mark:] KeyPair of statusmessage=roleid or 0
- [:x:] Discord client not ready
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] KeyPair of statusmessage=roleid or 0
- [:x:] Discord client not ready
- [:x:] Token not accepted

 
### Discord_RoleList
 
http://localhost:8080/discord/Discord_RoleList/{serverid}/{token}
Method: Get
OR
Discord_RoleList|||{serverid}
OR
Returns a list of roles and their ids in collection 
 This command requires Discord full client mode enabled and connected
***Args helper***
 
- [:heavy_check_mark:] KeyPair of status: KeyPair of roleid: rolename
- [:x:] Discord client not ready
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] KeyPair of status: KeyPair of roleid: rolename
- [:x:] Discord client not ready
- [:x:] Token not accepted

 
### Discord_RoleRemove
 
http://localhost:8080/discord/Discord_RoleRemove/{serverid}/{token}
Method: Get
OR
Discord_RoleRemove|||{serverid}
OR
Remove a role from a server 
 This command requires Discord full client mode enabled and connected
***Args helper***
 
- [:heavy_check_mark:] true|false
- [:x:] Discord client not ready
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] true|false
- [:x:] Discord client not ready
- [:x:] Token not accepted

 
### Discord_TextChannels_List
 
http://localhost:8080/discord/Discord_TextChannels_List/{serverid}/{token}
Method: Get
OR
Discord_TextChannels_List|||{serverid}
OR
Returns a list of text channels in a server
***Args helper***
 
- [:heavy_check_mark:] array of channelid: name
- [:x:] Discord client not ready
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] array of channelid: name
- [:x:] Discord client not ready
- [:x:] Token not accepted

 
## estate
 
### SimRestart
 
http://localhost:8080/estate/SimRestart/{delay}/{mode}/{token}
Method: Get
OR
SimRestart|||{delay}~#~{mode}
OR
Sends the message to the current sim
***Args helper***
 
- [:heavy_check_mark:] restarting
- [:x:] Not an estate manager here
- [:x:] canceled
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] restarting
- [:x:] Not an estate manager here
- [:x:] canceled
- [:x:] Token not accepted

 
### SimMessage
 
http://localhost:8080/estate/SimMessage/{token}
Method: Post
OR
SimMessage|||{message}
OR
Sends the message to the current sim
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Not an estate manager here
- [:x:] Message empty
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Not an estate manager here
- [:x:] Message empty
- [:x:] Token not accepted

 
### GetSimTexture
 
http://localhost:8080/estate/GetSimTexture/{regionname}/{token}
Method: Get
OR
GetSimTexture|||{regionname}
OR
Fetchs the regions map tile
***Args helper***
 
- [:heavy_check_mark:] Texture UUID
- [:x:] Unable to find region
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Texture UUID
- [:x:] Unable to find region
- [:x:] Token not accepted

 
### EstateParcelReclaim
 
http://localhost:8080/estate/EstateParcelReclaim/{token}
Method: Get
OR
EstateParcelReclaim
OR
Reclaims ownership of the current parcel
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Not an estate manager here
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Not an estate manager here
- [:x:] Token not accepted

 
### GetSimGlobalPos
 
http://localhost:8080/estate/GetSimGlobalPos/{token}
Method: Get
OR
GetSimGlobalPos
OR
Reclaims ownership of the current parcel
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Not an estate manager here
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Not an estate manager here
- [:x:] Token not accepted

 
### GetEstateBanList
 
http://localhost:8080/estate/GetEstateBanList/{token}
Method: Get
OR
GetEstateBanList
OR
Requests the estate banlist
***Args helper***
 
- [:heavy_check_mark:] ban list json
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ban list json
- [:x:] Token not accepted

 
### UpdateEstateBanlist
 
http://localhost:8080/estate/UpdateEstateBanlist/{avatar}/{mode}/{global}/{token}
Method: Get
OR
UpdateEstateBanlist|||{avatar}~#~{mode}~#~{global}
OR
Attempts to add/remove the avatar to/from the Estate banlist
***Args helper***
 
- [:heavy_check_mark:] Unban request accepted
- [:heavy_check_mark:] Ban request accepted
- [:x:] Unable to find avatar UUID
- [:x:] Unable to process global value please use true or false
- [:x:] Not an estate manager on region {REGIONNAME}
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Unban request accepted
- [:heavy_check_mark:] Ban request accepted
- [:x:] Unable to find avatar UUID
- [:x:] Unable to process global value please use true or false
- [:x:] Not an estate manager on region {REGIONNAME}
- [:x:] Token not accepted

 
## friends
 
### Friendslist
 
http://localhost:8080/friends/Friendslist/{token}
Method: Get
OR
Friendslist
OR
Gets the friendslist <br/>Formated as follows<br/>friendreplyobject<br/><ul><li>name: String</li><li>id: String</li><li>online: bool</li></ul>
***Args helper***
 
- [:heavy_check_mark:] array UUID = friendreplyobject
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] array UUID = friendreplyobject
- [:x:] Token not accepted

 
### FriendFullPerms
 
http://localhost:8080/friends/FriendFullPerms/{avatar}/{state}/{token}
Method: Get
OR
FriendFullPerms|||{avatar}~#~{state}
OR
Updates the friend perms for avatar avatar to State 
 if true grants (Online/Map/Modify) perms
***Args helper***
 
- [:heavy_check_mark:] granted
- [:heavy_check_mark:] removed
- [:x:] Not A friend
- [:x:] state invaild
- [:x:] avatar lookup
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] granted
- [:heavy_check_mark:] removed
- [:x:] Not A friend
- [:x:] state invaild
- [:x:] avatar lookup
- [:x:] Token not accepted

 
### FriendRequest
 
http://localhost:8080/friends/FriendRequest/{avatar}/{state}/{token}
Method: Get
OR
FriendRequest|||{avatar}~#~{state}
OR
Updates the friend perms for avatar avatar to State 
 if true grants (Online/Map/Modify) perms
***Args helper***
 
- [:heavy_check_mark:] Request sent
- [:heavy_check_mark:] Removed
- [:x:] Already a friend
- [:x:] Not in friendslist
- [:x:] state invaild
- [:x:] avatar lookup
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Request sent
- [:heavy_check_mark:] Removed
- [:x:] Already a friend
- [:x:] Not in friendslist
- [:x:] state invaild
- [:x:] avatar lookup
- [:x:] Token not accepted

 
## funds
 
### Balance
 
http://localhost:8080/funds/Balance/{token}
Method: Get
OR
Balance
OR
Requests the current balance and requests the balance to update.
***Args helper***
 
- [:heavy_check_mark:] Current fund level
- [:x:] Funds commands are disabled
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Current fund level
- [:x:] Funds commands are disabled
- [:x:] Token not accepted

 
### PayAvatar
 
http://localhost:8080/funds/PayAvatar/{avatar}/{amount}/{token}
Method: Get
OR
PayAvatar|||{avatar}~#~{amount}
OR
Makes the bot pay a avatar
***Args helper***
 
- [:heavy_check_mark:] Accepted
- [:x:] avatar lookup
- [:x:] Amount out of range
- [:x:] Invaild amount
- [:x:] Transfer funds to avatars disabled
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Accepted
- [:x:] avatar lookup
- [:x:] Amount out of range
- [:x:] Invaild amount
- [:x:] Transfer funds to avatars disabled
- [:x:] Token not accepted

 
### PayObject
 
http://localhost:8080/funds/PayObject/{object}/{primname}/{amount}/{token}
Method: Get
OR
PayObject|||{object}~#~{primname}~#~{amount}
OR
Makes the bot pay a object
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Primname is empty
- [:x:] Invaild object UUID
- [:x:] Invaild amount
- [:x:] Amount out of range
- [:x:] Funds commands are disabled
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Primname is empty
- [:x:] Invaild object UUID
- [:x:] Invaild amount
- [:x:] Amount out of range
- [:x:] Funds commands are disabled
- [:x:] Token not accepted

 
## group
 
### IsGroupMember
 
http://localhost:8080/group/IsGroupMember/{group}/{avatar}/{token}
Method: Get
OR
IsGroupMember|||{group}~#~{avatar}
OR
Checks if the given UUID is in the given group<br/>Note: if group membership data is more than 60 secs old this will return Updating<br/>Please wait and retry later
***Args helper***
 
- [:heavy_check_mark:] Membership reply with [membershipStatus,AvatarUUID,AvatarnameIfKnown,GroupUUID]
- [:x:] Updating
- [:x:] Unknown group
- [:x:] Invaild group UUID
- [:x:] avatar lookup
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Membership reply with [membershipStatus,AvatarUUID,AvatarnameIfKnown,GroupUUID]
- [:x:] Updating
- [:x:] Unknown group
- [:x:] Invaild group UUID
- [:x:] avatar lookup
- [:x:] Token not accepted

 
### GetGroupMembers
 
http://localhost:8080/group/GetGroupMembers/{group}/{token}
Method: Get
OR
GetGroupMembers|||{group}
OR
Gets membership of a group
***Args helper***
 
- [:heavy_check_mark:] list of UUIDS of group members
- [:x:] Updating
- [:x:] Unknown group
- [:x:] Invaild group UUID
- [:x:] avatar lookup
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] list of UUIDS of group members
- [:x:] Updating
- [:x:] Unknown group
- [:x:] Invaild group UUID
- [:x:] avatar lookup
- [:x:] Token not accepted

 
### GroupBan
 
http://localhost:8080/group/GroupBan/{group}/{avatar}/{state}/{token}
Method: Get
OR
GroupBan|||{group}~#~{avatar}~#~{state}
OR
Attempts to ban/unban a given avatar from a group
***Args helper***
 
- [:heavy_check_mark:] ? request accepted
- [:x:] Updating
- [:x:] Unknown group
- [:x:] Invaild group UUID
- [:x:] avatar lookup
- [:x:] Missing group GroupBanAccess power
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ? request accepted
- [:x:] Updating
- [:x:] Unknown group
- [:x:] Invaild group UUID
- [:x:] avatar lookup
- [:x:] Missing group GroupBanAccess power
- [:x:] Token not accepted

 
### GroupEject
 
http://localhost:8080/group/GroupEject/{group}/{avatar}/{token}
Method: Get
OR
GroupEject|||{group}~#~{avatar}
OR
Eject selected avatar from group
***Args helper***
 
- [:heavy_check_mark:] Requested
- [:x:] Updating
- [:x:] Unknown group
- [:x:] Invaild group UUID
- [:x:] Not in group
- [:x:] avatar lookup
- [:x:] Missing group Eject power
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Requested
- [:x:] Updating
- [:x:] Unknown group
- [:x:] Invaild group UUID
- [:x:] Not in group
- [:x:] avatar lookup
- [:x:] Missing group Eject power
- [:x:] Token not accepted

 
### GroupAddRole
 
http://localhost:8080/group/GroupAddRole/{group}/{avatar}/{token}
Method: Get
OR
GroupAddRole|||{group}~#~{avatar}
OR
Adds the avatar to the Group with the role 
 if they are not in the group then it invites them at that role
***Args helper***
 
- [:heavy_check_mark:] Roles updated
- [:heavy_check_mark:] Invite sent
- [:x:] Updating
- [:x:] Unknown group
- [:x:] Invaild group UUID
- [:x:] Invaild role UUID
- [:x:] Not in group
- [:x:] avatar lookup
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Roles updated
- [:heavy_check_mark:] Invite sent
- [:x:] Updating
- [:x:] Unknown group
- [:x:] Invaild group UUID
- [:x:] Invaild role UUID
- [:x:] Not in group
- [:x:] avatar lookup
- [:x:] Token not accepted

 
### GroupInvite
 
http://localhost:8080/group/GroupInvite/{group}/{avatar}/{role}/{token}
Method: Get
OR
GroupInvite|||{group}~#~{avatar}~#~{role}
OR
Invites selected avatar to the group with the selected role
***Args helper***
 
- [:heavy_check_mark:] Invite sent
- [:heavy_check_mark:] Already in group
- [:x:] Updating
- [:x:] Unknown group
- [:x:] Invaild group UUID
- [:x:] avatar lookup
- [:x:] Missing group Invite power
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Invite sent
- [:heavy_check_mark:] Already in group
- [:x:] Updating
- [:x:] Unknown group
- [:x:] Invaild group UUID
- [:x:] avatar lookup
- [:x:] Missing group Invite power
- [:x:] Token not accepted

 
### Groupnotice
 
http://localhost:8080/group/Groupnotice/{group}/{token}
Method: Post
OR
Groupnotice|||{group}~#~{title}~#~{message}
OR
Sends a group notice (No attachments please use GroupnoticeWithAttachment to attach items!)
***Args helper***
 
- [:heavy_check_mark:] Sending notice
- [:x:] Unknown group
- [:x:] Invaild group UUID
- [:x:] Title empty
- [:x:] Message empty
- [:x:] Missing group Notice power
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Sending notice
- [:x:] Unknown group
- [:x:] Invaild group UUID
- [:x:] Title empty
- [:x:] Message empty
- [:x:] Missing group Notice power
- [:x:] Token not accepted

 
### GroupActiveTitle
 
http://localhost:8080/group/GroupActiveTitle/{group}/{role}/{token}
Method: Get
OR
GroupActiveTitle|||{group}~#~{role}
OR
Activates the selected title
***Args helper***
 
- [:heavy_check_mark:] Switching title
- [:x:] Unknown group
- [:x:] Invaild group UUID
- [:x:] Invaild role UUID
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Switching title
- [:x:] Unknown group
- [:x:] Invaild group UUID
- [:x:] Invaild role UUID
- [:x:] Token not accepted

 
### GroupActiveGroup
 
http://localhost:8080/group/GroupActiveGroup/{group}/{token}
Method: Get
OR
GroupActiveGroup|||{group}
OR
Sets the selected group to the active group
***Args helper***
 
- [:heavy_check_mark:] Switching active group
- [:x:] Unknown group
- [:x:] Invaild group UUID
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Switching active group
- [:x:] Unknown group
- [:x:] Invaild group UUID
- [:x:] Token not accepted

 
### GroupnoticeWithAttachment
 
http://localhost:8080/group/GroupnoticeWithAttachment/{group}/{attachment}/{token}
Method: Post
OR
GroupnoticeWithAttachment|||{group}~#~{title}~#~{message}~#~{attachment}
OR
Sends a group notice with an attachment
***Args helper***
 
- [:heavy_check_mark:] Sending notice with attachment
- [:x:] Unknown group
- [:x:] Invaild group UUID
- [:x:] Invaild inventory UUID
- [:x:] Title empty
- [:x:] Message empty
- [:x:] Missing group Notice power
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Sending notice with attachment
- [:x:] Unknown group
- [:x:] Invaild group UUID
- [:x:] Invaild inventory UUID
- [:x:] Title empty
- [:x:] Message empty
- [:x:] Missing group Notice power
- [:x:] Token not accepted

 
### GetGroupList
 
http://localhost:8080/group/GetGroupList/{token}
Method: Get
OR
GetGroupList
OR
fetchs a list of all groups known to the bot
***Args helper***
 
- [:heavy_check_mark:] array UUID=name
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] array UUID=name
- [:x:] Token not accepted

 
### GetGroupRoles
 
http://localhost:8080/group/GetGroupRoles/{group}/{token}
Method: Get
OR
GetGroupRoles|||{group}
OR
Requests the roles for the selected group<br/>Replys with GroupRoleDetails object formated as follows <ul><li>UpdateUnderway (Bool)</li><li>RoleDataAge (Int) [default -1]</li><li>Roles (KeyPair array of UUID=Name)</li></ul><br/>
***Args helper***
 
- [:heavy_check_mark:] GroupRoleDetails object
- [:x:] Group is not currently known
- [:x:] Invaild group UUID
- [:x:] Updating
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] GroupRoleDetails object
- [:x:] Group is not currently known
- [:x:] Invaild group UUID
- [:x:] Updating
- [:x:] Token not accepted

 
### GroupchatListAllUnreadGroups
 
http://localhost:8080/group/GroupchatListAllUnreadGroups/{token}
Method: Get
OR
GroupchatListAllUnreadGroups
OR
fetchs a list of all groups with unread messages
***Args helper***
 
- [:heavy_check_mark:] array UUID
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] array UUID
- [:x:] Token not accepted

 
### GroupchatGroupHasUnread
 
http://localhost:8080/group/GroupchatGroupHasUnread/{group}/{token}
Method: Get
OR
GroupchatGroupHasUnread|||{group}
OR
fetchs a list of all groups with unread messages
***Args helper***
 
- [:heavy_check_mark:] true|false
- [:x:] Unknown group
- [:x:] group value is invaild
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] true|false
- [:x:] Unknown group
- [:x:] group value is invaild
- [:x:] Token not accepted

 
### GroupchatAnyUnread
 
http://localhost:8080/group/GroupchatAnyUnread/{token}
Method: Get
OR
GroupchatAnyUnread
OR
checks if there are any groups with unread messages
***Args helper***
 
- [:heavy_check_mark:] True|False
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] True|False
- [:x:] Token not accepted

 
### GroupchatClearAll
 
http://localhost:8080/group/GroupchatClearAll/{token}
Method: Get
OR
GroupchatClearAll
OR
Clears all group chat buffers at once
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Token not accepted

 
### GroupchatHistory
 
http://localhost:8080/group/GroupchatHistory/{group}/{token}
Method: Get
OR
GroupchatHistory|||{group}
OR
fetchs the groupchat history
***Args helper***
 
- [:heavy_check_mark:] Group Chat
- [:x:] Group UUID invaild
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Group Chat
- [:x:] Group UUID invaild
- [:x:] Token not accepted

 
### Groupchat
 
http://localhost:8080/group/Groupchat/{group}/{token}
Method: Post
OR
Groupchat|||{group}~#~{message}
OR
sends a message to the groupchat
***Args helper***
 
- [:heavy_check_mark:] Sending
- [:x:] Group UUID invaild
- [:x:] Opening groupchat - Please retry later
- [:x:] Missing group JoinChat power
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Sending
- [:x:] Group UUID invaild
- [:x:] Opening groupchat - Please retry later
- [:x:] Missing group JoinChat power
- [:x:] Token not accepted

 
## info
 
### ListSculptys
 
http://localhost:8080/info/ListSculptys/{token}
Method: Get
OR
ListSculptys
OR
Lists objects that are sculpty type in the current sim that the bot can see
***Args helper***
 
- [:heavy_check_mark:] A json object
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] A json object
- [:x:] Token not accepted

 
### Version
 
http://localhost:8080/info/Version/{token}
Method: Get
OR
Version
OR
Fetchs the current bot
***Args helper***
 
- [:heavy_check_mark:] The build ID of the bot
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] The build ID of the bot
- [:x:] Token not accepted

 
### Name
 
http://localhost:8080/info/Name/{token}
Method: Get
OR
Name
OR
Fetchs the name of the bot
***Args helper***
 
- [:heavy_check_mark:] Firstname Lastname
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Firstname Lastname
- [:x:] Token not accepted

 
### ParcelName
 
http://localhost:8080/info/ParcelName/{token}
Method: Get
OR
ParcelName
OR
Fetchs the current parcels name
***Args helper***
 
- [:heavy_check_mark:] Parcelname
- [:x:] Error parcel not found
- [:x:] Error not in a sim
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Parcelname
- [:x:] Error parcel not found
- [:x:] Error not in a sim
- [:x:] Token not accepted

 
### UnixTimeNow
 
http://localhost:8080/info/UnixTimeNow/{token}
Method: Get
OR
UnixTimeNow
OR
Requests the current unixtime at the bot
***Args helper***
 
- [:heavy_check_mark:] Unixtime
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Unixtime
- [:x:] Token not accepted

 
### SimName
 
http://localhost:8080/info/SimName/{token}
Method: Get
OR
SimName
OR
Fetchs the current region name
***Args helper***
 
- [:heavy_check_mark:] Regionname
- [:x:] Error not in a sim
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Regionname
- [:x:] Error not in a sim
- [:x:] Token not accepted

 
### GetPosition
 
http://localhost:8080/info/GetPosition/{token}
Method: Get
OR
GetPosition
OR
Fetchs the current location of the bot
***Args helper***
 
- [:heavy_check_mark:] array of X,Y,Z values
- [:x:] Error not in a sim
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] array of X,Y,Z values
- [:x:] Error not in a sim
- [:x:] Token not accepted

 
## inventory
 
### SetInventoryUpdate
 
http://localhost:8080/inventory/SetInventoryUpdate/{inventoryType}/{token}
Method: Post
OR
SetInventoryUpdate|||{inventoryType}~#~{outputTarget}
OR
Attachs an event for inventory changes
***Args helper***
 
- [:heavy_check_mark:] cleared
- [:heavy_check_mark:] No action
- [:heavy_check_mark:] Event added
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] cleared
- [:heavy_check_mark:] No action
- [:heavy_check_mark:] Event added
- [:x:] Token not accepted

 
### UploadMediaWave
 
http://localhost:8080/inventory/UploadMediaWave/{inventoryName}/{token}
Method: Post
OR
UploadMediaWave|||{sourcePath}~#~{inventoryName}
OR
Uploads a new sound file to inventory
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Token not accepted

 
### RezObject
 
http://localhost:8080/inventory/RezObject/{item}/{token}
Method: Get
OR
RezObject|||{item}
OR
rezs the item at the bots current location
***Args helper***
 
- [:heavy_check_mark:] UUID of rezzed item
- [:heavy_check_mark:] Invaild item UUID
- [:heavy_check_mark:] Unable to find item
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] UUID of rezzed item
- [:heavy_check_mark:] Invaild item UUID
- [:heavy_check_mark:] Unable to find item
- [:x:] Token not accepted

 
### RenameInventory
 
http://localhost:8080/inventory/RenameInventory/{item}/{token}
Method: Post
OR
RenameInventory|||{item}~#~{newname}
OR
renames a folder or inventory item
***Args helper***
 
- [:heavy_check_mark:] ok
- [:heavy_check_mark:] invaild item uuid
- [:heavy_check_mark:] Item name is to short
- [:heavy_check_mark:] Unable to find inventory item
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:heavy_check_mark:] invaild item uuid
- [:heavy_check_mark:] Item name is to short
- [:heavy_check_mark:] Unable to find inventory item
- [:x:] Token not accepted

 
### DeleteInventoryItem
 
http://localhost:8080/inventory/DeleteInventoryItem/{item}/{token}
Method: Get
OR
DeleteInventoryItem|||{item}
OR
Attempts to Remove the given inventory item
***Args helper***
 
- [:heavy_check_mark:] ok
- [:heavy_check_mark:] invaild item uuid
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:heavy_check_mark:] invaild item uuid
- [:x:] Token not accepted

 
### DeleteInventoryFolder
 
http://localhost:8080/inventory/DeleteInventoryFolder/{folder}/{token}
Method: Get
OR
DeleteInventoryFolder|||{folder}
OR
Attempts to Remove the given inventory folder
***Args helper***
 
- [:heavy_check_mark:] ok
- [:heavy_check_mark:] invaild folder uuid
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:heavy_check_mark:] invaild folder uuid
- [:x:] Token not accepted

 
### Attach
 
http://localhost:8080/inventory/Attach/{item}/{token}
Method: Get
OR
Attach|||{item}
OR
Attempts to attach the given inventory item
***Args helper***
 
- [:heavy_check_mark:] ok
- [:heavy_check_mark:] invaild item uuid
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:heavy_check_mark:] invaild item uuid
- [:x:] Token not accepted

 
### Detach
 
http://localhost:8080/inventory/Detach/{item}/{token}
Method: Get
OR
Detach|||{item}
OR
Attempts to Remove the given inventory folder
***Args helper***
 
- [:heavy_check_mark:] ok
- [:heavy_check_mark:] invaild item uuid
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:heavy_check_mark:] invaild item uuid
- [:x:] Token not accepted

 
### Outfit
 
http://localhost:8080/inventory/Outfit/{name}/{token}
Method: Get
OR
Outfit|||{name}
OR
Replaces the current avatar outfit with the Clothing/[NAME] folder<br/>Please note: This does not use the outfits folder!<br/>Please do not use links in the folder!
***Args helper***
 
- [:heavy_check_mark:] ok
- [:heavy_check_mark:] Named folder value is empty
- [:heavy_check_mark:] Cant find Clothing folder
- [:heavy_check_mark:] Cant find target folder
- [:heavy_check_mark:] target folder is empty or so full I cant get it in 5 secs...
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:heavy_check_mark:] Named folder value is empty
- [:heavy_check_mark:] Cant find Clothing folder
- [:heavy_check_mark:] Cant find target folder
- [:heavy_check_mark:] target folder is empty or so full I cant get it in 5 secs...
- [:x:] Token not accepted

 
### InventoryPurgeNotecards
 
http://localhost:8080/inventory/InventoryPurgeNotecards/{token}
Method: Get
OR
InventoryPurgeNotecards
OR
Searchs the notecards folder for notecards, any older than 31 days are deleted.<br/>Depending on the number of notecards this might require multiple calls!
***Args helper***
 
- [:heavy_check_mark:] ok
- [:heavy_check_mark:] Unable to find notecard folder
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:heavy_check_mark:] Unable to find notecard folder
- [:x:] Token not accepted

 
### getRealUUID
 
http://localhost:8080/inventory/getRealUUID/{item}/{token}
Method: Get
OR
getRealUUID|||{item}
OR
converts a inventory uuid to a realworld uuid<br/>Needed for texture preview
***Args helper***
 
- [:heavy_check_mark:] Asset UUID or UUID zero
- [:heavy_check_mark:] Invaild item uuid
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Asset UUID or UUID zero
- [:heavy_check_mark:] Invaild item uuid
- [:x:] Token not accepted

 
### SendItem
 
http://localhost:8080/inventory/SendItem/{item}/{avatar}/{token}
Method: Get
OR
SendItem|||{item}~#~{avatar}
OR
sends a item to an avatar
***Args helper***
 
- [:heavy_check_mark:] ok
- [:heavy_check_mark:] Failed
- [:heavy_check_mark:] Invaild avatar uuid
- [:heavy_check_mark:] Invaild item uuid
- [:heavy_check_mark:] Unable to find item
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:heavy_check_mark:] Failed
- [:heavy_check_mark:] Invaild avatar uuid
- [:heavy_check_mark:] Invaild item uuid
- [:heavy_check_mark:] Unable to find item
- [:x:] Token not accepted

 
### SendFolder
 
http://localhost:8080/inventory/SendFolder/{item}/{avatar}/{token}
Method: Get
OR
SendFolder|||{item}~#~{avatar}
OR
Sends a folder to an avatar
***Args helper***
 
- [:heavy_check_mark:] ok
- [:heavy_check_mark:] Failed
- [:heavy_check_mark:] Invaild avatar uuid
- [:heavy_check_mark:] Invaild folter uuid
- [:heavy_check_mark:] Unable to find folder
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:heavy_check_mark:] Failed
- [:heavy_check_mark:] Invaild avatar uuid
- [:heavy_check_mark:] Invaild folter uuid
- [:heavy_check_mark:] Unable to find folder
- [:x:] Token not accepted

 
### TransferInventoryToObject
 
http://localhost:8080/inventory/TransferInventoryToObject/{item}/{object}/{running}/{token}
Method: Get
OR
TransferInventoryToObject|||{item}~#~{object}~#~{running}
OR
Transfers a item [ARG 2] to a objects inventory [ARG 1] (And if set with the script running state [ARG 3])
***Args helper***
 
- [:heavy_check_mark:] Transfering running script
- [:heavy_check_mark:] Transfering inventory
- [:heavy_check_mark:] Invaild item uuid
- [:heavy_check_mark:] Invaild object uuid
- [:heavy_check_mark:] Unable to find inventory
- [:heavy_check_mark:] Unable to find object
- [:heavy_check_mark:] Invaild running
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Transfering running script
- [:heavy_check_mark:] Transfering inventory
- [:heavy_check_mark:] Invaild item uuid
- [:heavy_check_mark:] Invaild object uuid
- [:heavy_check_mark:] Unable to find inventory
- [:heavy_check_mark:] Unable to find object
- [:heavy_check_mark:] Invaild running
- [:x:] Token not accepted

 
### InventoryFolders
 
http://localhost:8080/inventory/InventoryFolders/{token}
Method: Get
OR
InventoryFolders
OR
Requests the inventory folder layout as a json object InventoryMapFolder<br/>Formated as follows<br/>InventoryMapItem<br/><ul><li>id: UUID</li><li>name: String</li><li>subfolders: InventoryMapFolder[]</li></ul>
***Args helper***
 
- [:heavy_check_mark:] array of InventoryMapFolder
- [:heavy_check_mark:] Error
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] array of InventoryMapFolder
- [:heavy_check_mark:] Error
- [:x:] Token not accepted

 
### InventoryFoldersLimited
 
http://localhost:8080/inventory/InventoryFoldersLimited/{targetfolder}/{token}
Method: Get
OR
InventoryFoldersLimited|||{targetfolder}
OR
Requests folders limited to selected folder
***Args helper***
 
- [:heavy_check_mark:] single InventoryMapFolder
- [:heavy_check_mark:] Error
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] single InventoryMapFolder
- [:heavy_check_mark:] Error
- [:x:] Token not accepted

 
### InventoryContents
 
http://localhost:8080/inventory/InventoryContents/{folderUUID}/{token}
Method: Get
OR
InventoryContents|||{folderUUID}
OR
Requests the contents of a folder as an array of InventoryMapItem<br/>Formated as follows<br/>InventoryMapItem<br/><ul><li>id: UUID</li><li>name: String</li><li>typename: String</li></ul>
***Args helper***
 
- [:heavy_check_mark:] array of InventoryMapItem
- [:heavy_check_mark:] Invaild folder UUID
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] array of InventoryMapItem
- [:heavy_check_mark:] Invaild folder UUID
- [:x:] Token not accepted

 
## movement
 
### AutoPilot
 
http://localhost:8080/movement/AutoPilot/{x}/{y}/{z}/{token}
Method: Get
OR
AutoPilot|||{x}~#~{y}~#~{z}
OR
uses the AutoPilot to move to a location
***Args helper***
 
- [:heavy_check_mark:] Error Unable to AutoPilot to location
- [:heavy_check_mark:] ok
- [:x:] Convert to vector has failed
- [:x:] ?  value out of range 0-?
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Error Unable to AutoPilot to location
- [:heavy_check_mark:] ok
- [:x:] Convert to vector has failed
- [:x:] ?  value out of range 0-?
- [:x:] Token not accepted

 
### AutoPilotStop
 
http://localhost:8080/movement/AutoPilotStop/{token}
Method: Get
OR
AutoPilotStop
OR
Attempt to teleport to a new region
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Token not accepted

 
### SendTeleportLure
 
http://localhost:8080/movement/SendTeleportLure/{avatar}/{token}
Method: Get
OR
SendTeleportLure|||{avatar}
OR
Make the bot request the target avatar teleport to the bot
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Invaild avatar UUID
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Invaild avatar UUID
- [:x:] Token not accepted

 
### RequestTeleport
 
http://localhost:8080/movement/RequestTeleport/{avatar}/{token}
Method: Get
OR
RequestTeleport|||{avatar}
OR
Sends a teleport request (Move the bot to the avatar)
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Invaild avatar UUID
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Invaild avatar UUID
- [:x:] Token not accepted

 
### Fly
 
http://localhost:8080/movement/Fly/{mode}/{token}
Method: Get
OR
Fly|||{mode}
OR
Makes the bot fly (or not)
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Invaild mode
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Invaild mode
- [:x:] Token not accepted

 
### RotateToFaceVector
 
http://localhost:8080/movement/RotateToFaceVector/{token}
Method: Post
OR
RotateToFaceVector|||{vector}
OR
Rotates the bot to face a vector from its current location
***Args helper***
 
- [:heavy_check_mark:] true|false
- [:x:] Invaild vector
- [:x:] Vector ? value is out of range 0-?
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] true|false
- [:x:] Invaild vector
- [:x:] Vector ? value is out of range 0-?
- [:x:] Token not accepted

 
### RotateToFace
 
http://localhost:8080/movement/RotateToFace/{avatar}/{token}
Method: Post
OR
RotateToFace|||{avatar}
OR
Rotates the bot to face a avatar
***Args helper***
 
- [:heavy_check_mark:] true|false
- [:x:] Invaild avatar UUID
- [:x:] Unable to see avatar
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] true|false
- [:x:] Invaild avatar UUID
- [:x:] Unable to see avatar
- [:x:] Token not accepted

 
### RotateTo
 
http://localhost:8080/movement/RotateTo/{deg}/{token}
Method: Post
OR
RotateTo|||{deg}
OR
Rotates the avatar to face a rotation from north in Degrees
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Unable to process rotation
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Unable to process rotation
- [:x:] Token not accepted

 
### Teleport
 
http://localhost:8080/movement/Teleport/{region}/{x}/{y}/{z}/{token}
Method: Get
OR
Teleport|||{region}~#~{x}~#~{y}~#~{z}
OR
Attempt to teleport to a new region
***Args helper***
 
- [:heavy_check_mark:] Accepted
- [:x:] Error Unable to Teleport to location
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] Accepted
- [:x:] Error Unable to Teleport to location
- [:x:] Token not accepted

 
### TeleportSLURL
 
http://localhost:8080/movement/TeleportSLURL/{token}
Method: Post
OR
TeleportSLURL|||{slurl}
OR
Attempt to teleport to a new region via a SL url
***Args helper***
 
- [:heavy_check_mark:] True|False
- [:x:] slurl is empty
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] True|False
- [:x:] slurl is empty
- [:x:] Token not accepted

 
## notecard
 
### NotecardAdd
 
http://localhost:8080/notecard/NotecardAdd/{collection}/{token}
Method: Post
OR
NotecardAdd|||{collection}~#~{content}
OR
Adds content to the Collection<br/> Also creates the Collection if it does not exist
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Collection value is empty
- [:x:] Content value is empty
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Collection value is empty
- [:x:] Content value is empty
- [:x:] Token not accepted

 
### NotecardClear
 
http://localhost:8080/notecard/NotecardClear/{collection}/{token}
Method: Get
OR
NotecardClear|||{collection}
OR
Clears the contents of a collection
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Collection value is empty
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Collection value is empty
- [:x:] Token not accepted

 
### NotecardSend
 
http://localhost:8080/notecard/NotecardSend/{avatar}/{collection}/{notecardname}/{token}
Method: Get
OR
NotecardSend|||{avatar}~#~{collection}~#~{notecardname}
OR
Sends a notecard to a avatar using the text in the prebuilt collection [see NotecardAdd] and also clears the collection just before sending [see NotecardClear]
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Collection value is empty
- [:x:] Notecardname value is empty
- [:x:] Invaild avatar uuid
- [:x:] No content in notecard storage ?
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Collection value is empty
- [:x:] Notecardname value is empty
- [:x:] Invaild avatar uuid
- [:x:] No content in notecard storage ?
- [:x:] Token not accepted

 
### NotecardDirectSend
 
http://localhost:8080/notecard/NotecardDirectSend/{avatar}/{notecardname}/{token}
Method: Post
OR
NotecardDirectSend|||{avatar}~#~{content}~#~{notecardname}
OR
Creates and sends a notecard in one command good if you are using HTTP otherwise see [NotecardSend]
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] notecardname value is empty
- [:x:] Content value is empty
- [:x:] Invaild avatar uuid
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] notecardname value is empty
- [:x:] Content value is empty
- [:x:] Invaild avatar uuid
- [:x:] Token not accepted

 
## parcel
 
### SetParcelSale
 
http://localhost:8080/parcel/SetParcelSale/{amount}/{avatar}/{token}
Method: Get
OR
SetParcelSale|||{amount}~#~{avatar}
OR
Sets the current parcel for sale Also marks the parcel for sale
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Invaild amount
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Invaild amount
- [:x:] Token not accepted

 
### GetParcelTraffic
 
http://localhost:8080/parcel/GetParcelTraffic/{token}
Method: Get
OR
GetParcelTraffic
OR
Gets the parcel Dwell (Traffic) value and returns it via the reply target
***Args helper***
 
- [:heavy_check_mark:] traffic value
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] traffic value
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Token not accepted

 
### SetParcelLandingZone
 
http://localhost:8080/parcel/SetParcelLandingZone/{x}/{y}/{z}/{token}
Method: Get
OR
SetParcelLandingZone|||{x}~#~{y}~#~{z}
OR
Changes the parcel landing mode to point and sets the landing point
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Invaild amount
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Invaild amount
- [:x:] Token not accepted

 
### SetParcelName
 
http://localhost:8080/parcel/SetParcelName/{name}/{token}
Method: Get
OR
SetParcelName|||{name}
OR
Updates the current parcels name
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Parcel name is empty
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Parcel name is empty
- [:x:] Token not accepted

 
### SetParcelDesc
 
http://localhost:8080/parcel/SetParcelDesc/{token}
Method: Post
OR
SetParcelDesc|||{desc}
OR
Updates the current parcels description
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Token not accepted

 
### GetParcelDesc
 
http://localhost:8080/parcel/GetParcelDesc/{token}
Method: Get
OR
GetParcelDesc
OR
Fetchs the current parcels desc
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Token not accepted

 
### GetParcelFlags
 
http://localhost:8080/parcel/GetParcelFlags/{token}
Method: Get
OR
GetParcelFlags
OR
gets the flags for the parcel
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Token not accepted

 
### ParcelEject
 
http://localhost:8080/parcel/ParcelEject/{avatar}/{token}
Method: Get
OR
ParcelEject|||{avatar}
OR
Ejects an avatar
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Invaild avatar
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Invaild avatar
- [:x:] Token not accepted

 
### AbandonLand
 
http://localhost:8080/parcel/AbandonLand/{token}
Method: Get
OR
AbandonLand
OR
Abandons the parcel the bot is currently on, returning it to Linden's or Estate owner
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Token not accepted

 
### ParcelBan
 
http://localhost:8080/parcel/ParcelBan/{avatar}/{token}
Method: Get
OR
ParcelBan|||{avatar}
OR
Bans an avatar from a parcel
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Invaild avatar
- [:x:] Avatar is in the blacklist
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Invaild avatar
- [:x:] Avatar is in the blacklist
- [:x:] Token not accepted

 
### ParcelUnBan
 
http://localhost:8080/parcel/ParcelUnBan/{avatar}/{token}
Method: Get
OR
ParcelUnBan|||{avatar}
OR
Unbans an avatar from a parcel
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Invaild avatar
- [:x:] Avatar is already unbanned
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Invaild avatar
- [:x:] Avatar is already unbanned
- [:x:] Token not accepted

 
### SetParcelMusic
 
http://localhost:8080/parcel/SetParcelMusic/{musicurl}/{token}
Method: Get
OR
SetParcelMusic|||{musicurl}
OR
Updates the current parcels name
***Args helper***
 
- [:heavy_check_mark:] true|false
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] true|false
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Token not accepted

 
### SetParcelFlag
 
http://localhost:8080/parcel/SetParcelFlag/{token}
Method: Post
OR
SetParcelFlag|||{escapedflagdata}
OR
Updates the current parcels name
***Args helper***
 
- [:heavy_check_mark:] true|false
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Incorrect perms to control parcel
- [:x:] No accepted flags
- [:x:] Unable to set flag ...
- [:x:] Flag: ? is unknown
- [:x:] Flag: ? missing "="
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] true|false
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Incorrect perms to control parcel
- [:x:] No accepted flags
- [:x:] Unable to set flag ...
- [:x:] Flag: ? is unknown
- [:x:] Flag: ? missing "="
- [:x:] Token not accepted

 
### ParcelReturnTargeted
 
http://localhost:8080/parcel/ParcelReturnTargeted/{avatar}/{token}
Method: Get
OR
ParcelReturnTargeted|||{avatar}
OR
Returns all objects from the current parcel for the selected avatar
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Invaild avatar UUID
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Invaild avatar UUID
- [:x:] Token not accepted

 
### ParcelDeedToGroup
 
http://localhost:8080/parcel/ParcelDeedToGroup/{token}
Method: Get
OR
ParcelDeedToGroup
OR
transfers the current parcel ownership to the assigned group
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Invaild group uuid
- [:x:] Not in group
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Invaild group uuid
- [:x:] Not in group
- [:x:] Token not accepted

 
### ParcelBuy
 
http://localhost:8080/parcel/ParcelBuy/{amount}/{token}
Method: Get
OR
ParcelBuy|||{amount}
OR
Attempts to buy the parcel the bot is standing on, the amount must match the sale price for the land!
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Parcel not for sale
- [:x:] Parcel not for sale
- [:x:] Parcel sale locked to other avatars
- [:x:] Parcel sale price and amount do not match
- [:x:] Invaild amount
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Parcel not for sale
- [:x:] Parcel not for sale
- [:x:] Parcel sale locked to other avatars
- [:x:] Parcel sale price and amount do not match
- [:x:] Invaild amount
- [:x:] Token not accepted

 
### ParcelFreeze
 
http://localhost:8080/parcel/ParcelFreeze/{avatar}/{state}/{token}
Method: Get
OR
ParcelFreeze|||{avatar}~#~{state}
OR
Freezes an avatar
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Invaild avatar UUID
- [:x:] Invaild state
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Invaild avatar UUID
- [:x:] Invaild state
- [:x:] Token not accepted

 
### GetParcelBanlist
 
http://localhost:8080/parcel/GetParcelBanlist/{token}
Method: Get
OR
GetParcelBanlist
OR
Fetchs the parcel ban list of the parcel the bot is currently on<br/>If the name returned is lookup the bot is currently requesting the avatar name
***Args helper***
 
- [:x:] json object: GetParcelBanlistObject
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Token not accepted

***Replys***
 
- [:x:] json object: GetParcelBanlistObject
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Token not accepted

 
### UnRezObject
 
http://localhost:8080/parcel/UnRezObject/{objectuuid}/{token}
Method: Get
OR
UnRezObject|||{objectuuid}
OR
Returns a rezzed object
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Invaild object uuid
- [:x:] Unable to find object
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Invaild object uuid
- [:x:] Unable to find object
- [:x:] Token not accepted

 
### ParcelSetMedia
 
http://localhost:8080/parcel/ParcelSetMedia/{token}
Method: Get
OR
ParcelSetMedia|||{escapedflagdata}
OR
Updates the current parcels media settings 
MediaAutoScale=Bool (True|False)
MediaLoop=Bool (True|False)
MediaID=UUID (Texture)
MediaURL=String
MediaDesc=String
MediaHeight=Int (256 to 1024)
MediaWidth=Int (256 to 1024)
MediaType=String ["IMG-PNG","IMG-JPG","VID-MP4","VID-AVI" or "Custom-MIME_TYPE_CODE"]
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Error not in a sim
- [:x:] Parcel data not ready
- [:x:] Token not accepted

 
## self
 
### GoHome
 
http://localhost:8080/self/GoHome/{token}
Method: Get
OR
GoHome
OR
Makes the bot teleport to its home region
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Token not accepted

 
### PointAt
 
http://localhost:8080/self/PointAt/{token}
Method: Get
OR
PointAt
OR
Makes the bot turn to face avatar and point at them (if found)
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Cant find UUID in sim
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Cant find UUID in sim
- [:x:] Token not accepted

 
### ReadKeyValue
 
http://localhost:8080/self/ReadKeyValue/{Key}/{token}
Method: Get
OR
ReadKeyValue|||{Key}
OR
Reads a value from the KeyValue storage (temp unless SQL is enabled)
***Args helper***
 
- [:heavy_check_mark:] value
- [:x:] Unknown Key: KeyName
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] value
- [:x:] Unknown Key: KeyName
- [:x:] Token not accepted

 
### SetKeyValue
 
http://localhost:8080/self/SetKeyValue/{Key}/{token}
Method: Post
OR
SetKeyValue|||{Key}~#~{Value}
OR
sets a value for KeyValue storage (temp unless SQL is enabled)
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Key is empty
- [:x:] Value is empty
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Key is empty
- [:x:] Value is empty
- [:x:] Token not accepted

 
### ClearKeyValue
 
http://localhost:8080/self/ClearKeyValue/{Key}/{token}
Method: Get
OR
ClearKeyValue|||{Key}
OR
Reads a value from the KeyValue storage (temp unless SQL is enabled)
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Key is empty
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Key is empty
- [:x:] Token not accepted

 
### Sit
 
http://localhost:8080/self/Sit/{target}/{token}
Method: Get
OR
Sit|||{target}
OR
Makes the bot sit on the ground or on a object if it can see it
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Invaild object UUID
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Invaild object UUID
- [:x:] Token not accepted

 
### Stand
 
http://localhost:8080/self/Stand/{token}
Method: Get
OR
Stand
OR
Makes the bot stand up if sitting (also resets animations)
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Token not accepted

 
### ClickObject
 
http://localhost:8080/self/ClickObject/{target}/{token}
Method: Get
OR
ClickObject|||{target}
OR
Makes the bot sit on the ground or on a object if it can see it
***Args helper***
 
- [:heavy_check_mark:] true|false
- [:x:] Invaild object UUID
- [:x:] Unable to see object
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] true|false
- [:x:] Invaild object UUID
- [:x:] Unable to see object
- [:x:] Token not accepted

 
### Logoff
 
http://localhost:8080/self/Logoff/{token}
Method: Get
OR
Logoff
OR
Makes the bot kill itself you monster
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Token not accepted

 
### Logout
 
http://localhost:8080/self/Logout/{token}
Method: Get
OR
Logout
OR
Makes the bot kill itself you monster
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Token not accepted

 
### Bye
 
http://localhost:8080/self/Bye/{token}
Method: Get
OR
Bye
OR
Makes the bot kill itself you monster - without making a sound
***Args helper***
 
- [:heavy_check_mark:] ok
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] ok
- [:x:] Token not accepted

 
### GetLastCommands
 
http://localhost:8080/self/GetLastCommands/{token}
Method: Get
OR
GetLastCommands
OR
Gets the last 5 commands issued to the bot
***Args helper***
 
- [:heavy_check_mark:] list of commands
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] list of commands
- [:x:] Token not accepted

 
### SetPermFlag
 
http://localhost:8080/self/SetPermFlag/{avatar}/{flag}/{state}/{sticky}/{token}
Method: Get
OR
SetPermFlag|||{avatar}~#~{flag}~#~{state}~#~{sticky}
OR
Sets the bot to accept a request type from the avatar (or a object owned by the avatar)
 friend: friend request 
 group: group invite 
 animation: trigger animation request [from a object]
teleport: teleport lure
inventory: Inventory transfer
command: A non signed command
***Args helper***
 
- [:x:] avatar lookup
- [:x:] Invaild state
- [:x:] Invaild sticky
- [:x:] Invaild flag
- [:x:] Token not accepted

***Replys***
 
- [:x:] avatar lookup
- [:x:] Invaild state
- [:x:] Invaild sticky
- [:x:] Invaild flag
- [:x:] Token not accepted

 
## streamadmin
 
### FetchNextNotecard
 
http://localhost:8080/streamadmin/FetchNextNotecard/{token}
Method: Post
OR
FetchNextNotecard|||{endpoint}~#~{endpointcode}
OR
A streamadin command
***Args helper***
 
- [:heavy_check_mark:] True|False
- [:x:] Bad reply:  ...
- [:x:] Endpoint is empty
- [:x:] Endpointcode is empty
- [:x:] HTTP status code: ...
- [:x:] Error: ...
- [:x:] Notecard title is to short
- [:x:] Token not accepted

***Replys***
 
- [:heavy_check_mark:] True|False
- [:x:] Bad reply:  ...
- [:x:] Endpoint is empty
- [:x:] Endpointcode is empty
- [:x:] HTTP status code: ...
- [:x:] Error: ...
- [:x:] Notecard title is to short
- [:x:] Token not accepted

 
View as [HTML](https://wiki.blackatom.live/files/Commands.html)
 
Host it yourself: [+Docker image](https://hub.docker.com/r/madpeter/secondbot_wiki)
 
---
# Commands list

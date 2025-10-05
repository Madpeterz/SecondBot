#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["SecondBotEvents/SecondBotEvents.csproj", "SecondBotEvents/"]
COPY ["LibreMetaverse/LibreMetaverse.csproj", "LibreMetaverse/"]
COPY ["LibreMetaverse.StructuredData/LibreMetaverse.StructuredData.csproj", "LibreMetaverse.StructuredData/"]
COPY ["LibreMetaverse.Types/LibreMetaverse.Types.csproj", "LibreMetaverse.Types/"]
RUN dotnet restore "SecondBotEvents/SecondBotEvents.csproj"
COPY . .
WORKDIR "/src/SecondBotEvents"
RUN dotnet build "SecondBotEvents.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SecondBotEvents.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV basic_Username='' \
	basic_Password='' \
	basic_LoginURI='secondlife' \
	basic_LogCommands='true' \
	basic_DefaultHoverHeight=0.1 \
	commands_AllowFundsCommands='false' \
	commands_AllowIMcontrol='false' \
	commands_SharedSecret='examplecode' \
	commands_EnableMasterControls='true' \
	commands_MastersCSV='Madpeter Zond' \
	commands_CheckDotNames='false' \
	commands_EnforceTimeWindow='false' \
	commands_TimeWindowSecs=35 \
	commands_Enabled='true' \
	commands_ObjectMasterOptout='false' \
	commands_AllowServiceControl='false' \
	commands_CommandHistoryLogResults='false' \
	datastore_AutoCleanAvatars='true' \
	datastore_AvatarsCleanAfterMins=10 \
	datastore_LocalChatHistoryLimit=120 \
	datastore_PrefetchAvatarDisplaynames='true' \
	datastore_GroupChatHistoryLimitPerGroup=50 \
	datastore_ImChatHistoryLimit=50 \
	datastore_PrefetchGroupMembers='true' \
	datastore_PrefetchGroupRoles='false' \
	datastore_PrefetchInventory='false' \
	datastore_PrefetchInventoryDepth='2' \
	datastore_PrefetchEstateBanlist='true' \
	datastore_AutoCleanKeyValueStore='true' \
	datastore_CleanKeyValueStoreAfterMins=10 \
	datastore_CommandHistoryLimit=30 \
	discord_Enabled='false' \
	discord_ServerID='' \
	discord_ClientToken='' \
	discord_AllowDiscordCommands='false' \
	discord_hideChatterName='false' \
	discord_ClearChatOnConnect='true' \
	events_Enabled='false' \
	events_GroupMemberJoins='false' \
	events_GroupMemberLeaves='false' \
	events_GroupMemberEventsGroupUUID='' \
	events_GuestEntersArea='false' \
	events_GuestLeavesArea='false' \
	events_GuestTrackingSimname='' \
	events_GuestTrackingParcelname='' \
	events_SimAlertMessage='false' \
	events_StatusMessage='false' \
	events_MoneyEvent='false' \
	events_ChangeSim='false' \
	events_ChangeParcel='false' \
	events_OutputChannel=-1 \
	events_OutputIMuuid='00000000-0000-0000-0000-000000000000' \
	events_OutputHttpURL='none' \
	events_OutputSecret='notset' \
	homebound_Enabled='false' \
	homebound_HomeSimSlUrl='' \
	homebound_BackupSimSLUrl='' \
	homebound_AtHomeSeekLocation='false' \
	homebound_AtBackupSeekLocation='false' \
	homebound_AtHomeAutoSitUuid='00000000-0000-0000-0000-000000000000' \
	homebound_ReturnToHomeSimAfterMins=5 \
	http_Enabled='false' \
	http_Port=80 \
	http_DisableCommandValidation='false' \
	interaction_Enabled='true' \
	interaction_AcceptTeleports='false' \
	interaction_AcceptGroupInvites='false' \
	interaction_AcceptInventory='false' \
	interaction_AcceptFriendRequests='false' \
	interaction_EnableJsonOutputEvents='false' \
	interaction_JsonOutputEventsTarget='none' \
	interaction_FriendRequestLevel='Owner' \
	interaction_InventoryTransferLevel='Owner' \
	interaction_GroupInviteLevel='Owner' \
	interaction_TeleportRequestLevel='Owner' \
	interaction_EnableDebug='false' \
	commands_HideStatusOutput='false' \
	datastore_HideStatusOutput='false' \
	discord_HideStatusOutput='false' \
	events_HideStatusOutput='false' \
	homebound_HideStatusOutput='false' \
	http_HideStatusOutput='false' \
	interaction_HideStatusOutput='false' \
	rlv_Enabled='false' \
	rlv_HideStatusOutput='false' \
	relay_count='0' \
	relay_HideStatusOutput='false' \
	relay_UseShortEncoder='false' \
	chatgpt_Enabled='false' \
	chatgpt_FakeTypeDelay='true' \
        chatgpt_ApiKey='none' \
        chatgpt_OrganizationId='none' \
        chatgpt_AllowImReplys='false' \
        chatgpt_ImReplyFriendsOnly='true' \
        chatgpt_AllowGroupReplys='false' \
        chatgpt_GroupReplyForGroup='none' \
        chatgpt_LocalchatReply='false' \
        chatgpt_UseModel='gpt-3.5-turbo' \
        chatgpt_ChatHistoryMessages='5' \
	chatgpt_ChatPrompt='respond as if you are a horse that knows its going to the glue factory and you are upset about this fact' \
	chatgpt_Provider='openai' \
	chatgtp_ShowDebug='false' \
	chatgpt_ImReplyRateLimiter='3' \
	chatgpt_GroupReplyRateLimiter='3' \
	chatgpt_LocalchatRateLimiter='3' \
	chatgpt_ChatHistoryTimeout='15' \
	chatgpt_CustomName='<!FIRSTNAME!>' \
	chatgpt_UseRedis='false' \
	chatgpt_RedisSource='127.0.0.1:6379' \
	chatgpt_RedisPrefix='sbot' \
	chatgpt_RedisLocalchat='false' \
	chatgpt_RedisGroupchat='false' \
	chatgpt_RedisImchat='true' \
	chatgpt_RedisMaxageMins='120' \
	chatgpt_RedisCountLocal='60' \
	chatgpt_RedisCountGroup='60' \
	chatgpt_RedisCountIm='60' \
	onevent_Enabled='false' \
	onevent_Count=0 \
	onevent_WaitSecsToStart=10 \
	onevent_HideStatusOutput='false' \
	smtp_Enabled='false' \
	smtp_AllowEmailAsReplyTarget='false' \
	smtp_AllowCommandSendMail='false' \
	smtp_UseAllowedRecivers='false' \
	smtp_AllowedReciversCSV='none' \
	smtp_AllowSendAlertStatus='false' \
	smtp_AllowSendLoginNotice='false' \
	smtp_MailReplyAddress='me@myemail.tld' \
	smtp_Port='587' \
	smtp_Host='smtp.mail.example' \
	smtp_useSSL='false' \
	smtp_User='me@email.addr.tld' \
	smtp_Token='none' \
	smtp_SendAlertsAndLoginsTo='me@somewhere.tld' \
	smtp_HideStatusOutput='false' \
	rabbit_Enabled='false' \
	rabbit_HideStatusOutput='false' \
	rabbit_HostIP='127.0.0.1' \
	rabbit_HostUsername='guest' \
	rabbit_HostPassword='guest' \
	rabbit_HostPort='5672' \
	rabbit_NotecardQueue='notecards' \
	rabbit_CommandQueue='commands' \
	rabbit_ImQueue='ims' \
	rabbit_GroupImQueue='groupims' \
	rabbit_LogDebug='true'

EXPOSE 80
ENV ASPNETCORE_URLS http://+:80

ENTRYPOINT ["dotnet", "SecondBotEvents.dll"]

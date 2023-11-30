#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
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
	commands_AllowFundsCommands='false' \
	commands_AllowIMcontrol='false' \
	commands_SharedSecret='examplecode' \
	commands_EnableMasterControls='true' \
	commands_MastersCSV='Madpeter Zond' \
	commands_EnforceTimeWindow='false' \
	commands_TimeWindowSecs=35 \
	commands_Enabled='true' \
	datastore_AutoCleanAvatars='true' \
	datastore_AvatarsCleanAfterMins=10 \
	datastore_LocalChatHistoryLimit=120 \
	datastore_GroupChatHistoryLimitPerGroup=50 \
	datastore_ImChatHistoryLimit=50 \
	datastore_PrefetchGroupMembers='true' \
	datastore_PrefetchGroupRoles='false' \
	datastore_PrefetchEstateBanlist='true' \
	datastore_AutoCleanKeyValueStore='true' \
	datastore_CleanKeyValueStoreAfterMins=10 \
	datastore_CommandHistoryLimit=30 \
	discord_Enabled='false' \
	discord_ServerID='' \
	discord_ClientToken='' \
	discord_AllowDiscordCommands='false' \
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
	http_Enabled='false' \
	http_Port=80 \
	interaction_Enabled='true' \
	interaction_AcceptTeleports='false' \
	interaction_AcceptGroupInvites='false' \
	interaction_AcceptInventory='false' \
	interaction_AcceptFriendRequests='false' \
	interaction_AcceptFromMasterOnly='true' \
	interaction_EnableJsonOutputEvents='false' \
	interaction_JsonOutputEventsTarget='none'

EXPOSE 80
ENV ASPNETCORE_URLS http://+:80

ENTRYPOINT ["dotnet", "SecondBotEvents.dll"]
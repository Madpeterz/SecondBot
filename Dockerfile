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
	commands_AllowFundsCommands='false' \
	commands_SharedSecret='examplecode' \
	commands_MastersCSV='Madpeter Zond' \
	homebound_Enabled='false' \
	homebound_HomeSimSlUrl='' \
	homebound_BackupSimSLUrl='' \
	homebound_AtHomeAutoSitUuid='00000000-0000-0000-0000-000000000000' \
	interaction_FriendRequestLevel='Owner' \
	interaction_InventoryTransferLevel='Owner' \
	interaction_GroupInviteLevel=Owner'' \
	interaction_TeleportRequestLevel='Owner' \
	chatgpt_Enabled='false' \
        chatgpt_ApiKey='none' \
        chatgpt_OrganizationId='none' \
        chatgpt_AllowImReplys='false' \
        chatgpt_ImReplyFriendsOnly='true' \
        chatgpt_ImReplyRateLimiter='3' \
        chatgpt_AllowGroupReplys='false' \
        chatgpt_GroupReplyForGroup='none' \
        chatgpt_GroupReplyRateLimiter='3' \
        chatgpt_LocalchatReply='false' \
        chatgpt_LocalchatRateLimiter='3' \
        chatgpt_UseModel='gpt-3.5-turbo' \
        chatgpt_ChatHistoryMessages='5' \
	chatgpt_ChatPrompt='respond as if you are a horse that knows its going to the glue factory and you are upset about this fact' \
	chatgpt_Provider='openai' \
	chatgpt_ChatHistoryTimeout='15'
	

EXPOSE 80
ENV ASPNETCORE_URLS http://+:80

ENTRYPOINT ["dotnet", "SecondBotEvents.dll"]
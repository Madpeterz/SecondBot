#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["BetterSecondbot/BetterSecondbot.csproj", "BetterSecondbot/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["LibreMetaverse/LibreMetaverse.csproj", "LibreMetaverse/"]
COPY ["LibreMetaverse.StructuredData/LibreMetaverse.StructuredData.csproj", "LibreMetaverse.StructuredData/"]
COPY ["LibreMetaverse.Types/LibreMetaverse.Types.csproj", "LibreMetaverse.Types/"]
RUN dotnet restore "BetterSecondbot/BetterSecondbot.csproj"
COPY . .
WORKDIR "/src/BetterSecondbot"
RUN dotnet build "BetterSecondbot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BetterSecondbot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# --- Update your settings ---

ENV Basic_BotUserName='' \
	Basic_BotPassword='' \
	Basic_HomeRegions='Viserion/50/140/23' \
	Basic_AvoidRestartRegions='Just the Tip/60/96/1490' \
	Basic_LoginLocation='home' \
	Security_MasterUsername='' \
	Security_SubMasters='' \
	Security_SignedCommandkey='' \
	Security_WebUIKey='' \
	Setting_AllowFunds='false' \
	Setting_loginURI='secondlife' \
	Setting_Tracker='Event' \
	DiscordFull_Enable='false' \
	DiscordFull_Token='' \
	DiscordFull_ServerID='' \
	DiscordFull_Keep_GroupChat='false' \
	Http_Enable='false' \
	Http_Host='docker' \
	Log2File_Enable='false' \
	Log2File_Level='1' \
	Setting_LogCommands='false' \
	Agent_Channel='auto' \
	Agent_Version='auto' \
	OBJECT_CHAT='false' \
	SQLDB_Enabled='false' \
	SQLDB_Host='127.0.0.1' \
	SQLDB_Port='3306' \
	SQLDB_Username='' \
	SQLDB_Password='' \
	SQLDB_Timeout='3' \
	SQLDB_Database='secondbot'

EXPOSE 80
ENV ASPNETCORE_URLS http://+:80

# --- End of settings ---

ENTRYPOINT ["/app/BetterSecondbot"]
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
COPY ["LibreMetaverseTypes/LibreMetaverse.Types.csproj", "LibreMetaverseTypes/"]
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

ENV Basic_BotUserName=''
ENV Basic_BotPassword=''
ENV Basic_HomeRegions='Viserion/50/140/23'
ENV Basic_AvoidRestartRegions='Just the Tip/60/96/1490'
ENV Basic_LoginLocation='home'
ENV Security_MasterUsername=''
ENV Security_SubMasters=''
ENV Security_SignedCommandkey=''
ENV Security_WebUIKey=''
ENV Setting_AllowFunds='false'
ENV Setting_loginURI='secondlife'
ENV Setting_Tracker='Event'
ENV DiscordFull_Enable='false'
ENV DiscordFull_Token=''
ENV DiscordFull_ServerID=''
ENV DiscordFull_Keep_GroupChat='false'
ENV Http_Enable='false'
ENV Http_Host='docker'
ENV Log2File_Enable='false'
ENV Log2File_Level='1'
ENV Setting_LogCommands='false'
ENV Agent_Channel='auto'
ENV Agent_Version='auto'
ENV SQLDB_Enabled='false'
ENV SQLDB_Host='127.0.0.1'
ENV SQLDB_Port='3306'
ENV SQLDB_Username=''
ENV SQLDB_Password=''
ENV SQLDB_Timeout='3'
ENV SQLDB_Database='secondbot'

EXPOSE 80
ENV ASPNETCORE_URLS http://+:80

# --- End of settings ---

ENTRYPOINT ["/app/BetterSecondbot"]
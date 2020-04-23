#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["CLI/BetterSecondBot.csproj", "CLI/"]
COPY ["Core/BSB.csproj", "Core/"]
COPY ["BSBshared/BSBshared.csproj", "BSBshared/"]
COPY ["libremetaverse-core/Libremetaversetypes/LibreMetaverse.Types.csproj", "libremetaverse-core/Libremetaversetypes/"]
COPY ["libremetaverse-core/Libremetaverse/LibreMetaverse.csproj", "libremetaverse-core/Libremetaverse/"]
COPY ["libremetaverse-core/Libremetaverse.structureddata/LibreMetaverse.StructuredData.csproj", "libremetaverse-core/Libremetaverse.structureddata/"]
RUN dotnet restore "CLI/BetterSecondBot.csproj"
COPY . .
WORKDIR "/src/CLI"
RUN dotnet build "BetterSecondBot.csproj" -c DockerBuild -o /app/build

FROM build AS publish
RUN dotnet publish "BetterSecondBot.csproj" -c DockerBuild -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV BotRunningInDocker='REQUIRED'
ENV userName='Firstname LastName'
ENV password='PassWord'
ENV master='Madpeter Zond'
ENV code='DontTellAnyoneThis'
ENV discord='https://discordapp.com/api/webhooks/XXXX/YYYY'
ENV discordGroupTarget='00000000-0000-0000-0000-000000000000'
ENV allowRLV='false'
ENV homeRegion='"SLurl1","SLurl2"'
ENV EnableHttp='false'
ENV Httpport='80'
ENV Httpkey='ThisIsAKeyYo'
ENV HttpRequireSigned='false'
ENV HttpHost='http://localhost'
ENV HttpAsCnC='false'
ENV DiscordFullServer='false'
ENV DiscordClientToken='https://discordapp.com/developers/ Bot'
ENV DiscordServerID='1234567890'
ENV DiscordServerImHistoryHours='24'
ENV DefaultSitUUID='00000000-0000-0000-0000-000000000000'

EXPOSE 8080
ENV ASPNETCORE_URLS http://+:8080

ENTRYPOINT ["/app/BetterSecondBot"]

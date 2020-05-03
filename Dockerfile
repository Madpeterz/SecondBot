#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["CLI/BetterSecondBot.csproj", "CLI/"]
COPY ["Core/BSB.csproj", "Core/"]
COPY ["BSBshared/BSBshared.csproj", "BSBshared/"]
RUN dotnet restore "CLI/BetterSecondBot.csproj"
COPY . .
WORKDIR "/src/CLI"
RUN dotnet build "BetterSecondBot.csproj" -c DockerBuild -o /app/build

FROM build AS publish
RUN dotnet publish "BetterSecondBot.csproj" -c DockerBuild -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# --- Update your settings ---

# Required
ENV userName=''
ENV password=''
ENV master=''
ENV code=''

# Mixed
ENV allowRLV='False'
ENV DefaultSitUUID='00000000-0000-0000-0000-000000000000'
ENV CommandsToConsole='True'
ENV MaxCommandHistory='250'
ENV RelayImToAvatar='00000000-0000-0000-0000-000000000000'

# Discord
ENV discordWebhookURL=''
ENV discordGroupTarget='00000000-0000-0000-0000-000000000000'
ENV DiscordFullServer='False'
ENV DiscordClientToken=''
ENV DiscordServerID=''
ENV DiscordServerImHistoryHours='24'

# @home
ENV homeRegion=''
ENV AtHomeSimOnly='False'
ENV AtHomeSimPosMaxRange='10.0'

# HTTPinterface
ENV EnableHttp='False'
ENV HttpPublicUrlBase='http://localhost'
ENV Httpport='8080'
ENV Httpkey=''
ENV HttpHost='http://localhost'
ENV HttpAsCnC='False'

# --- End of settings ---

ENV BotRunningInDocker='REQUIRED'
EXPOSE 8080
ENV ASPNETCORE_URLS http://+:8080

ENTRYPOINT ["/app/BetterSecondBot"]

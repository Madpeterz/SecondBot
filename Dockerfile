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
	commands_SharedSecret='examplecode' \
	commands_MastersCSV='Madpeter Zond'
	

EXPOSE 80
ENV ASPNETCORE_URLS http://+:80

ENTRYPOINT ["dotnet", "SecondBotEvents.dll"]
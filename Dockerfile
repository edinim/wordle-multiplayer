FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5096

ENV ASPNETCORE_URLS=http://+:5096

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["wordle-multiplayer.csproj", "./"]
RUN dotnet restore "wordle-multiplayer.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "wordle-multiplayer.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "wordle-multiplayer.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "wordle-multiplayer.dll"]

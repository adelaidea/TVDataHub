#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/TVDataHub.Api/TVDataHub.Api.csproj", "TVDataHub.Api/"]
COPY ["src/TVDataHub.DataAccess/TVDataHub.DataAccess.csproj", "TVDataHub.DataAccess/"]
COPY ["src/TVDataHub.Domain/TVDataHub.Domain.csproj", "TVDataHub.Domain/"]
RUN dotnet restore "TVDataHub.Api/TVDataHub.Api.csproj"
COPY . .

WORKDIR /src
RUN dotnet build "src/TVDataHub.Api/TVDataHub.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/TVDataHub.Api/TVDataHub.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TVDataHub.Api.dll"]

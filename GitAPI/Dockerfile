# Dockerfile for Sky.GitAPI
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["GitAPI/Sky.GitAPI.csproj", "GitAPI/"]
COPY ["Common/Cosmos.Common.csproj", "Common/"]
COPY ["AspNetCore.Identity.FlexDb/AspNetCore.Identity.FlexDb.csproj", "AspNetCore.Identity.FlexDb/"]
RUN dotnet restore "GitAPI/Sky.GitAPI.csproj"
COPY . .
WORKDIR "/src/GitAPI"
RUN dotnet build "Sky.GitAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sky.GitAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sky.GitAPI.dll"]
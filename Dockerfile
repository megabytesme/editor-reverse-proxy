FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

COPY src/EditorReverseProxy/EditorReverseProxy.csproj src/EditorReverseProxy/
WORKDIR /app/src/EditorReverseProxy
RUN dotnet restore

WORKDIR /app
COPY src/. ./src
RUN dotnet publish src/EditorReverseProxy/EditorReverseProxy.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "EditorReverseProxy.dll"]

EXPOSE 80

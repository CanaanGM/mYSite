FROM mcr.microsoft.com/dotnet/sdk:7.0 as build

WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY Scarlet-Site.sln Scarlet-Site.sln
COPY src/API/API.csproj src/API/API.csproj
COPY src/Application/Application.csproj src/Application/Application.csproj
COPY src/CLI/CLI.csproj src/CLI/CLI.csproj
COPY src/DataAccess/DataAccess.csproj src/DataAccess/DataAccess.csproj

RUN dotnet restore Scarlet-Site.sln

COPY src/API src/API
COPY src/CLI src/CLI
COPY src/Application src/Application
COPY src/DataAccess src/DataAccess

WORKDIR /app/src/API
RUN dotnet publish -c Release -o out API.csproj


# Build runtime image

FROM mcr.microsoft.com/dotnet/aspnet:7.0 

WORKDIR /app
COPY --from=build /app/src/API/out .
ENTRYPOINT [ "dotnet", "API.dll" ]

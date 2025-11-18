# Base image cho runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Cài thư viện Linux cần thiết (LibreOffice + font + unoconv)
RUN apt-get update && \
    apt-get install -y \
        libfontconfig1 \
        libfreetype6 \
        libreoffice \
        unoconv \
        python3 \
        fonts-dejavu \
        fontconfig && \
    rm -rf /var/lib/apt/lists/*

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .

RUN dotnet restore "CoreAdminWeb.sln"

COPY . .

WORKDIR /src/CoreAdminWeb
RUN dotnet build "CoreAdminWeb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CoreAdminWeb.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "CoreAdminWeb.dll"]

# Base image cho runtime
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Cài thư viện Linux cần thiết (an toàn cho SkiaSharp nếu còn)
RUN apt-get update && apt-get install -y libfontconfig1 libfreetype6 && rm -rf /var/lib/apt/lists/*

# Build stage
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .

# Clean và restore để không còn DLL cũ
RUN dotnet clean
RUN dotnet restore "CoreAdminWeb.sln"

WORKDIR /src/CoreAdminWeb
RUN dotnet build "CoreAdminWeb.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "CoreAdminWeb.csproj" -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "CoreAdminWeb.dll"]

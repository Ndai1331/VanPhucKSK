FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "CoreAdminWeb.sln"
WORKDIR /src/CoreAdminWeb
RUN dotnet build "CoreAdminWeb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CoreAdminWeb.csproj" -c Release -o /app/publish

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoreAdminWeb.dll"] 



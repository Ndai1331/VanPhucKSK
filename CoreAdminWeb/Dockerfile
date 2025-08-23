FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:9.0 AS base

# Install comprehensive native dependencies for SkiaSharp, MudBlazor, and EPPlus
# These packages are required to prevent "The type initializer for 'SkiaSharp.SKData' threw an exception"
RUN apt-get update && apt-get install -y \
    libfontconfig1 \
    libfreetype6 \
    libpng16-16 \
    libjpeg62-turbo \
    libgif7 \
    libharfbuzz0b \
    libfribidi0 \
    uuid-runtime \
    libc6-dev \
    libgcc-s1 \
    libstdc++6 \
    libx11-6 \
    libxext6 \
    libxrender1 \
    libxrandr2 \
    libxss1 \
    libxtst6 \
    libxi6 \
    libgl1-mesa-glx \
    libglu1-mesa \
    libgles2-mesa \
    libegl1-mesa \
    && rm -rf /var/lib/apt/lists/*

# Set environment variables for SkiaSharp
ENV SKIA_SHARP_SKIA_LIBRARY_PATH=/usr/lib/x86_64-linux-gnu
ENV SKIA_SHARP_SKIA_LIBRARY_NAME=libskia.so
ENV DISPLAY=:99


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

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoreAdminWeb.dll"] 



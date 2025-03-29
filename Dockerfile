FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5200

# Instalar dependencias necesarias para SkiaSharp y utilidades de red
RUN apt-get update && apt-get install -y \
    libfontconfig1 \
    libfreetype6 \
    iputils-ping \
    wget \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["SchoolTreasureAPI.csproj", "./"]
RUN dotnet restore "SchoolTreasureAPI.csproj"
COPY . .
RUN dotnet build "SchoolTreasureAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SchoolTreasureAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SchoolTreasureAPI.dll"]
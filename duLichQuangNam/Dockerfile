# Base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 7270

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["duLichQuangNam/duLichQuangNam.csproj", "duLichQuangNam/"]
RUN dotnet restore "./duLichQuangNam/duLichQuangNam.csproj"
COPY . .
WORKDIR "/src/duLichQuangNam"
RUN dotnet build "./duLichQuangNam.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./duLichQuangNam.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "duLichQuangNam.dll"]

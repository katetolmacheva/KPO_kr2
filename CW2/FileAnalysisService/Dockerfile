FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY FileStoringService/ FileStoringService/
COPY FileAnalysisService/ FileAnalysisService/

RUN dotnet restore FileAnalysisService/FileAnalysisService.csproj

RUN dotnet build FileAnalysisService/FileAnalysisService.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/build

RUN rm FileStoringService/appsettings.json

FROM build AS publish
RUN dotnet publish FileAnalysisService/FileAnalysisService.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p wwwroot
ENTRYPOINT ["dotnet", "FileAnalysisService.dll"]

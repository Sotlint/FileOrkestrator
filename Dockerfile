# https://hub.docker.com/_/microsoft-dotnet
FROM  mcr.microsoft.com/dotnet/sdk:10.0-alpine AS prepare-restore-build-app
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet tool install --global --no-cache dotnet-subset --version 0.3.2

WORKDIR /app
COPY . .
RUN dotnet subset restore FileOrkestrator.Host/FileOrkestrator.Host.csproj --root-directory /app --output restore_subset/

# BACKEND BUILD STAGE
FROM  mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build-app
ARG CONFIGURATION=Release
WORKDIR /app

# NUGET CONFIG
COPY . .
RUN dotnet restore "FileOrkestrator.Host/FileOrkestrator.Host.csproj"
RUN dotnet publish "FileOrkestrator.Host/FileOrkestrator.Host.csproj" -c ${CONFIGURATION} -o out --no-restore

# RUNTIME STAGE
FROM  mcr.microsoft.com/dotnet/aspnet:10.0-alpine
ARG ASPNETCORE_ENVIRONMENT=Production

# Добавляем поддержку Culture - ru-RU
# https://github.com/dotnet/dotnet-docker/blob/main/samples/aspnetapp/Dockerfile.alpine-icu
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false DOTNET_SYSTEM_GLOBALIZATION_USENLS=true LC_ALL=ru_RU.UTF-8 LANG=ru_RU.UTF-8
RUN apk add --no-cache \
    icu-data-full \
    icu-libs \
    fontconfig \
    ttf-dejavu \
    libx11

WORKDIR /app
COPY --from=build-app /app/out ./

ENV ASPNETCORE_URLS="http://+:8080"
ENV ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}

# https://github.com/dotnet/runtime/issues/80641
RUN sed -i 's/providers = provider_sect/providers = provider_sect\n\
ssl_conf = ssl_sect\n\
\n\
[ssl_sect]\n\
system_default = system_default_sect\n\
\n\
[system_default_sect]\n\
Options = UnsafeLegacyRenegotiation/' /etc/ssl/openssl.cnf

EXPOSE 8080
# Среда задаётся переменной ASPNETCORE_ENVIRONMENT (см. docker-compose / оркестратор).
CMD ["dotnet", "FileOrkestrator.Host.dll"]

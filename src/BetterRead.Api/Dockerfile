ARG DotnetVersion=5.0
FROM mcr.microsoft.com/dotnet/sdk:$DotnetVersion AS build
WORKDIR /app

# Copy fsproj files
COPY src/*/*.fsproj ./
RUN for file in $(ls *.fsproj); do \
        echo $file \
        && mkdir -p src/${file%.*}/ \
        && mv $file src/${file%.*}/; \
    done

# Restore api fsproj
RUN dotnet restore ./src/BetterRead.Api/BetterRead.Api.fsproj

# Copy everything else and build
COPY . ./

ARG CI_BUILDID
ARG CI_PRERELEASE

ENV CI_BUILDID ${CI_BUILDID}
ENV CI_PRERELEASE ${CI_PRERELEASE}

# Publish to out folder
RUN dotnet publish ./src/BetterRead.Api/BetterRead.Api.fsproj -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:$DotnetVersion AS base

WORKDIR /app
COPY --from=build /app/out .

# Container healthcheck
HEALTHCHECK --interval=30s --timeout=30s --retries=3 \
   CMD curl --silent --fail http://localhost:8080/_health || exit 1

EXPOSE 8080/tcp
ENTRYPOINT ["dotnet", "BetterRead.Api.dll"]
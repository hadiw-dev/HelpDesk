# syntax=docker/dockerfile:1

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first (and only these) so `dotnet restore` is cached in its own layer —
# source-code-only changes below won't invalidate this layer and force a full package re-download.
COPY src/HelpDesk.Domain/HelpDesk.Domain.csproj src/HelpDesk.Domain/
COPY src/HelpDesk.Application/HelpDesk.Application.csproj src/HelpDesk.Application/
COPY src/HelpDesk.Infrastructure/HelpDesk.Infrastructure.csproj src/HelpDesk.Infrastructure/
COPY src/HelpDesk.Api/HelpDesk.Api.csproj src/HelpDesk.Api/

RUN dotnet restore src/HelpDesk.Api/HelpDesk.Api.csproj

# Only the runtime source tree is needed to publish the Api project and its Domain/Application/
# Infrastructure dependencies — the test projects are intentionally never copied into this image.
COPY src/ src/

RUN dotnet publish src/HelpDesk.Api/HelpDesk.Api.csproj -c Release -o /app/publish --no-restore

# ---- Final (runtime) stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
ARG APP_UID=1654
WORKDIR /app

# curl is not present in the slim runtime image by default; needed for the container HEALTHCHECK
# below (and for docker-compose's `condition: service_healthy` on this service).
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Pre-create the directories this app writes to at runtime (uploaded attachments, Serilog file
# sink) and open them up before switching to the non-root user below — a root-owned COPY leaves
# /app read-only to $APP_UID otherwise, and both LocalFileStorageService and Serilog's file sink
# would fail to create files in a fresh container.
RUN mkdir -p /app/file-storage /app/Logs && chmod -R 777 /app/file-storage /app/Logs

COPY --from=build /app/publish .

USER $APP_UID

HEALTHCHECK --interval=10s --timeout=5s --start-period=20s --retries=5 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "HelpDesk.Api.dll"]

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY MF.Express.Bot.sln ./
COPY MF.Express.Bot/MF.Express.Bot.csproj MF.Express.Bot/
COPY MF.Express.Bot.Application/MF.Express.Bot.Application.csproj MF.Express.Bot.Application/
COPY MF.Express.Bot.Infrastructure/MF.Express.Bot.Infrastructure.csproj MF.Express.Bot.Infrastructure/

RUN dotnet restore

COPY . .
WORKDIR /src/MF.Express.Bot
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
   curl \
   && apt-get clean && rm -rf /var/lib/apt/lists/*

RUN adduser --disabled-password --home /app --gecos '' appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

RUN mkdir -p Logs

# ====================================
# Environment Variables Configuration
# ====================================
# The following environment variables can be set at runtime:
#
# REQUIRED:
# - EXPRESSBOT__BOTID               : Bot ID from BotX platform
#                                     Example: ef45fc05-5425-56fd-ae2b-d60b82b9827a
# - EXPRESSBOT__BOTSECRETKEY        : Bot secret key from BotX platform
#                                     Example: 9d993361696617fad8c7a7aa5342cd1b
# - EXPRESSBOT__BOTXAPIBASEURL      : BotX API base URL
#                                     Example: https://express.multifactor.dev
# - EXPRESSBOT__EXPECTEDISSUER      : Expected JWT token issuer
#                                     Example: express.multifactor.dev
# - MFEXPRESSAPI__BASEURL           : MultiFactor Express API base URL
#                                     Example: https://mf-express-service.ru.tuna.am
#
# OPTIONAL:
# - ASPNETCORE_ENVIRONMENT          : Environment name (Local, Development, Production)
#                                     Production: JSON logs to stdout for ELK/Loki
#                                     Development/Local: Human-readable logs + file
# - EXPRESSBOT__REQUESTTIMEOUTSECONDS : Request timeout in seconds (default: 30)
# - EXPRESSBOT__PROTOVERSION          : Protocol version (default: 4)
# - EXPRESSBOT__ENABLEDETAILEDLOGGING : Enable detailed logging (default: false)
# - MFEXPRESSAPI__CHATCREATEDENDPOINT : Chat created webhook endpoint (default: /api/express/webhook/chat-created)
# - MFEXPRESSAPI__AUTHCALLBACKENDPOINT : Auth callback webhook endpoint (default: /api/express/webhook/auth-callback)
# - MFEXPRESSAPI__REQUESTTIMEOUTSECONDS : MF Express API request timeout (default: 30)
#
# LOGGING:
# - Production: Only Warning and Error levels to console + files in /app/Logs
#   Format: 2025-10-29 06:30:54.622 +00:00 [WRN] Message
#   Retention: 14 days, daily rotation
# - Development/Local: All levels (Debug+) to console + files in /app/Logs
#   Format: [06:30:54 DBG] SourceContext - Message
#   Retention: 3 days, daily rotation
#   Note: Mount volume for logs: -v /host/logs:/app/Logs
#
# For docker-compose, use .env file
# For Kubernetes, use ConfigMap/Secret
# For direct docker run, use -e flags
# ====================================

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "MF.Express.Bot.dll"]


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY src/MF.Express.Bot.sln ./src/
COPY src/global.json ./src/
COPY src/MF.Express.Bot/*.csproj ./src/MF.Express.Bot/
COPY src/MF.Express.Bot.Application/*.csproj ./src/MF.Express.Bot.Application/
COPY src/MF.Express.Bot.Infrastructure/*.csproj ./src/MF.Express.Bot.Infrastructure/

RUN dotnet restore "src/MF.Express.Bot.sln"

COPY src/ ./src/

WORKDIR /app/src/MF.Express.Bot
RUN dotnet publish -c Release -o /app/out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

RUN adduser --disabled-password --home /app --gecos '' appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/out .

RUN mkdir -p Logs

EXPOSE 8080

#ENV ASPNETCORE_URLS=http://+:8080
#ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "MF.Express.Bot.dll"]


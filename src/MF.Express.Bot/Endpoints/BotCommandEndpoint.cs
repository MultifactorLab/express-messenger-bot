using MF.Express.Bot.Application.Commands;
using MF.Express.Bot.Application.DTOs;
using Microsoft.Extensions.Options;
using MF.Express.Bot.Infrastructure.Configuration;

namespace MF.Express.Bot.Api.Endpoints;

/// <summary>
/// Bot API v4 endpoint для обработки команд от BotX
/// Согласно документации https://docs.express.ms/chatbots/developer-guide/api/bot-api/command/
/// </summary>
public class BotCommandEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/command", HandleAsync)
            .WithName("HandleBotXCommand")
            .Produces<BotApiResponse>(202)
            .Produces<BotApiErrorResponse>(503);
    }

    private static async Task<IResult> HandleAsync(
        BotXCommandDto command,
        ICommand<ProcessBotXCommandCommand, BotApiResponse> handler,
        IOptions<ExpressBotConfiguration> config,
        ILogger<BotCommandEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            if (command.BotId != config.Value.BotId)
            {
                logger.LogWarning("Получена команда для другого бота: {CommandBotId}, ожидался: {ConfigBotId}", 
                    command.BotId, config.Value.BotId);
                
                return Results.BadRequest(new { error = "Invalid bot_id" });
            }

            if (config.Value.EnableDetailedLogging)
            {
                logger.LogInformation("Получена команда Bot API v4: {CommandType} {Body} от пользователя {UserHuid} в чате {ChatId}",
                    command.Command.CommandType, 
                    command.Command.Body,
                    command.From.UserHuid ?? "system",
                    command.From.GroupChatId ?? "private");
            }

            var processingCommand = new ProcessBotXCommandCommand(
                SyncId: command.SyncId,
                SourceSyncId: command.SourceSyncId,
                CommandType: command.Command.CommandType,
                CommandBody: command.Command.Body,
                CommandData: command.Command.Data,
                CommandMetadata: command.Command.Metadata,
                UserHuid: command.From.UserHuid,
                GroupChatId: command.From.GroupChatId,
                ChatType: command.From.ChatType,
                Username: command.From.Username,
                AdLogin: command.From.AdLogin,
                AdDomain: command.From.AdDomain,
                IsAdmin: command.From.IsAdmin,
                IsCreator: command.From.IsCreator,
                Device: command.From.Device,
                DeviceSoftware: command.From.DeviceSoftware,
                Platform: command.From.Platform,
                AppVersion: command.From.AppVersion,
                Locale: command.From.Locale,
                Host: command.From.Host,
                BotId: command.BotId,
                ProtoVersion: command.ProtoVersion
            );

            var result = await handler.Handle(processingCommand, ct);

            logger.LogDebug("Команда Bot API v4 успешно обработана: {SyncId}", command.SyncId);

            return Results.Json(result, statusCode: 202);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обработке команды Bot API v4: {SyncId}", command.SyncId);
            return Results.Json(new BotApiResponse(), statusCode: 202);
        }
    }
}


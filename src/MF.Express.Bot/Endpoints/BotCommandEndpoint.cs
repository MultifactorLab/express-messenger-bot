using MF.Express.Bot.Application.Commands;
using MF.Express.Bot.Api.DTOs.BotCommand;
using MF.Express.Bot.Api.DTOs.Common;
using Microsoft.Extensions.Options;
using MF.Express.Bot.Infrastructure.Configuration;

namespace MF.Express.Bot.Api.Endpoints;

/// <summary>
/// Bot API v4 endpoint для обработки команд от BotX
/// https://docs.express.ms/chatbots/developer-guide/api/bot-api/command/
/// </summary>
public class BotCommandEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/command", HandleAsync)
            .WithName("HandleBotXCommand")
            .Produces<BotApiResponseDto>(202)
            .Produces<BotApiErrorResponseDto>(503);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        BotCommandDto dto,
        ICommand<ProcessBotXCommandCommand, Application.Models.BotCommand.BotApiResponseAppModel> handler,
        IOptions<ExpressBotConfiguration> config,
        ILogger<BotCommandEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            var audienceClaim = httpContext.User.FindFirst("aud")?.Value;
            if (audienceClaim != dto.BotId)
            {
                logger.LogWarning(
                    "Security violation: JWT audience mismatch. JWT aud={JwtAudience}, body bot_id={BodyBotId}",
                    audienceClaim ?? "null",
                    dto.BotId);
                
                return Results.Json(
                    new { error = "Unauthorized" },
                    statusCode: 401);
            }

            if (dto.BotId != config.Value.BotId)
            {
                logger.LogWarning("Получена команда для другого бота: {CommandBotId}, ожидался: {ConfigBotId}", 
                    dto.BotId, config.Value.BotId);
                
                return Results.BadRequest(new { error = "Invalid bot_id" });
            }

            var command = BotCommandDto.ToCommand(dto);
            
            var resultAppModel = await handler.Handle(command, ct);

            logger.LogDebug("Команда Bot API v4 успешно обработана: {SyncId}", dto.SyncId);
            var responseDto = BotApiResponseDto.FromAppModel(resultAppModel);
            return Results.Json(responseDto, statusCode: 202);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обработке команды Bot API v4: {SyncId}", dto.SyncId);
            return Results.Json(new BotApiResponseDto(), statusCode: 202);
        }
    }
}


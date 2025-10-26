using MF.Express.Bot.Application.UseCases.BotCommands;
using MF.Express.Bot.Api.DTOs.BotCommand;
using MF.Express.Bot.Api.DTOs.Common;
using MF.Express.Bot.Application.UseCases;
using Microsoft.Extensions.Options;
using MF.Express.Bot.Infrastructure.Configuration;

namespace MF.Express.Bot.Api.Endpoints;

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
        IUseCase<BotCommandRequest, BotCommandResult> useCase,
        IOptions<ExpressBotConfiguration> config,
        ILogger<BotCommandEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            var audienceClaim = httpContext.User.FindFirst("aud")?.Value;
            if (audienceClaim != dto.BotId)
            {
                logger.LogWarning("Security violation: JWT audience mismatch");
                return Results.Unauthorized();
            }

            if (dto.BotId != config.Value.BotId)
            {
                logger.LogWarning("Command for different bot received. CommandBotId: {CommandBotId:l}, ConfigBotId: {ConfigBotId:l}", 
                    dto.BotId, config.Value.BotId);
                
                return Results.BadRequest(new { error = "Invalid bot_id" });
            }

            var request = BotCommandDto.ToRequest(dto);
            
            await useCase.ExecuteAsync(request, ct);

            logger.LogDebug("Bot API v4 command processed successfully. SyncId: {SyncId:l}", dto.SyncId);
            return Results.Json(new BotApiResponseDto(), statusCode: 202);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process Bot API v4 command. SyncId: {SyncId:l}", dto.SyncId);
            return Results.Json(new BotApiResponseDto(), statusCode: 202);
        }
    }
}


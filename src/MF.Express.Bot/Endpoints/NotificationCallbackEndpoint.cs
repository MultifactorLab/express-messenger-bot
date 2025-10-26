using MF.Express.Bot.Application.UseCases;
using MF.Express.Bot.Application.UseCases.Notifications;
using MF.Express.Bot.Api.DTOs.BotCommand;
using MF.Express.Bot.Api.DTOs.Common;
using MF.Express.Bot.Api.DTOs.NotificationCallback;
using Microsoft.Extensions.Options;
using MF.Express.Bot.Infrastructure.Configuration;

namespace MF.Express.Bot.Api.Endpoints;

public class NotificationCallbackEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/notification/callback", HandleAsync)
            .WithName("HandleNotificationCallback")
            .Produces<BotApiResponseDto>(202)
            .Produces<BotApiErrorResponseDto>(503);
    }

    private static async Task<IResult> HandleAsync(
        NotificationCallbackDto dto,
        IUseCase<NotificationCallbackRequest, NotificationCallbackResult> useCase,
        IOptions<ExpressBotConfiguration> config,
        ILogger<NotificationCallbackEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            if (dto.Errors != null && dto.Errors.Length != 0)
            {
                logger.LogWarning("Errors: {Errors:l}", string.Join(", ", dto.Errors));
            }

            switch (dto.Status?.ToLowerInvariant())
            {
                case "ok":
                    logger.LogInformation("Message delivered successfully. SyncId: {SyncId:l}", dto.SyncId);
                    break;
                case "error":
                    logger.LogError("Message delivery failed. SyncId: {SyncId:l}, Reason: {Reason:l}", 
                        dto.SyncId, dto.Reason);
                    break;
                default:
                    logger.LogWarning("Unknown callback status. SyncId: {SyncId:l}, Status: {Status:l}", 
                        dto.SyncId, dto.Status);
                    break;
            }
            
            var request = NotificationCallbackDto.ToRequest(dto);
            await useCase.ExecuteAsync(request, ct);

            logger.LogDebug("Notification callback processed successfully. SyncId: {SyncId:l}", dto.SyncId);

            return Results.Json(new BotApiResponseDto(), statusCode: 202);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process notification callback. SyncId: {SyncId:l}", dto.SyncId);

            return Results.Json(new BotApiResponseDto(), statusCode: 202);
        }
    }
}

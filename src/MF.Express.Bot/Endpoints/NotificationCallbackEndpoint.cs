using MF.Express.Bot.Application.UseCases.Notifications;
using MF.Express.Bot.Api.DTOs.BotCommand;
using MF.Express.Bot.Api.DTOs.Common;
using MF.Express.Bot.Api.DTOs.NotificationCallback;
using Microsoft.Extensions.Options;
using MF.Express.Bot.Infrastructure.Configuration;

namespace MF.Express.Bot.Api.Endpoints;

/// <summary>
/// Bot API v4 endpoint для обработки notification callback'ов от BotX
/// Согласно документации https://docs.express.ms/chatbots/developer-guide/api/bot-api/notification-callback/
/// </summary>
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
        IProcessNotificationCallbackUseCase useCase,
        IOptions<ExpressBotConfiguration> config,
        ILogger<NotificationCallbackEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            if (dto.Errors != null && dto.Errors.Length != 0)
            {
                logger.LogWarning("Errors: {Errors}", string.Join(", ", dto.Errors));
            }

            switch (dto.Status?.ToLowerInvariant())
            {
                case "ok":
                    logger.LogInformation("Сообщение {SyncId} успешно доставлено", dto.SyncId);
                    break;
                case "error":
                    logger.LogError("Ошибка доставки сообщения {SyncId}: {Reason}", 
                        dto.SyncId, dto.Reason);
                    break;
                default:
                    logger.LogWarning("Неизвестный статус callback'а {SyncId}: {Status}", 
                        dto.SyncId, dto.Status);
                    break;
            }
            
            var request = NotificationCallbackDto.ToRequest(dto);
            await useCase.ExecuteAsync(request, ct);

            logger.LogDebug("Notification callback успешно обработан: {SyncId}", dto.SyncId);

            return Results.Json(new BotApiResponseDto(), statusCode: 202);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обработке notification callback: {SyncId}", dto.SyncId);

            return Results.Json(new BotApiResponseDto(), statusCode: 202);
        }
    }
}

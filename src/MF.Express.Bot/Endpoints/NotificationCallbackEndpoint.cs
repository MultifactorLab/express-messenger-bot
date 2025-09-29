using MF.Express.Bot.Application.Commands;
using MF.Express.Bot.Application.DTOs;
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
            .Produces<BotApiResponse>(202)
            .Produces<BotApiErrorResponse>(503);
    }

    private static async Task<IResult> HandleAsync(
        NotificationCallbackDto callback,
        ICommand<ProcessNotificationCallbackCommand, NotificationCallbackResult> handler,
        IOptions<ExpressBotConfiguration> config,
        ILogger<NotificationCallbackEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            if (callback.Result != null)
            {
                logger.LogInformation("Result: {Result}", System.Text.Json.JsonSerializer.Serialize(callback.Result));
            }
            
            if (callback.Errors?.Any() == true)
            {
                logger.LogWarning("Errors: {Errors}", string.Join(", ", callback.Errors));
            }
            
            if (callback.ErrorData != null)
            {
                logger.LogWarning("ErrorData: {ErrorData}", System.Text.Json.JsonSerializer.Serialize(callback.ErrorData));
            }

            switch (callback.Status?.ToLowerInvariant())
            {
                case "ok":
                    logger.LogInformation("Сообщение {SyncId} успешно доставлено", callback.SyncId);
                    break;
                case "error":
                    logger.LogError("Ошибка доставки сообщения {SyncId}: {Reason}", 
                        callback.SyncId, callback.Reason);
                    break;
                default:
                    logger.LogWarning("Неизвестный статус callback'а {SyncId}: {Status}", 
                        callback.SyncId, callback.Status);
                    break;
            }

            var processingCommand = new ProcessNotificationCallbackCommand(
                SyncId: callback.SyncId,
                Status: callback.Status,
                Result: callback.Result,
                Reason: callback.Reason,
                Errors: callback.Errors,
                ErrorData: callback.ErrorData
            );

            var result = await handler.Handle(processingCommand, ct);

            logger.LogDebug("Notification callback успешно обработан: {SyncId}", callback.SyncId);

            return Results.Json(new BotApiResponse(), statusCode: 202);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обработке notification callback: {SyncId}", callback.SyncId);

            return Results.Json(new BotApiResponse(), statusCode: 202);
        }
    }
}

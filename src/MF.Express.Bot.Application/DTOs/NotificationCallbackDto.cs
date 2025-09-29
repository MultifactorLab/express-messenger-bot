using System.Text.Json.Serialization;

namespace MF.Express.Bot.Application.DTOs;

/// <summary>
/// DTO для notification callback от BotX согласно документации
/// https://docs.express.ms/chatbots/developer-guide/api/bot-api/notification-callback/
/// 
/// Обрабатывает результат отправки бот-сообщения (успешный или с ошибкой)
/// </summary>
public record NotificationCallbackDto(
    [property: JsonPropertyName("sync_id")] string SyncId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("result")] object? Result = null,
    [property: JsonPropertyName("reason")] string? Reason = null,
    [property: JsonPropertyName("errors")] string[]? Errors = null,
    [property: JsonPropertyName("error_data")] object? ErrorData = null
);

/// <summary>
/// Статус доставки уведомления
/// </summary>
public enum NotificationStatus
{
    Delivered,
    Failed,
    Read
}

/// <summary>
/// Результат обработки notification callback
/// </summary>
public record NotificationCallbackResult(
    bool Success,
    string? ErrorMessage = null,
    NotificationStatus? Status = null
);

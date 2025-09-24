namespace MF.Express.Bot.Application.DTOs;

/// <summary>
/// DTO для входящего сообщения от пользователя Express
/// </summary>
public record IncomingMessageDto(
    string ChatId,
    string UserId,
    string Text,
    DateTime Timestamp,
    string MessageId,
    string? Username = null,
    string? FirstName = null,
    string? LastName = null,
    Dictionary<string, object>? Metadata = null
);

/// <summary>
/// DTO для ответа на обработку команды Bot API v4
/// </summary>
public record CommandProcessedResponse(
    bool Success,
    string? ErrorMessage = null,
    DateTime ProcessedAt = default
)
{
    public CommandProcessedResponse() : this(true)
    {
        ProcessedAt = DateTime.UtcNow;
    }
};

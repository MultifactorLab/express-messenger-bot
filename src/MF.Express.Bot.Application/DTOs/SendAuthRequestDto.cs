namespace MF.Express.Bot.Application.DTOs;

/// <summary>
/// DTO для запроса отправки сообщения с кнопками авторизации
/// </summary>
public record SendAuthRequestDto(
    string ChatId,
    string UserId,
    string AuthRequestId,
    string Message,
    string? ResourceName = null,
    Dictionary<string, object>? Metadata = null
);

/// <summary>
/// DTO результата отправки запроса авторизации
/// </summary>
public record SendAuthResultDto(
    bool Success,
    string? MessageId = null,
    string? ErrorMessage = null,
    DateTime Timestamp = default
)
{
    public SendAuthResultDto() : this(false)
    {
        Timestamp = DateTime.UtcNow;
    }
};


namespace MF.Express.Bot.Application.DTOs;

/// <summary>
/// DTO результата отправки сообщения
/// </summary>
public record SendMessageResultDto(
    string MessageId,
    bool Success,
    string? ErrorMessage = null,
    DateTime Timestamp = default
)
{
    public SendMessageResultDto() : this(string.Empty, false) { }
};


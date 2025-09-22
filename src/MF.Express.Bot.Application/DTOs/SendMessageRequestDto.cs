using MF.Express.Bot.Domain.Enums;

namespace MF.Express.Bot.Application.DTOs;

/// <summary>
/// DTO для запроса отправки сообщения
/// </summary>
public record SendMessageRequestDto(
    string ChatId,
    string Text,
    MessageType Type = MessageType.Text,
    string? ReplyToMessageId = null,
    Dictionary<string, object>? Metadata = null
);


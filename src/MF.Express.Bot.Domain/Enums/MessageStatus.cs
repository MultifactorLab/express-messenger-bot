namespace MF.Express.Bot.Domain.Enums;

/// <summary>
/// Статус сообщения
/// </summary>
public enum MessageStatus
{
    Pending = 1,
    Sent = 2,
    Delivered = 3,
    Failed = 4,
    Processing = 5
}


namespace MF.Express.Bot.Domain.Enums;

/// <summary>
/// Тип сообщения
/// </summary>
public enum MessageType
{
    Text = 1,
    Image = 2,
    Document = 3,
    Audio = 4,
    Video = 5,
    Sticker = 6,
    Location = 7,
    Contact = 8,
    Command = 9,
    System = 10
}


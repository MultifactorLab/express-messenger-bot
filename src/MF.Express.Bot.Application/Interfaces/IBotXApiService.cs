namespace MF.Express.Bot.Application.Interfaces;

/// <summary>
/// Интерфейс для работы с BotX API
/// Основан на документации https://docs.express.ms/chatbots/developer-guide/api/botx-api/
/// </summary>
public interface IBotXApiService
{
    /// <summary>
    /// Отправить текстовое сообщение через BotX API
    /// </summary>
    Task<bool> SendTextMessageAsync(
        string chatId,
        string text, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Отправить сообщение с bubble кнопками (кнопки под сообщением)
    /// </summary>
    Task<bool> SendMessageWithInlineKeyboardAsync(
        string chatId,
        string text,
        List<List<InlineKeyboardButton>> keyboard,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ответить на команду (reply)
    /// </summary>
    Task<bool> ReplyToCommandAsync(
        string syncId,
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить информацию о пользователе
    /// </summary>
    Task<BotXUserInfo?> GetUserInfoAsync(
        string userHuid,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Inline кнопка для сообщений
/// </summary>
public record InlineKeyboardButton(
    string Text,
    string Data
);

/// <summary>
/// Информация о пользователе из BotX
/// </summary>
public record BotXUserInfo(
    string UserHuid,
    string? Username,
    string? AdLogin,
    string? AdDomain
);

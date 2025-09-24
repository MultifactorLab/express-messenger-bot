using MF.Express.Bot.Application.Commands;
using MF.Express.Bot.Application.DTOs;

namespace MF.Express.Bot.Application.Interfaces;

/// <summary>
/// Интерфейс для работы с Bot API v4
/// Обеспечивает отправку сообщений через Express.MS BotX
/// </summary>
public interface IExpressBotService
{
    /// <summary>
    /// Отправляет текстовое сообщение в чат
    /// </summary>
    Task<SendMessageResultDto> SendTextMessageAsync(string chatId, string text, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Отправляет сообщение с inline кнопками
    /// </summary>
    Task<SendMessageResultDto> SendMessageWithButtonsAsync(string chatId, string text, InlineKeyboardMarkup inlineKeyboard, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Отправляет файл в чат
    /// </summary>
    Task<SendMessageResultDto> SendFileAsync(string chatId, Stream file, string fileName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает информацию о боте для Bot API v4
    /// </summary>
    Task<BotInfoDto> GetBotInfoAsync(CancellationToken cancellationToken = default);
}

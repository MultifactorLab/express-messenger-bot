using MF.Express.Bot.Application.Commands;
using MF.Express.Bot.Application.DTOs;

namespace MF.Express.Bot.Application.Interfaces;

/// <summary>
/// Интерфейс для работы с Express Bot API
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
    /// Получает информацию о боте
    /// </summary>
    Task<BotInfoDto> GetBotInfoAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Устанавливает webhook URL
    /// </summary>
    Task<bool> SetWebhookAsync(string webhookUrl, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Удаляет webhook
    /// </summary>
    Task<bool> DeleteWebhookAsync(CancellationToken cancellationToken = default);
}

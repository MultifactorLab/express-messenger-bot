using MF.Express.Bot.Application.DTOs;
using MF.Express.Bot.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MF.Express.Bot.Application.Commands;

/// <summary>
/// Команда отправки запроса авторизации с кнопками подтверждения
/// </summary>
public record SendAuthRequestCommand(
    string ChatId,
    string UserId,
    string AuthRequestId,
    string Message,
    string? ResourceName = null,
    Dictionary<string, object>? Metadata = null
);

/// <summary>
/// Обработчик команды отправки запроса авторизации
/// </summary>
public class SendAuthRequestHandler : ICommand<SendAuthRequestCommand, SendAuthResultDto>
{
    private readonly IExpressBotService _expressBotService;
    private readonly ILogger<SendAuthRequestHandler> _logger;

    public SendAuthRequestHandler(
        IExpressBotService expressBotService,
        ILogger<SendAuthRequestHandler> logger)
    {
        _expressBotService = expressBotService;
        _logger = logger;
    }

    public async Task<SendAuthResultDto> Handle(SendAuthRequestCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Отправка запроса авторизации {AuthRequestId} пользователю {UserId} в чат {ChatId}", 
                command.AuthRequestId, command.UserId, command.ChatId);

            var messageText = FormatAuthMessage(command);
            var inlineKeyboard = CreateAuthButtons(command.AuthRequestId);

            var result = await _expressBotService.SendMessageWithButtonsAsync(
                command.ChatId,
                messageText,
                inlineKeyboard,
                cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("Запрос авторизации {AuthRequestId} успешно отправлен. MessageId: {MessageId}", 
                    command.AuthRequestId, result.MessageId);
                
                return new SendAuthResultDto(
                    Success: true,
                    MessageId: result.MessageId,
                    Timestamp: DateTime.UtcNow);
            }

            _logger.LogWarning("Ошибка при отправке запроса авторизации {AuthRequestId}: {Error}", 
                command.AuthRequestId, result.ErrorMessage);
                
            return new SendAuthResultDto(
                Success: false,
                ErrorMessage: result.ErrorMessage,
                Timestamp: DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке запроса авторизации {AuthRequestId}", command.AuthRequestId);
            
            return new SendAuthResultDto(
                Success: false,
                ErrorMessage: ex.Message,
                Timestamp: DateTime.UtcNow);
        }
    }

    private static string FormatAuthMessage(SendAuthRequestCommand command)
    {
        var resourceInfo = !string.IsNullOrEmpty(command.ResourceName) 
            ? $"\n🏢 Ресурс: {command.ResourceName}" 
            : "";

        return $"""
            🔐 Запрос на авторизацию
            
            {command.Message}{resourceInfo}
            
            ⏰ Время: {DateTime.Now:HH:mm:ss}
            
            Подтвердите или отклоните доступ:
            """;
    }

    private static InlineKeyboardMarkup CreateAuthButtons(string authRequestId)
    {
        return new InlineKeyboardMarkup(new[]
        {
            new InlineKeyboardButton[]
            {
                new("✅ Разрешить", $"auth_allow_{authRequestId}"),
                new("❌ Отклонить", $"auth_deny_{authRequestId}")
            }
        });
    }
}

/// <summary>
/// Inline клавиатура для кнопок
/// </summary>
public record InlineKeyboardMarkup(InlineKeyboardButton[][] Keyboard);

/// <summary>
/// Кнопка inline клавиатуры
/// </summary>
public record InlineKeyboardButton(string Text, string CallbackData);


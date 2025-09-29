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
    private readonly IBotXApiService _expressBotService;
    private readonly ILogger<SendAuthRequestHandler> _logger;

    public SendAuthRequestHandler(
        IBotXApiService expressBotService,
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

            var success = await _expressBotService.SendMessageWithInlineKeyboardAsync(
                command.ChatId,
                messageText,
                inlineKeyboard,
                cancellationToken);

            return success ? new SendAuthResultDto { Success = true } : new SendAuthResultDto { Success = false };

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

    private static List<List<InlineKeyboardButton>> CreateAuthButtons(string authRequestId)
    {
        return
        [
            new List<InlineKeyboardButton>()
            {
                new("✅ Разрешить", $"auth_allow_{authRequestId}"),
                new("❌ Отклонить", $"auth_deny_{authRequestId}")    
            }
        ];
    }
}



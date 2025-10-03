using MF.Express.Bot.Application.Models.SendAuthRequest;
using MF.Express.Bot.Application.Models.BotX;
using MF.Express.Bot.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MF.Express.Bot.Application.Commands;

public record SendAuthRequestCommand(
    string ChatId,
    string UserId,
    string AuthRequestId,
    string Message,
    string? ResourceName = null,
    Dictionary<string, object>? Metadata = null
);

public class SendAuthRequestHandler : ICommand<SendAuthRequestCommand, SendAuthResultAppModel>
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

    public async Task<SendAuthResultAppModel> Handle(SendAuthRequestCommand command, CancellationToken cancellationToken = default)
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

            return success ? new SendAuthResultAppModel { Success = true } : new SendAuthResultAppModel { Success = false };

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке запроса авторизации {AuthRequestId}", command.AuthRequestId);

            return new SendAuthResultAppModel(
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

    private static List<List<InlineKeyboardButtonModel>> CreateAuthButtons(string authRequestId)
    {
        return
        [
            new List<InlineKeyboardButtonModel>()
            {
                new("✅ Разрешить", $"auth_allow_{authRequestId}"),
                new("❌ Отклонить", $"auth_deny_{authRequestId}")    
            }
        ];
    }
}



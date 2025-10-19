using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Application.Models.BotX;
using Microsoft.Extensions.Logging;

namespace MF.Express.Bot.Application.UseCases.Auth;

public interface ISendAuthRequestUseCase : IUseCase<SendAuthRequestRequest, SendAuthRequestResult>
{
}

public record SendAuthRequestRequest(
    string ChatId,
    string UserId,
    string AuthRequestId,
    string Message,
    string? ResourceName = null
);

public record SendAuthRequestResult(
    bool Success,
    string? ErrorMessage = null,
    DateTime? Timestamp = null,
    SendAuthErrorType? ErrorType = null
);

public enum SendAuthErrorType
{
    ValidationError,
    ExternalServiceError,
    InternalError
}

public class SendAuthRequestUseCase : ISendAuthRequestUseCase
{
    private readonly IBotXApiService _botXApiService;
    private readonly ILogger<SendAuthRequestUseCase> _logger;

    public SendAuthRequestUseCase(
        IBotXApiService botXApiService,
        ILogger<SendAuthRequestUseCase> logger)
    {
        _botXApiService = botXApiService;
        _logger = logger;
    }

    public async Task<SendAuthRequestResult> ExecuteAsync(
        SendAuthRequestRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Отправка запроса авторизации {AuthRequestId} пользователю {UserId} в чат {ChatId}",
                request.AuthRequestId, request.UserId, request.ChatId);

            var messageText = FormatAuthMessage(request);
            var inlineKeyboard = CreateAuthButtons(request.AuthRequestId);

            var success = await _botXApiService.SendMessageWithInlineKeyboardAsync(
                request.ChatId,
                messageText,
                inlineKeyboard,
                cancellationToken);

            if (success)
            {
                return new SendAuthRequestResult(
                    Success: true,
                    Timestamp: DateTime.UtcNow);
            }
            else
            {
                return new SendAuthRequestResult(
                    Success: false,
                    ErrorMessage: "Не удалось отправить сообщение",
                    Timestamp: DateTime.UtcNow,
                    ErrorType: SendAuthErrorType.ExternalServiceError);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке запроса авторизации {AuthRequestId}", request.AuthRequestId);

            return new SendAuthRequestResult(
                Success: false,
                ErrorMessage: ex.Message,
                Timestamp: DateTime.UtcNow,
                ErrorType: SendAuthErrorType.InternalError);
        }
    }

    private static string FormatAuthMessage(SendAuthRequestRequest request)
    {
        var resourceInfo = !string.IsNullOrEmpty(request.ResourceName) 
            ? $"\n🏢 Ресурс: {request.ResourceName}" 
            : "";

        return $"""
            🔐 Запрос на авторизацию
            
            {request.Message}{resourceInfo}
            
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

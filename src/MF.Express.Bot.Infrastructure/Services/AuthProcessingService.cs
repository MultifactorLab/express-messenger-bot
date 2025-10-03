using Microsoft.Extensions.Logging;
using MF.Express.Bot.Application.Models.Auth;
using MF.Express.Bot.Application.Models.BotCommand;
using MF.Express.Bot.Application.Models.Common;
using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Application.Services;

namespace MF.Express.Bot.Infrastructure.Services;

public class AuthProcessingService : IAuthProcessingService
{
    private readonly IMultifactorApiService _multifactorApiService;
    private readonly IBotXApiService _botXApiService;
    private readonly ILogger<AuthProcessingService> _logger;

    public AuthProcessingService(
        IMultifactorApiService multifactorApiService,
        IBotXApiService botXApiService,
        ILogger<AuthProcessingService> logger)
    {
        _multifactorApiService = multifactorApiService;
        _botXApiService = botXApiService;
        _logger = logger;
    }

    public async Task<CommandProcessedResponse> ProcessAuthCallbackAsync(
        string callbackId,
        string authRequestId,
        string userId,
        string chatId,
        AuthAction action,
        string? messageId = null,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Обработка callback авторизации {AuthRequestId} от пользователя {UserId}: {Action}", 
                authRequestId, userId, action);

            var authResult = new AuthorizationResultAppModel(
                AuthRequestId: authRequestId,
                UserId: userId,
                Action: action,
                ProcessedAt: DateTime.UtcNow
            );

            // отправка в Multifactor API
            // var success = await _multifactorApiService.SendAuthorizationResultAsync(authResult, cancellationToken);
            //
            // if (!success)
            // {
            //     _logger.LogWarning("Не удалось отправить результат авторизации {AuthRequestId} в Multifactor API", authRequestId);
            //     return new CommandProcessedResponse(false, "Ошибка при отправке результата авторизации в Multifactor API");
            // }

            var actionText = action == AuthAction.Allow ? "РАЗРЕШЕНА" : "ОТКЛОНЕНА";
            var actionEmoji = action == AuthAction.Allow ? "✅" : "❌";
            
            var detailedMessage = $"""
                {actionEmoji} **АВТОРИЗАЦИЯ {actionText}**
                
                📋 **Детали запроса:**
                • Auth Request ID: {authRequestId}
                • Callback ID: {callbackId}
                • Действие: {action}
                • Время обработки: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
                
                👤 **Пользователь:**
                • User ID: {userId}
                • Chat ID: {chatId}
                • Message ID: {messageId ?? "не указан"}
                
                📊 **Статус:** Callback успешно обработан
                """;

            await _botXApiService.SendTextMessageAsync(
                chatId, 
                detailedMessage, 
                cancellationToken);

            _logger.LogInformation("Callback авторизации {AuthRequestId} успешно обработан (действие: {Action})", 
                authRequestId, action);
            return new CommandProcessedResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке callback авторизации {AuthRequestId}", authRequestId);
            
            return new CommandProcessedResponse(false, $"Внутренняя ошибка: {ex.Message}");
        }
    }
}


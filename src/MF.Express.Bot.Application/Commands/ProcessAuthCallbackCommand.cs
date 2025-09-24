using Microsoft.Extensions.Logging;
using MF.Express.Bot.Application.DTOs;
using MF.Express.Bot.Application.Interfaces;

namespace MF.Express.Bot.Application.Commands;

/// <summary>
/// Команда для обработки callback от кнопок авторизации
/// </summary>
public record ProcessAuthCallbackCommand(
    string CallbackId,
    string UserId,
    string ChatId,
    string AuthRequestId,
    AuthAction Action,
    DateTime Timestamp,
    string? MessageId = null,
    Dictionary<string, object>? Metadata = null
);

/// <summary>
/// Обработчик команды ProcessAuthCallback
/// </summary>
public class ProcessAuthCallbackHandler : ICommand<ProcessAuthCallbackCommand, WebhookProcessedResponse>
{
    private readonly IMultifactorApiService _multifactorApiService;
    private readonly IExpressBotService _expressBotService;
    private readonly ILogger<ProcessAuthCallbackHandler> _logger;

    public ProcessAuthCallbackHandler(
        IMultifactorApiService multifactorApiService,
        IExpressBotService expressBotService,
        ILogger<ProcessAuthCallbackHandler> logger)
    {
        _multifactorApiService = multifactorApiService;
        _expressBotService = expressBotService;
        _logger = logger;
    }

    public async Task<WebhookProcessedResponse> Handle(ProcessAuthCallbackCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Обработка callback авторизации {AuthRequestId} от пользователя {UserId}: {Action}", 
                command.AuthRequestId, command.UserId, command.Action);

            // Создаем результат авторизации
            var authResult = new AuthorizationResultDto(
                AuthRequestId: command.AuthRequestId,
                UserId: command.UserId,
                Action: command.Action,
                ProcessedAt: command.Timestamp
            );

            // Отправляем результат в Multifactor API
            var success = await _multifactorApiService.SendAuthorizationResultAsync(authResult, cancellationToken);

            if (!success)
            {
                _logger.LogWarning("Не удалось отправить результат авторизации {AuthRequestId} в Multifactor API", command.AuthRequestId);
                return new WebhookProcessedResponse(false, "Ошибка при отправке результата авторизации в Multifactor API");
            }

            // Отправляем подтверждающее сообщение пользователю
            var confirmationMessage = command.Action == AuthAction.Allow 
                ? "✅ Авторизация разрешена" 
                : "❌ Авторизация отклонена";

            await _expressBotService.SendTextMessageAsync(
                command.ChatId, 
                confirmationMessage, 
                cancellationToken);

            _logger.LogInformation("Callback авторизации {AuthRequestId} успешно обработан", command.AuthRequestId);
            return new WebhookProcessedResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке callback авторизации {AuthRequestId}", command.AuthRequestId);
            return new WebhookProcessedResponse(false, $"Внутренняя ошибка: {ex.Message}");
        }
    }
}

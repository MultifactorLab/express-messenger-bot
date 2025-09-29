using Microsoft.Extensions.Logging;
using MF.Express.Bot.Application.DTOs;

namespace MF.Express.Bot.Application.Commands;

/// <summary>
/// Команда для обработки notification callback от BotX
/// Согласно документации https://docs.express.ms/chatbots/developer-guide/api/bot-api/notification-callback/
/// </summary>
public record ProcessNotificationCallbackCommand(
    string SyncId,
    string Status,
    object? Result,
    string? Reason,
    string[]? Errors,
    object? ErrorData
);

/// <summary>
/// Обработчик команды ProcessNotificationCallback
/// </summary>
public class ProcessNotificationCallbackHandler : ICommand<ProcessNotificationCallbackCommand, NotificationCallbackResult>
{
    private readonly ILogger<ProcessNotificationCallbackHandler> _logger;

    public ProcessNotificationCallbackHandler(ILogger<ProcessNotificationCallbackHandler> logger)
    {
        _logger = logger;
    }

    public async Task<NotificationCallbackResult> Handle(ProcessNotificationCallbackCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Обработка notification callback: {Status} для {SyncId}", 
                command.Status, command.SyncId);

            return command.Status.ToLowerInvariant() switch
            {
                "ok" => await HandleSuccessCallback(command, cancellationToken),
                "error" => await HandleErrorCallback(command, cancellationToken),
                _ => HandleUnknownStatus(command)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке notification callback {SyncId}", command.SyncId);
            return new NotificationCallbackResult(false, $"Внутренняя ошибка: {ex.Message}");
        }
    }

    private async Task<NotificationCallbackResult> HandleSuccessCallback(ProcessNotificationCallbackCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Сообщение {SyncId} успешно обработано. Результат: {Result}", 
            command.SyncId, command.Result);

        await Task.CompletedTask;
        
        return new NotificationCallbackResult(true, Status: NotificationStatus.Delivered);
    }

    private async Task<NotificationCallbackResult> HandleErrorCallback(ProcessNotificationCallbackCommand command, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Ошибка при обработке сообщения {SyncId}. Причина: {Reason}, Ошибки: {Errors}, Данные: {ErrorData}", 
            command.SyncId, command.Reason, command.Errors != null ? string.Join(", ", command.Errors) : "нет", command.ErrorData);

        await Task.CompletedTask;
        
        return new NotificationCallbackResult(true, Status: NotificationStatus.Failed);
    }

    private NotificationCallbackResult HandleUnknownStatus(ProcessNotificationCallbackCommand command)
    {
        _logger.LogWarning("Неизвестный статус notification callback: {Status} для {SyncId}", 
            command.Status, command.SyncId);
        
        return new NotificationCallbackResult(true);
    }
}

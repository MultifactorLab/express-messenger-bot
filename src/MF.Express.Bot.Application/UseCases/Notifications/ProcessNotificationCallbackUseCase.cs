using MF.Express.Bot.Application.Models.NotificationCallback;
using Microsoft.Extensions.Logging;

namespace MF.Express.Bot.Application.UseCases.Notifications;

public interface IProcessNotificationCallbackUseCase : IUseCase<NotificationCallbackRequest, NotificationCallbackResult>
{
}

public record NotificationCallbackRequest(
    string SyncId,
    string Status,
    object? Result = null,
    string? Reason = null,
    string[]? Errors = null,
    object? ErrorData = null
);

public record NotificationCallbackResult(
    bool Success,
    NotificationStatus Status = NotificationStatus.Unknown,
    string? ErrorMessage = null
);

public class ProcessNotificationCallbackUseCase : IProcessNotificationCallbackUseCase
{
    private readonly ILogger<ProcessNotificationCallbackUseCase> _logger;

    public ProcessNotificationCallbackUseCase(ILogger<ProcessNotificationCallbackUseCase> logger)
    {
        _logger = logger;
    }

    public async Task<NotificationCallbackResult> ExecuteAsync(
        NotificationCallbackRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Обработка notification callback: {Status} для {SyncId}", 
                request.Status, request.SyncId);

            return request.Status.ToLowerInvariant() switch
            {
                "ok" => await HandleSuccessCallback(request, cancellationToken),
                "error" => await HandleErrorCallback(request, cancellationToken),
                _ => HandleUnknownStatus(request)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке notification callback {SyncId}", request.SyncId);
            return new NotificationCallbackResult(false, ErrorMessage: $"Внутренняя ошибка: {ex.Message}");
        }
    }

    private async Task<NotificationCallbackResult> HandleSuccessCallback(
        NotificationCallbackRequest request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Сообщение {SyncId} успешно обработано. Результат: {Result}", 
            request.SyncId, request.Result);

        await Task.CompletedTask;
        
        return new NotificationCallbackResult(true, Status: NotificationStatus.Delivered);
    }

    private async Task<NotificationCallbackResult> HandleErrorCallback(
        NotificationCallbackRequest request, 
        CancellationToken cancellationToken)
    {
        _logger.LogWarning("Ошибка при обработке сообщения {SyncId}. Причина: {Reason}, Ошибки: {Errors}, Данные: {ErrorData}", 
            request.SyncId, 
            request.Reason, 
            request.Errors != null ? string.Join(", ", request.Errors) : "нет", 
            request.ErrorData);

        await Task.CompletedTask;
        
        return new NotificationCallbackResult(true, Status: NotificationStatus.Failed);
    }

    private NotificationCallbackResult HandleUnknownStatus(NotificationCallbackRequest request)
    {
        _logger.LogWarning("Неизвестный статус notification callback: {Status} для {SyncId}", 
            request.Status, request.SyncId);
        
        return new NotificationCallbackResult(true);
    }
}

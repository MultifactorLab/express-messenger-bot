using MF.Express.Bot.Application.Models.NotificationCallback;
using Microsoft.Extensions.Logging;

namespace MF.Express.Bot.Application.UseCases.Notifications;

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

public class ProcessNotificationCallbackUseCase : IUseCase<NotificationCallbackRequest, NotificationCallbackResult>
{
    private readonly ILogger<ProcessNotificationCallbackUseCase> _logger;

    public ProcessNotificationCallbackUseCase(ILogger<ProcessNotificationCallbackUseCase> logger)
    {
        _logger = logger;
    }

    public async Task<NotificationCallbackResult> ExecuteAsync(
        NotificationCallbackRequest botRequest, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing notification callback. Status: {Status:l}, SyncId: {SyncId:l}", 
                botRequest.Status, botRequest.SyncId);

            return botRequest.Status.ToLowerInvariant() switch
            {
                "ok" => await HandleSuccessCallback(botRequest, cancellationToken),
                "error" => await HandleErrorCallback(botRequest, cancellationToken),
                _ => HandleUnknownStatus(botRequest)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process notification callback. SyncId: {SyncId:l}", botRequest.SyncId);
            return new NotificationCallbackResult(false, ErrorMessage: $"Внутренняя ошибка: {ex.Message}");
        }
    }

    private async Task<NotificationCallbackResult> HandleSuccessCallback(
        NotificationCallbackRequest request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Message processed successfully. SyncId: {SyncId:l}, Result: {Result:l}", 
            request.SyncId, request.Result);

        await Task.CompletedTask;
        
        return new NotificationCallbackResult(true, Status: NotificationStatus.Delivered);
    }

    private async Task<NotificationCallbackResult> HandleErrorCallback(
        NotificationCallbackRequest request, 
        CancellationToken cancellationToken)
    {
        _logger.LogWarning("Message processing failed. SyncId: {SyncId:l}, Reason: {Reason:l}, Errors: {Errors:l}, ErrorData: {ErrorData:l}", 
            request.SyncId, 
            request.Reason, 
            request.Errors != null ? string.Join(", ", request.Errors) : "none", 
            request.ErrorData);

        await Task.CompletedTask;
        
        return new NotificationCallbackResult(true, Status: NotificationStatus.Failed);
    }

    private NotificationCallbackResult HandleUnknownStatus(NotificationCallbackRequest request)
    {
        _logger.LogWarning("Unknown notification callback status. Status: {Status:l}, SyncId: {SyncId:l}", 
            request.Status, request.SyncId);
        
        return new NotificationCallbackResult(true);
    }
}

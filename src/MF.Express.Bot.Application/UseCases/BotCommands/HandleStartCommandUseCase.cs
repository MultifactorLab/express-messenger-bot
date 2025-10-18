using MF.Express.Bot.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MF.Express.Bot.Application.UseCases.BotCommands;

public interface IHandleStartCommandUseCase : IUseCase<StartCommandRequest, StartCommandResult>
{
}

public record StartCommandRequest(
    string UserId,
    string ChatId,
    string? Username = null,
    string? FirstName = null,
    string? LastName = null,
    string? AdLogin = null,
    string? AdDomain = null,
    string? ChatType = null,
    string? Platform = null,
    string? AppVersion = null,
    string? Device = null,
    string? Locale = null,
    Dictionary<string, object>? Metadata = null
);

public record StartCommandResult(
    bool Success,
    string? ErrorMessage = null
);

public class HandleStartCommandUseCase : IHandleStartCommandUseCase
{
    private readonly IBotXApiService _botXApiService;
    private readonly ILogger<HandleStartCommandUseCase> _logger;

    public HandleStartCommandUseCase(
        IBotXApiService botXApiService,
        ILogger<HandleStartCommandUseCase> logger)
    {
        _botXApiService = botXApiService;
        _logger = logger;
    }

    public async Task<StartCommandResult> ExecuteAsync(
        StartCommandRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Обработка команды /start от пользователя {UserId} в чате {ChatId}", 
            request.UserId, request.ChatId);

        try
        {
            var userDataMessage = FormatUserDataMessage(request);

            var success = await _botXApiService.SendTextMessageAsync(
                request.ChatId, 
                userDataMessage, 
                cancellationToken);

            if (success)
            {
                _logger.LogInformation("Данные пользователя {UserId} успешно отправлены в чат {ChatId}", 
                    request.UserId, request.ChatId);
                return new StartCommandResult(true);
            }
            else
            {
                _logger.LogWarning("Не удалось отправить данные пользователя {UserId} в чат {ChatId}", 
                    request.UserId, request.ChatId);
                return new StartCommandResult(false, "Не удалось отправить сообщение");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке команды /start для пользователя {UserId}", request.UserId);
            return new StartCommandResult(false, ex.Message);
        }
    }

    private static string FormatUserDataMessage(StartCommandRequest request)
    {
        return $"""
            📋 **Данные пользователя /start:**
            
            👤 **Пользователь:**
            • ID: {request.UserId}
            • Username: {request.Username ?? "не указан"}
            • Имя: {request.FirstName ?? "не указано"}
            • Фамилия: {request.LastName ?? "не указана"}
            
            🏢 **AD данные:**
            • Login: {request.AdLogin ?? "не указан"}
            • Domain: {request.AdDomain ?? "не указан"}
            
            💬 **Чат:**
            • Chat ID: {request.ChatId}
            • Chat Type: {request.ChatType ?? "не указан"}
            
            📱 **Устройство:**
            • Platform: {request.Platform ?? "не указана"}
            • Device: {request.Device ?? "не указано"}
            • App Version: {request.AppVersion ?? "не указана"}
            • Locale: {request.Locale ?? "не указана"}
            
            🕐 **Время:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
            """;
    }
}

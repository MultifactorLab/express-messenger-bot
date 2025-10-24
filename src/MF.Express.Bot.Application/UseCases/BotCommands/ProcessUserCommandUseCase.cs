using MF.Express.Bot.Application.Models.BotCommand;
using Microsoft.Extensions.Logging;

namespace MF.Express.Bot.Application.UseCases.BotCommands;

public interface IProcessUserCommandUseCase : IUseCase<UserCommandRequest, UserCommandResult>
{
}

public record UserCommandRequest(
    string SyncId,
    string CommandBody,
    Dictionary<string, object>? CommandData,
    Dictionary<string, object>? CommandMetadata,
    string? UserHuid,
    string? GroupChatId,
    string? BotId,
    string? Username,
    string? Device,
    string? Locale
);

public record UserCommandResult(
    bool Success,
    string? ErrorMessage = null
);

public class ProcessUserCommandUseCase : IProcessUserCommandUseCase
{
    private readonly IHandleStartCommandUseCase _handleStartCommandUseCase;
    private readonly IHandleAuthCallbackUseCase _handleAuthCallbackUseCase;
    private readonly ILogger<ProcessUserCommandUseCase> _logger;

    public ProcessUserCommandUseCase(
        IHandleStartCommandUseCase handleStartCommandUseCase,
        IHandleAuthCallbackUseCase handleAuthCallbackUseCase,
        ILogger<ProcessUserCommandUseCase> logger)
    {
        _handleStartCommandUseCase = handleStartCommandUseCase;
        _handleAuthCallbackUseCase = handleAuthCallbackUseCase;
        _logger = logger;
    }

    public async Task<UserCommandResult> ExecuteAsync(
        UserCommandRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Обработка пользовательской команды: {Body} от {UserHuid}", 
            request.CommandBody, request.UserHuid);

        if (request.UserHuid == null || request.GroupChatId == null || request.BotId == null)
        {
            _logger.LogWarning("");   
        }
        try
        {
            if (IsStartCommand(request))
            {
                var requestId = ExtractRequestId(request);
                
                if (string.IsNullOrEmpty(requestId))
                {
                    _logger.LogWarning("Команда /start без requestId. Регистрация чата невозможна. CommandBody: {CommandBody}", 
                        request.CommandBody);
                    return new UserCommandResult(false, "Request ID is required for chat registration");
                }
                
                _logger.LogInformation("Извлечен requestId: {RequestId} для ExpressUserId {UserHuid}", 
                    requestId, request.UserHuid);

                var startRequest = new StartCommandRequest(
                    ExpressUserId: request.UserHuid,
                    ChatId: request.GroupChatId,
                    BotId: request.BotId,
                    RequestId: requestId,
                    Username: request.Username,
                    Device: request.Device,
                    Locale: request.Locale
                );

                var result = await _handleStartCommandUseCase.ExecuteAsync(startRequest, cancellationToken);
                return new UserCommandResult(result.Success, result.ErrorMessage);
            }

            if (IsButtonCallback(request))
            {
                var callbackData = request.CommandBody;
                
                _logger.LogDebug("Извлеченные callback данные: {CallbackData} из CommandBody: {CommandBody}", 
                    callbackData, request.CommandBody);
                
                if (string.IsNullOrEmpty(callbackData))
                {
                    _logger.LogWarning("Не удалось извлечь данные callback'а из команды {SyncId}. CommandBody: {CommandBody}", 
                        request.SyncId, request.CommandBody);
                    return new UserCommandResult(true);
                }

                var callbackRequest = new AuthCallbackRequest(
                    CallbackData: callbackData,
                    ChatId: request.GroupChatId
                );

                var result = await _handleAuthCallbackUseCase.ExecuteAsync(callbackRequest, cancellationToken);
                return new UserCommandResult(result.Success, result.ErrorMessage);
            }
            
            return new UserCommandResult(false, "Unknown command");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке пользовательской команды {SyncId}", request.SyncId);
            return new UserCommandResult(false, ex.Message);
        }
    }

    private static bool IsStartCommand(UserCommandRequest request)
    {
        return request.CommandBody.Trim().Equals("/start", StringComparison.OrdinalIgnoreCase) ||
               request.CommandBody.Trim().Equals("start", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsButtonCallback(UserCommandRequest request)
    {
        return request.CommandBody.Contains(':') && request.CommandBody.Split(':').Length == 3;
    }

    private static string? ExtractRequestId(UserCommandRequest request)
    {
        if (request.CommandData?.TryGetValue("command", out var commandObj) == true)
        {
            var commandValue = commandObj?.ToString();
            if (!string.IsNullOrEmpty(commandValue))
            {
                var match = System.Text.RegularExpressions.Regex.Match(commandValue, @"/req=([^\s]+)");
                if (match is { Success: true, Groups.Count: > 1 })
                {
                    return match.Groups[1].Value;
                }
            }
        }

        return null;
    }
}

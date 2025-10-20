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
    private readonly IProcessIncomingMessageUseCase _processIncomingMessageUseCase;
    private readonly ILogger<ProcessUserCommandUseCase> _logger;

    public ProcessUserCommandUseCase(
        IHandleStartCommandUseCase handleStartCommandUseCase,
        IHandleAuthCallbackUseCase handleAuthCallbackUseCase,
        IProcessIncomingMessageUseCase processIncomingMessageUseCase,
        ILogger<ProcessUserCommandUseCase> logger)
    {
        _handleStartCommandUseCase = handleStartCommandUseCase;
        _handleAuthCallbackUseCase = handleAuthCallbackUseCase;
        _processIncomingMessageUseCase = processIncomingMessageUseCase;
        _logger = logger;
    }

    public async Task<UserCommandResult> ExecuteAsync(
        UserCommandRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Обработка пользовательской команды: {Body} от {UserHuid}", 
            request.CommandBody, request.UserHuid);

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
                var callbackData = ExtractCallbackData(request);
                
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
                    ChatId: request.GroupChatId ?? "private"
                );

                var result = await _handleAuthCallbackUseCase.ExecuteAsync(callbackRequest, cancellationToken);
                return new UserCommandResult(result.Success, result.ErrorMessage);
            }

            var messageRequest = new IncomingMessageRequest(
                ChatId: request.GroupChatId ?? "private",
                UserId: request.UserHuid ?? "unknown",
                Text: request.CommandBody,
                MessageId: request.SyncId,
                Username: request.Username,
                FirstName: ExtractFromData(request.CommandData, "first_name"),
                LastName: ExtractFromData(request.CommandData, "last_name"),
                Metadata: request.CommandMetadata
            );

            var messageResult = await _processIncomingMessageUseCase.ExecuteAsync(messageRequest, cancellationToken);
            return new UserCommandResult(messageResult.Success, messageResult.ErrorMessage);
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
        return request.CommandData?.ContainsKey("callback_data") == true ||
               request.CommandData?.ContainsKey("button_data") == true ||
               request.CommandBody.StartsWith("callback:", StringComparison.OrdinalIgnoreCase) ||
               (request.CommandBody.Contains(':') && request.CommandBody.Split(':').Length >= 2);
    }

    private static string? ExtractCallbackData(UserCommandRequest request)
    {
        if (request.CommandData?.TryGetValue("callback_data", out var callbackObj) == true)
        {
            return callbackObj?.ToString();
        }

        if (request.CommandData?.TryGetValue("button_data", out var buttonObj) == true)
        {
            return buttonObj?.ToString();
        }

        if (request.CommandBody.StartsWith("callback:", StringComparison.OrdinalIgnoreCase))
        {
            return request.CommandBody[9..];
        }

        if (request.CommandBody.Contains(':'))
        {
            return request.CommandBody;
        }

        return null;
    }

    private static string? ExtractFromData(Dictionary<string, object>? data, string key)
    {
        return data?.TryGetValue(key, out var value) == true ? value?.ToString() : null;
    }

    private static string? ExtractRequestId(UserCommandRequest request)
    {
        if (request.CommandData?.TryGetValue("Value", out var valueObj) == true)
        {
            var value = valueObj?.ToString();
            if (!string.IsNullOrEmpty(value))
            {
                var match = System.Text.RegularExpressions.Regex.Match(value, @"/req=([^\s]+)");
                if (match.Success && match.Groups.Count > 1)
                {
                    return match.Groups[1].Value;
                }
            }
        }

        if (request.CommandData?.TryGetValue("command", out var commandObj) == true)
        {
            var commandValue = commandObj?.ToString();
            if (!string.IsNullOrEmpty(commandValue))
            {
                var match = System.Text.RegularExpressions.Regex.Match(commandValue, @"/req=([^\s]+)");
                if (match.Success && match.Groups.Count > 1)
                {
                    return match.Groups[1].Value;
                }
            }
        }

        return null;
    }
}

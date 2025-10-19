using MF.Express.Bot.Application.Models.BotCommand;
using Microsoft.Extensions.Logging;

namespace MF.Express.Bot.Application.UseCases.BotCommands;

public interface IProcessUserCommandUseCase : IUseCase<UserCommandRequest, UserCommandResult>
{
}

public record UserCommandRequest(
    string SyncId,
    string? SourceSyncId,
    string CommandBody,
    Dictionary<string, object>? CommandData,
    Dictionary<string, object>? CommandMetadata,
    string? UserHuid,
    string? GroupChatId,
    string? ChatType,
    string? Username,
    string? AdLogin,
    string? AdDomain,
    string? Device,
    string? DeviceSoftware,
    string? Platform,
    string? AppVersion,
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
                var startRequest = new StartCommandRequest(
                    UserId: request.UserHuid ?? "unknown",
                    ChatId: request.GroupChatId ?? "private",
                    Username: request.Username,
                    FirstName: ExtractFromData(request.CommandData, "first_name"),
                    LastName: ExtractFromData(request.CommandData, "last_name"),
                    AdLogin: request.AdLogin,
                    AdDomain: request.AdDomain,
                    ChatType: request.ChatType,
                    Platform: request.Platform,
                    AppVersion: request.AppVersion,
                    Device: request.Device,
                    Locale: request.Locale,
                    Metadata: request.CommandMetadata
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
}

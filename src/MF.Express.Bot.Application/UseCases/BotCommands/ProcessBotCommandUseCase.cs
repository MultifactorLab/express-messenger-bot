using System.Text.RegularExpressions;
using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Application.Services;
using Microsoft.Extensions.Logging;

namespace MF.Express.Bot.Application.UseCases.BotCommands;
public record BotCommandRequest(
    string? SyncId,
    string? CommandType,
    string? CommandBody,
    Dictionary<string, object>? CommandData,
    string? UserHuid,
    string? GroupChatId,
    string? Username,
    string? Device,
    string? Locale,
    string? BotId
);

public record BotCommandResult(
    bool Success = true,
    string? ErrorMessage = null
);

public partial class ProcessBotCommandUseCase : IUseCase<BotCommandRequest, BotCommandResult>
{
    private readonly IMfExpressApiService _mfApiService;
    private readonly IAuthProcessingService _authProcessingService;
    private readonly IBotXApiService _botXApiService;
    private readonly ILogger<ProcessBotCommandUseCase> _logger;

    public ProcessBotCommandUseCase(
        IMfExpressApiService mfApiService,
        IAuthProcessingService authProcessingService,
        IBotXApiService botXApiService,
        ILogger<ProcessBotCommandUseCase> logger)
    {
        _mfApiService = mfApiService;
        _authProcessingService = authProcessingService;
        _botXApiService = botXApiService;
        _logger = logger;
    }

    public async Task<BotCommandResult> ExecuteAsync(
        BotCommandRequest botRequest, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            return botRequest.CommandType.ToLowerInvariant() switch
            {
                "user" => await HandleUserCommand(botRequest, cancellationToken),
                "system" => await HandleSystemCommand(botRequest, cancellationToken),
                _ => new BotCommandResult()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process BotX command. SyncId: {SyncId:l}, CommandType: {CommandType:l}", 
                botRequest.SyncId, botRequest.CommandType);
            
            return new BotCommandResult(Success: true);
        }
    }

    private async Task<BotCommandResult> HandleUserCommand(
        BotCommandRequest botRequest, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing user command. Body: {Body:l}, UserHuid: {UserHuid:l}", 
            botRequest.CommandBody, botRequest.UserHuid);

        if (botRequest.UserHuid == null || botRequest.GroupChatId == null || botRequest.BotId == null)
        {
            _logger.LogWarning("Missing required parameters");   
        }
        try
        {
            if (IsStartCommand(botRequest))
            {
                var requestId = ExtractRequestId(botRequest);
                
                if (string.IsNullOrEmpty(requestId))
                {
                    _logger.LogWarning("/start command without requestId");
                    return new BotCommandResult(false, "Request ID is required for chat registration");
                }
                
                _logger.LogInformation("RequestId extracted. RequestId: {RequestId:l}, ExpressUserId: {UserHuid:l}", 
                    requestId, botRequest.UserHuid);

                var result = await _mfApiService.SendChatCreatedCallbackAsync(botRequest, requestId, cancellationToken);
                return new BotCommandResult(result);
            }

            if (IsInlineEnrollmentCode(botRequest))
            {
                var code = botRequest.CommandBody!.Trim();
                var response = await _mfApiService.SendInlineEnrollCodeAsync(botRequest, code, cancellationToken);
                if (!response.Success)
                {
                    return new BotCommandResult(false, response.ErrorMessage ?? "Inline enrollment failed");
                }

                if (!string.IsNullOrWhiteSpace(response.Message) && !string.IsNullOrWhiteSpace(botRequest.GroupChatId))
                {
                    await _botXApiService.SendTextMessageAsync(botRequest.GroupChatId, response.Message, cancellationToken);
                }

                return new BotCommandResult(true);
            }

            if (IsButtonCallback(botRequest))
            {
                var callbackData = botRequest.CommandBody;
                
                _logger.LogDebug("Callback data extracted. CallbackData: {CallbackData:l}, CommandBody: {CommandBody:l}", 
                    callbackData, botRequest.CommandBody);
                
                if (string.IsNullOrEmpty(callbackData) || string.IsNullOrEmpty(botRequest.GroupChatId))
                {
                    _logger.LogWarning("Failed to extract callback data.");
                    return new BotCommandResult(false);
                }

                var result = await _authProcessingService.ProcessAuthCallbackAsync(
                    callbackData: callbackData,
                    chatId: botRequest.GroupChatId,
                    cancellationToken: cancellationToken
                );
                
                return new BotCommandResult(result.Success, result.ErrorMessage);
            }
            
            return new BotCommandResult(false, "Unknown command");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process user command. SyncId: {SyncId:l}", botRequest.SyncId);
            return new BotCommandResult(false, ex.Message);
        }
    }

    private async Task<BotCommandResult> HandleSystemCommand(
        BotCommandRequest botRequest, 
        CancellationToken cancellationToken)
    {
        if (botRequest.CommandBody?.Equals("system:chat_created", StringComparison.OrdinalIgnoreCase) == true)
        {
            // _logger.LogInformation("Chat created event detected. SyncId: {SyncId:l}, ChatId: {ChatId:l}", 
            //     botRequest.SyncId, botRequest.GroupChatId);
            //
            // var chatCreatedRequest = new ChatCreatedRequest(
            //     ChatId: botRequest.GroupChatId,
            //     UserId: botRequest.UserHuid
            // );
            //
            // var result = await _handleChatCreatedUseCase.ExecuteAsync(chatCreatedRequest, cancellationToken);
        }
        return new BotCommandResult(true);
    }
    
    private static bool IsStartCommand(BotCommandRequest request)
    {
        return request.CommandBody.Trim().Equals("/start", StringComparison.OrdinalIgnoreCase) ||
               request.CommandBody.Trim().Equals("start", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsButtonCallback(BotCommandRequest request)
    {
        return request.CommandBody.Contains(':') && request.CommandBody.Split(':').Length == 3;
    }

    private static bool IsInlineEnrollmentCode(BotCommandRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CommandBody))
        {
            return false;
        }

        return InlineEnrollmentRegex().IsMatch(request.CommandBody.Trim());
    }

    private static string? ExtractRequestId(BotCommandRequest request)
    {
        if (request.CommandData?.TryGetValue("command", out var commandObj) == true)
        {
            var commandValue = commandObj?.ToString();
            if (!string.IsNullOrEmpty(commandValue))
            {
                var match = RequestRegex().Match(commandValue);
                if (match is { Success: true, Groups.Count: > 1 })
                {
                    return match.Groups[1].Value;
                }
            }
        }

        return null;
    }

    [GeneratedRegex(@"^/?req=([^\s]+)$")]
    private static partial Regex RequestRegex();
    
    [GeneratedRegex(@"^\d{6}$")]
    private static partial Regex InlineEnrollmentRegex();
}

using Microsoft.Extensions.Logging;

namespace MF.Express.Bot.Application.UseCases.BotCommands;

public interface IProcessBotCommandUseCase : IUseCase<BotCommandRequest, BotCommandResult>
{
}

public record BotCommandRequest(
    string SyncId,
    string? SourceSyncId,
    string CommandType,
    string CommandBody,
    Dictionary<string, object>? CommandData,
    Dictionary<string, object>? CommandMetadata,
    string? UserHuid,
    string? GroupChatId,
    string? ChatType,
    string? Username,
    string? AdLogin,
    string? AdDomain,
    bool? IsAdmin,
    bool? IsCreator,
    string? Device,
    string? DeviceSoftware,
    string? Platform,
    string? AppVersion,
    string? Locale,
    string Host,
    string BotId,
    int ProtoVersion
);

public record BotCommandResult(
    bool Success = true,
    string? ErrorMessage = null
);

public class ProcessBotCommandUseCase : IProcessBotCommandUseCase
{
    private readonly IProcessUserCommandUseCase _processUserCommandUseCase;
    private readonly IProcessSystemCommandUseCase _processSystemCommandUseCase;
    private readonly ILogger<ProcessBotCommandUseCase> _logger;

    public ProcessBotCommandUseCase(
        IProcessUserCommandUseCase processUserCommandUseCase,
        IProcessSystemCommandUseCase processSystemCommandUseCase,
        ILogger<ProcessBotCommandUseCase> logger)
    {
        _processUserCommandUseCase = processUserCommandUseCase;
        _processSystemCommandUseCase = processSystemCommandUseCase;
        _logger = logger;
    }

    public async Task<BotCommandResult> ExecuteAsync(
        BotCommandRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            return request.CommandType.ToLowerInvariant() switch
            {
                "user" => await HandleUserCommand(request, cancellationToken),
                "system" => await HandleSystemCommand(request, cancellationToken),
                _ => new BotCommandResult()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке BotX команды {SyncId} типа {CommandType}", 
                request.SyncId, request.CommandType);
            
            return new BotCommandResult(Success: true);
        }
    }

    private async Task<BotCommandResult> HandleUserCommand(
        BotCommandRequest request, 
        CancellationToken cancellationToken)
    {
        var userRequest = new UserCommandRequest(
            SyncId: request.SyncId,
            SourceSyncId: request.SourceSyncId,
            CommandBody: request.CommandBody,
            CommandData: request.CommandData,
            CommandMetadata: request.CommandMetadata,
            UserHuid: request.UserHuid,
            GroupChatId: request.GroupChatId,
            ChatType: request.ChatType,
            Username: request.Username,
            AdLogin: request.AdLogin,
            AdDomain: request.AdDomain,
            Device: request.Device,
            DeviceSoftware: request.DeviceSoftware,
            Platform: request.Platform,
            AppVersion: request.AppVersion,
            Locale: request.Locale
        );

        var result = await _processUserCommandUseCase.ExecuteAsync(userRequest, cancellationToken);
        return new BotCommandResult(result.Success, result.ErrorMessage);
    }

    private async Task<BotCommandResult> HandleSystemCommand(
        BotCommandRequest request, 
        CancellationToken cancellationToken)
    {
        var systemRequest = new SystemCommandRequest(
            SyncId: request.SyncId,
            CommandBody: request.CommandBody,
            GroupChatId: request.GroupChatId,
            ChatType: request.ChatType,
            UserHuid: request.UserHuid,
            Host: request.Host,
            ProtoVersion: request.ProtoVersion
        );

        var result = await _processSystemCommandUseCase.ExecuteAsync(systemRequest, cancellationToken);
        return new BotCommandResult(result.Success, result.ErrorMessage);
    }
}

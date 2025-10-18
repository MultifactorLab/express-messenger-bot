using Microsoft.Extensions.Logging;

namespace MF.Express.Bot.Application.UseCases.BotCommands;

public interface IProcessSystemCommandUseCase : IUseCase<SystemCommandRequest, SystemCommandResult>
{
}

public record SystemCommandRequest(
    string SyncId,
    string CommandBody,
    string? GroupChatId,
    string? ChatType,
    string? UserHuid,
    string? Host,
    int ProtoVersion
);

public record SystemCommandResult(
    bool Success,
    string? ErrorMessage = null
);

public class ProcessSystemCommandUseCase : IProcessSystemCommandUseCase
{
    private readonly IHandleChatCreatedUseCase _handleChatCreatedUseCase;
    private readonly ILogger<ProcessSystemCommandUseCase> _logger;

    public ProcessSystemCommandUseCase(
        IHandleChatCreatedUseCase handleChatCreatedUseCase,
        ILogger<ProcessSystemCommandUseCase> logger)
    {
        _handleChatCreatedUseCase = handleChatCreatedUseCase;
        _logger = logger;
    }

    public async Task<SystemCommandResult> ExecuteAsync(
        SystemCommandRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Обработка системной команды: {Body}", request.CommandBody);

        try
        {
            if (request.CommandBody?.Equals("system:chat_created", StringComparison.OrdinalIgnoreCase) == true)
            {
                _logger.LogInformation("Обнаружено событие создания чата: {SyncId} в чате {ChatId}", 
                    request.SyncId, request.GroupChatId);

                var chatCreatedRequest = new ChatCreatedRequest(
                    ChatId: request.GroupChatId ?? "private",
                    UserId: request.UserHuid,
                    ChatType: request.ChatType,
                    Host: request.Host,
                    ProtoVersion: request.ProtoVersion
                );

                var result = await _handleChatCreatedUseCase.ExecuteAsync(chatCreatedRequest, cancellationToken);
                return new SystemCommandResult(result.Success, result.ErrorMessage);
            }

            _logger.LogInformation("Получена неизвестная системная команда: {Body}", request.CommandBody);
            return new SystemCommandResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке системной команды {SyncId}", request.SyncId);
            return new SystemCommandResult(false, ex.Message);
        }
    }
}

using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Application.Services;
using Microsoft.Extensions.Logging;

namespace MF.Express.Bot.Application.UseCases.BotCommands;

public interface IProcessIncomingMessageUseCase : IUseCase<IncomingMessageRequest, IncomingMessageResult>
{
}

public record IncomingMessageRequest(
    string ChatId,
    string UserId,
    string Text,
    string MessageId,
    string? Username = null,
    string? FirstName = null,
    string? LastName = null,
    Dictionary<string, object>? Metadata = null
);

public record IncomingMessageResult(
    bool Success,
    string? ErrorMessage = null
);

public class ProcessIncomingMessageUseCase : IProcessIncomingMessageUseCase
{
    private readonly IMessageProcessingService _messageProcessingService;
    private readonly ILogger<ProcessIncomingMessageUseCase> _logger;

    public ProcessIncomingMessageUseCase(
        IMessageProcessingService messageProcessingService,
        ILogger<ProcessIncomingMessageUseCase> logger)
    {
        _messageProcessingService = messageProcessingService;
        _logger = logger;
    }

    public async Task<IncomingMessageResult> ExecuteAsync(
        IncomingMessageRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Обработка входящего сообщения от {UserId} в чате {ChatId}: {Text}", 
            request.UserId, request.ChatId, request.Text);

        try
        {
            await _messageProcessingService.ProcessIncomingMessageAsync(
                chatId: request.ChatId,
                userId: request.UserId,
                text: request.Text,
                messageId: request.MessageId,
                username: request.Username,
                firstName: request.FirstName,
                lastName: request.LastName,
                metadata: request.Metadata,
                cancellationToken: cancellationToken
            );

            return new IncomingMessageResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке сообщения {MessageId}", request.MessageId);
            return new IncomingMessageResult(false, ex.Message);
        }
    }
}

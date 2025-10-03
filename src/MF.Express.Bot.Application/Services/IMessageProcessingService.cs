using MF.Express.Bot.Application.Models.Common;

namespace MF.Express.Bot.Application.Services;

public interface IMessageProcessingService
{
    Task<CommandProcessedResponse> ProcessIncomingMessageAsync(
        string chatId,
        string userId,
        string text,
        string messageId,
        string? username = null,
        string? firstName = null,
        string? lastName = null,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default);
}


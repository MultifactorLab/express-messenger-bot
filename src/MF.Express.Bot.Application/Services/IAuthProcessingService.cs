using MF.Express.Bot.Application.Models.BotCommand;
using MF.Express.Bot.Application.Models.Common;

namespace MF.Express.Bot.Application.Services;

public interface IAuthProcessingService
{
    Task<CommandProcessedResponse> ProcessAuthCallbackAsync(
        string callbackId,
        string authRequestId,
        string userId,
        string chatId,
        AuthAction action,
        string? messageId = null,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default);
}


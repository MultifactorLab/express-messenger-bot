using MF.Express.Bot.Application.Models.BotCommand;
using MF.Express.Bot.Application.Models.Common;

namespace MF.Express.Bot.Application.Services;

public interface IAuthProcessingService
{
    Task<CommandProcessedResponse> ProcessAuthCallbackAsync(
        string callbackData,
        string chatId,
        CancellationToken cancellationToken = default);
}


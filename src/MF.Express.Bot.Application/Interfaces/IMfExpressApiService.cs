using MF.Express.Bot.Application.UseCases.BotCommands;

namespace MF.Express.Bot.Application.Interfaces;

public interface IMfExpressApiService
{
    Task<bool> SendAuthCallbackAsync(
        string callbackData,
        string chatId,
        CancellationToken cancellationToken = default);
    
    Task<bool> SendChatCreatedCallbackAsync(
        BotCommandRequest botRequest,
        string authRequestId,
        CancellationToken cancellationToken = default);
}


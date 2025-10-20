namespace MF.Express.Bot.Application.Interfaces;

public interface IMfExpressApiService
{
    Task<bool> SendAuthCallbackAsync(
        string callbackData,
        string chatId,
        CancellationToken cancellationToken = default);
    
    Task<bool> SendStartCommandAsync(
        string chatId,
        string expressUserId,
        string botId,
        string requestId,
        string username,
        string device,
        string locale,
        CancellationToken cancellationToken = default);
}


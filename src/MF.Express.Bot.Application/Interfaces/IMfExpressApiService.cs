namespace MF.Express.Bot.Application.Interfaces;

public interface IMfExpressApiService
{
    Task<bool> SendAuthCallbackAsync(
        string callbackData,
        string chatId,
        CancellationToken cancellationToken = default);
}


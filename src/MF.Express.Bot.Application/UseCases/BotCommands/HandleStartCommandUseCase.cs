using MF.Express.Bot.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MF.Express.Bot.Application.UseCases.BotCommands;

public interface IHandleStartCommandUseCase : IUseCase<StartCommandRequest, StartCommandResult>
{
}
public class HandleStartCommandUseCase : IHandleStartCommandUseCase
{
    private readonly IMfExpressApiService _mfApiService;
    private readonly ILogger<HandleStartCommandUseCase> _logger;

    public HandleStartCommandUseCase(
        IMfExpressApiService mfApiService,
        ILogger<HandleStartCommandUseCase> logger)
    {
        _mfApiService = mfApiService;
        _logger = logger;
    }

    public async Task<StartCommandResult> ExecuteAsync(
        StartCommandRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Обработка команды /start от ExpressUserId {ExpressUserId} в чате {ChatId} с requestId {RequestId}", 
            request.ExpressUserId, request.ChatId, request.RequestId);

        try
        {
            var success = await _mfApiService.SendChatCreatedCallbackAsync(
                request.ChatId,
                request.ExpressUserId,
                request.BotId,
                request.RequestId,
                request.Username ?? string.Empty,
                request.Device ?? string.Empty,
                request.Locale ?? string.Empty,
                cancellationToken);

            if (success)
            {
                _logger.LogInformation("Данные ExpressUserId {ExpressUserId} успешно отправлены в чат {ChatId} с requestId {RequestId}", 
                    request.ExpressUserId, request.ChatId, request.RequestId);
                return new StartCommandResult(true);
            }
            else
            {
                _logger.LogWarning("Не удалось отправить данные ExpressUserId {ExpressUserId} в чат {ChatId}", 
                    request.ExpressUserId, request.ChatId);
                return new StartCommandResult(false, "Не удалось отправить сообщение");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке команды /start для ExpressUserId {ExpressUserId}", request.ExpressUserId);
            return new StartCommandResult(false, ex.Message);
        }
    }
}


public record StartCommandRequest(
    string ExpressUserId,
    string ChatId,
    string BotId,
    string RequestId,
    string? Username = null,
    string? Device = null,
    string? Locale = null
);

public record StartCommandResult(
    bool Success,
    string? ErrorMessage = null
);
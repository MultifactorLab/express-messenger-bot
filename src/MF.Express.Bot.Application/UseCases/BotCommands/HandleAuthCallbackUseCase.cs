using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Application.Models.BotCommand;
using MF.Express.Bot.Application.Services;
using Microsoft.Extensions.Logging;

namespace MF.Express.Bot.Application.UseCases.BotCommands;

public interface IHandleAuthCallbackUseCase : IUseCase<AuthCallbackRequest, AuthCallbackResult>
{
}

public record AuthCallbackRequest(
    string CallbackData,
    string ChatId
);

public record AuthCallbackResult(
    bool Success,
    string? ErrorMessage = null
);

public class HandleAuthCallbackUseCase : IHandleAuthCallbackUseCase
{
    private readonly IAuthProcessingService _authProcessingService;
    private readonly ILogger<HandleAuthCallbackUseCase> _logger;

    public HandleAuthCallbackUseCase(
        IAuthProcessingService authProcessingService,
        ILogger<HandleAuthCallbackUseCase> logger)
    {
        _authProcessingService = authProcessingService;
        _logger = logger;
    }

    public async Task<AuthCallbackResult> ExecuteAsync(
        AuthCallbackRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Обработка callback от кнопки авторизации. ChatId: {ChatId}", request.ChatId);

        try
        {
            await _authProcessingService.ProcessAuthCallbackAsync(
                callbackData: request.CallbackData,
                chatId: request.ChatId,
                cancellationToken: cancellationToken
            );

            return new AuthCallbackResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке callback'а авторизации");
            return new AuthCallbackResult(false, ex.Message);
        }
    }
}

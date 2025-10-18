using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Application.Models.BotCommand;
using MF.Express.Bot.Application.Services;
using Microsoft.Extensions.Logging;

namespace MF.Express.Bot.Application.UseCases.BotCommands;

public interface IHandleAuthCallbackUseCase : IUseCase<AuthCallbackRequest, AuthCallbackResult>
{
}

public record AuthCallbackRequest(
    string CallbackId,
    string AuthRequestId,
    string UserId,
    string ChatId,
    AuthAction Action,
    string? MessageId = null,
    Dictionary<string, object>? Metadata = null
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
        _logger.LogInformation("Обработка callback от кнопки авторизации: {AuthRequestId}, действие: {Action}, пользователь: {UserId}", 
            request.AuthRequestId, request.Action, request.UserId);

        try
        {
            await _authProcessingService.ProcessAuthCallbackAsync(
                callbackId: request.CallbackId,
                authRequestId: request.AuthRequestId,
                userId: request.UserId,
                chatId: request.ChatId,
                action: request.Action,
                messageId: request.MessageId,
                metadata: request.Metadata,
                cancellationToken: cancellationToken
            );

            return new AuthCallbackResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке callback'а авторизации {AuthRequestId}", request.AuthRequestId);
            return new AuthCallbackResult(false, ex.Message);
        }
    }
}

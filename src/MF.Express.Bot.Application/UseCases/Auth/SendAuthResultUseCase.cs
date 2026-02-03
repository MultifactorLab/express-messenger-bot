using MF.Express.Bot.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MF.Express.Bot.Application.UseCases.Auth;

public record SendAuthResultRequest(
    string ChatId,
    string Message,
    bool Success
);

public record SendAuthResultResult(
    bool Success,
    string? ErrorMessage = null,
    DateTime? Timestamp = null,
    SendAuthErrorType? ErrorType = null
);

public class SendAuthResultUseCase : IUseCase<SendAuthResultRequest, SendAuthResultResult>
{
    private readonly IBotXApiService _botXApiService;
    private readonly ILogger<SendAuthResultUseCase> _logger;

    public SendAuthResultUseCase(
        IBotXApiService botXApiService,
        ILogger<SendAuthResultUseCase> logger)
    {
        _botXApiService = botXApiService;
        _logger = logger;
    }

    public async Task<SendAuthResultResult> ExecuteAsync(
        SendAuthResultRequest botRequest, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending auth result. ChatId: {ChatId:l}", botRequest.ChatId);

            var success = await _botXApiService.SendTextMessageAsync(
                botRequest.ChatId,
                botRequest.Message,
                cancellationToken);

            if (success)
            {
                return new SendAuthResultResult(
                    Success: true,
                    Timestamp: DateTime.UtcNow);
            }
            else
            {
                return new SendAuthResultResult(
                    Success: false,
                    ErrorMessage: "Не удалось отправить сообщение",
                    Timestamp: DateTime.UtcNow,
                    ErrorType: SendAuthErrorType.ExternalServiceError);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send auth result");

            return new SendAuthResultResult(
                Success: false,
                ErrorMessage: ex.Message,
                Timestamp: DateTime.UtcNow,
                ErrorType: SendAuthErrorType.InternalError);
        }
    }
}



using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Application.UseCases.Auth;
using Microsoft.Extensions.Logging;

namespace MF.Express.Bot.Application.UseCases.Greeting;

public record SendGreetingRequest(
    string ChatId,
    string Message
);

public record SendGreetingResult(
    bool Success,
    string? ErrorMessage = null,
    DateTime? Timestamp = null,
    SendAuthErrorType? ErrorType = null
);

public class SendGreetingUseCase : IUseCase<SendGreetingRequest, SendGreetingResult>
{
    private readonly IBotXApiService _botXApiService;
    private readonly ILogger<SendGreetingUseCase> _logger;

    public SendGreetingUseCase(
        IBotXApiService botXApiService,
        ILogger<SendGreetingUseCase> logger)
    {
        _botXApiService = botXApiService;
        _logger = logger;
    }

    public async Task<SendGreetingResult> ExecuteAsync(
        SendGreetingRequest botRequest, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending greeting message. ChatId: {ChatId:l}", botRequest.ChatId);

            var success = await _botXApiService.SendTextMessageAsync(
                botRequest.ChatId,
                botRequest.Message,
                cancellationToken);

            if (success)
            {
                return new SendGreetingResult(
                    Success: true,
                    Timestamp: DateTime.UtcNow);
            }
            else
            {
                return new SendGreetingResult(
                    Success: false,
                    ErrorMessage: "Failed to send greeting message",
                    Timestamp: DateTime.UtcNow,
                    ErrorType: SendAuthErrorType.ExternalServiceError);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send greeting message");

            return new SendGreetingResult(
                Success: false,
                ErrorMessage: ex.Message,
                Timestamp: DateTime.UtcNow,
                ErrorType: SendAuthErrorType.InternalError);
        }
    }
}


using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Application.Models.BotX;
using Microsoft.Extensions.Logging;

namespace MF.Express.Bot.Application.UseCases.Auth;

public record SendAuthRequestRequest(
    string ChatId,
    string UserId,
    string AuthRequestId,
    string Message,
    string ApproveButtonText,
    string RejectButtonText,
    string ApproveButtonCallbackData,
    string RejectButtonCallbackData
);

public record SendAuthRequestResult(
    bool Success,
    string? ErrorMessage = null,
    DateTime? Timestamp = null,
    SendAuthErrorType? ErrorType = null
);

public enum SendAuthErrorType
{
    ValidationError,
    ExternalServiceError,
    InternalError
}

public class SendAuthRequestUseCase : IUseCase<SendAuthRequestRequest, SendAuthRequestResult>
{
    private readonly IBotXApiService _botXApiService;
    private readonly ILogger<SendAuthRequestUseCase> _logger;

    public SendAuthRequestUseCase(
        IBotXApiService botXApiService,
        ILogger<SendAuthRequestUseCase> logger)
    {
        _botXApiService = botXApiService;
        _logger = logger;
    }

    public async Task<SendAuthRequestResult> ExecuteAsync(
        SendAuthRequestRequest botRequest, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending auth request. AuthRequestId: {AuthRequestId:l}, UserId: {UserId:l}, ChatId: {ChatId:l}",
                botRequest.AuthRequestId, botRequest.UserId, botRequest.ChatId);

            var messageText = botRequest.Message;
            var inlineKeyboard = CreateAuthButtons(
                botRequest.ApproveButtonText, 
                botRequest.ApproveButtonCallbackData,
                botRequest.RejectButtonText,
                botRequest.RejectButtonCallbackData);

            var success = await _botXApiService.SendMessageWithInlineKeyboardAsync(
                botRequest.ChatId,
                messageText,
                inlineKeyboard,
                cancellationToken);

            if (success)
            {
                return new SendAuthRequestResult(
                    Success: true,
                    Timestamp: DateTime.UtcNow);
            }
            else
            {
                return new SendAuthRequestResult(
                    Success: false,
                    ErrorMessage: "Не удалось отправить сообщение",
                    Timestamp: DateTime.UtcNow,
                    ErrorType: SendAuthErrorType.ExternalServiceError);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send auth request. AuthRequestId: {AuthRequestId:l}", botRequest.AuthRequestId);

            return new SendAuthRequestResult(
                Success: false,
                ErrorMessage: ex.Message,
                Timestamp: DateTime.UtcNow,
                ErrorType: SendAuthErrorType.InternalError);
        }
    }

    private static List<List<InlineKeyboardButtonModel>> CreateAuthButtons(
        string approveText,
        string approveCallbackData,
        string rejectText,
        string rejectCallbackData)
    {
        return
        [
            new List<InlineKeyboardButtonModel>()
            {
                new(approveText, approveCallbackData),
                new(rejectText, rejectCallbackData)    
            }
        ];
    }
}

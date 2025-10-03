using Microsoft.Extensions.Logging;
using MF.Express.Bot.Application.Models.Auth;
using MF.Express.Bot.Application.Models.Common;
using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Application.Services;

namespace MF.Express.Bot.Infrastructure.Services;

public class MessageProcessingService : IMessageProcessingService
{
    private readonly IMultifactorApiService _multifactorApiService;
    private readonly IBotXApiService _botXApiService;
    private readonly ILogger<MessageProcessingService> _logger;

    public MessageProcessingService(
        IMultifactorApiService multifactorApiService,
        IBotXApiService botXApiService,
        ILogger<MessageProcessingService> logger)
    {
        _multifactorApiService = multifactorApiService;
        _botXApiService = botXApiService;
        _logger = logger;
    }

    public async Task<CommandProcessedResponse> ProcessIncomingMessageAsync(
        string chatId,
        string userId,
        string text,
        string messageId,
        string? username = null,
        string? firstName = null,
        string? lastName = null,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Обработка входящего сообщения от пользователя {UserId} в чате {ChatId}", 
                userId, chatId);

            var userChatInfo = new UserChatInfoAppModel(
                UserId: userId,
                ChatId: chatId,
                Username: username,
                FirstName: firstName,
                LastName: lastName,
                FirstContactTime: DateTime.UtcNow,
                LastMessage: text,
                Metadata: metadata
            );

            var success = await _multifactorApiService.SendUserChatInfoAsync(userChatInfo, cancellationToken);

            if (!success)
            {
                _logger.LogWarning("Не удалось отправить информацию о пользователе {UserId} в Multifactor API", userId);
                return new CommandProcessedResponse(false, "Ошибка при отправке данных в Multifactor API");
            }

            await _botXApiService.SendTextMessageAsync(
                chatId, 
                "Ваше сообщение получено и обработано.", 
                cancellationToken);

            _logger.LogInformation("Входящее сообщение от пользователя {UserId} успешно обработано", userId);
            return new CommandProcessedResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке входящего сообщения от пользователя {UserId}", userId);
            return new CommandProcessedResponse(false, $"Внутренняя ошибка: {ex.Message}");
        }
    }
}


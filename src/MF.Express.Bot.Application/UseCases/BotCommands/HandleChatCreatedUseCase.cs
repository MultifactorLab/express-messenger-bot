using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Application.Models.BotX;
using Microsoft.Extensions.Logging;

namespace MF.Express.Bot.Application.UseCases.BotCommands;

public interface IHandleChatCreatedUseCase : IUseCase<ChatCreatedRequest, ChatCreatedResult>
{
}

public record ChatCreatedRequest(
    string ChatId,
    string? UserId = null,
    string? ChatType = null,
    string? Host = null,
    int ProtoVersion = 0
);

public record ChatCreatedResult(
    bool Success,
    string? ErrorMessage = null
);

public class HandleChatCreatedUseCase : IHandleChatCreatedUseCase
{
    private readonly IBotXApiService _botXApiService;
    private readonly ILogger<HandleChatCreatedUseCase> _logger;

    public HandleChatCreatedUseCase(
        IBotXApiService botXApiService,
        ILogger<HandleChatCreatedUseCase> logger)
    {
        _botXApiService = botXApiService;
        _logger = logger;
    }

    public async Task<ChatCreatedResult> ExecuteAsync(
        ChatCreatedRequest request, 
        CancellationToken cancellationToken = default)
    {
        var userInfo = string.IsNullOrEmpty(request.UserId) 
            ? "системное событие" 
            : $"пользователь {request.UserId}";
            
        _logger.LogInformation("Обработка события создания чата с ботом: {UserInfo} в чате {ChatId}", 
            userInfo, request.ChatId);

        try
        {
            var welcomeMessage = FormatWelcomeMessage(request);
            var keyboard = CreateStartButton();

            var success = await _botXApiService.SendMessageWithInlineKeyboardAsync(
                request.ChatId, 
                welcomeMessage, 
                keyboard, 
                cancellationToken);

            if (success)
            {
                _logger.LogInformation("Приветственное сообщение отправлено в чат {ChatId} (событие: {UserInfo})", 
                    request.ChatId, userInfo);
                return new ChatCreatedResult(true);
            }
            else
            {
                _logger.LogWarning("Не удалось отправить приветственное сообщение в чат {ChatId} (событие: {UserInfo})", 
                    request.ChatId, userInfo);
                return new ChatCreatedResult(false, "Не удалось отправить приветственное сообщение");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события создания чата {ChatId} (событие: {UserInfo})", 
                request.ChatId, userInfo);
            return new ChatCreatedResult(false, ex.Message);
        }
    }

    private static string FormatWelcomeMessage(ChatCreatedRequest request)
    {
        return $"""
            🎉 **Добро пожаловать в чат с ExpressBot!**
            
            📋 **Информация о чате:**
            • Chat ID: {request.ChatId}
            • Chat Type: {request.ChatType ?? "не указан"}
            • Host: {request.Host ?? "не указан"}
            • Protocol Version: {request.ProtoVersion}
            
            🤖 **Доступные команды:**
            • `/start` - получить ваши данные пользователя
            
            Нажмите кнопку ниже, чтобы начать!
            """;
    }

    private static List<List<InlineKeyboardButtonModel>> CreateStartButton()
    {
        return new List<List<InlineKeyboardButtonModel>>
        {
            new List<InlineKeyboardButtonModel>
            {
                new InlineKeyboardButtonModel("🚀 Начать работу", "/start")
            }
        };
    }
}

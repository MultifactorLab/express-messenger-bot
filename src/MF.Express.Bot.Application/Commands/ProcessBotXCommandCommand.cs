using Microsoft.Extensions.Logging;
using MF.Express.Bot.Application.Models.BotCommand;
using MF.Express.Bot.Application.Models.BotX;
using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Application.Services;

namespace MF.Express.Bot.Application.Commands;

/// <summary>
/// Команда для обработки Bot API v4 команд от BotX
/// </summary>
public record ProcessBotXCommandCommand(
    string SyncId,
    string? SourceSyncId,
    string CommandType,
    string CommandBody,
    Dictionary<string, object>? CommandData,
    Dictionary<string, object>? CommandMetadata,
    string? UserHuid,
    string? GroupChatId,
    string? ChatType,
    string? Username,
    string? AdLogin,
    string? AdDomain,
    bool? IsAdmin,
    bool? IsCreator,
    string? Device,
    string? DeviceSoftware,
    string? Platform,
    string? AppVersion,
    string? Locale,
    string Host,
    string BotId,
    int ProtoVersion
);

/// <summary>
/// Обработчик команд Bot API v4
/// </summary>
public class ProcessBotXCommandHandler : ICommand<ProcessBotXCommandCommand, BotApiResponseAppModel>
{
    private readonly ILogger<ProcessBotXCommandHandler> _logger;
    private readonly IMessageProcessingService _messageService;
    private readonly IAuthProcessingService _authService;
    private readonly IBotXApiService _botXApiService;
    private readonly IMultifactorApiService _multifactorApiService;

    public ProcessBotXCommandHandler(
        ILogger<ProcessBotXCommandHandler> logger,
        IMessageProcessingService messageService,
        IAuthProcessingService authService,
        IBotXApiService botXApiService,
        IMultifactorApiService multifactorApiService)
    {
        _logger = logger;
        _messageService = messageService;
        _authService = authService;
        _botXApiService = botXApiService;
        _multifactorApiService = multifactorApiService;
    }

    public async Task<BotApiResponseAppModel> Handle(ProcessBotXCommandCommand command, CancellationToken cancellationToken)
    {
        try
        {
            return command.CommandType.ToLowerInvariant() switch
            {
                "user" => await HandleUserCommand(command, cancellationToken),
                "system" => await HandleSystemCommand(command, cancellationToken),
                _ => new BotApiResponseAppModel()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке BotX команды {SyncId} типа {CommandType}", 
                command.SyncId, command.CommandType);
            
            return new BotApiResponseAppModel();
        }
    }

    private async Task<BotApiResponseAppModel> HandleUserCommand(ProcessBotXCommandCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Обработка пользовательской команды: {Body} от {UserHuid}", 
            command.CommandBody, command.UserHuid);

        if (IsStartCommand(command))
        {
            return await HandleStartCommand(command, cancellationToken);
        }

        if (IsButtonCallback(command))
        {
            return await HandleButtonCallback(command, cancellationToken);
        }

        await _messageService.ProcessIncomingMessageAsync(
            chatId: command.GroupChatId ?? "private",
            userId: command.UserHuid ?? "unknown",
            text: command.CommandBody,
            messageId: command.SyncId,
            username: command.Username,
            firstName: ExtractFromData(command.CommandData, "first_name"),
            lastName: ExtractFromData(command.CommandData, "last_name"),
            metadata: command.CommandMetadata,
            cancellationToken: cancellationToken
        );

        return new BotApiResponseAppModel();
    }

    private static bool IsButtonCallback(ProcessBotXCommandCommand command)
    {
        return command.CommandData?.ContainsKey("callback_data") == true ||
               command.CommandData?.ContainsKey("button_data") == true ||
               command.CommandBody.StartsWith("callback:", StringComparison.OrdinalIgnoreCase) ||
               command.CommandBody.StartsWith("auth_allow_", StringComparison.OrdinalIgnoreCase) ||
               command.CommandBody.StartsWith("auth_deny_", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<BotApiResponseAppModel> HandleButtonCallback(ProcessBotXCommandCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Обработка callback от кнопки: {Body} от {UserHuid}", 
            command.CommandBody, command.UserHuid);

        try
        {
            var callbackData = ExtractCallbackData(command);
            
            _logger.LogDebug("Извлеченные callback данные: {CallbackData} из CommandBody: {CommandBody}", 
                callbackData, command.CommandBody);
            
            if (string.IsNullOrEmpty(callbackData))
            {
                _logger.LogWarning("Не удалось извлечь данные callback'а из команды {SyncId}. CommandBody: {CommandBody}", 
                    command.SyncId, command.CommandBody);
                return new BotApiResponseAppModel();
            }

            var parts = callbackData.Split(':', 2);
            if (parts.Length != 2)
            {
                _logger.LogWarning("Неверный формат callback данных: {CallbackData}", callbackData);
                return new BotApiResponseAppModel();
            }

            var authRequestId = parts[0];
            var actionStr = parts[1];

            if (!Enum.TryParse<AuthAction>(actionStr, true, out var action))
            {
                _logger.LogWarning("Неизвестное действие callback'а: {Action}", actionStr);
                return new BotApiResponseAppModel();
            }
            await _authService.ProcessAuthCallbackAsync(
                callbackId: command.SyncId,
                authRequestId: authRequestId,
                userId: command.UserHuid ?? "unknown",
                chatId: command.GroupChatId ?? "private",
                action: action,
                messageId: command.SourceSyncId,
                metadata: command.CommandMetadata,
                cancellationToken: cancellationToken
            );

            return new BotApiResponseAppModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке callback'а от кнопки");
            return new BotApiResponseAppModel();
        }
    }

    private static string? ExtractCallbackData(ProcessBotXCommandCommand command)
    {
        if (command.CommandData?.TryGetValue("callback_data", out var callbackObj) == true)
        {
            return callbackObj?.ToString();
        }

        if (command.CommandData?.TryGetValue("button_data", out var buttonObj) == true)
        {
            return buttonObj?.ToString();
        }

        if (command.CommandBody.StartsWith("callback:", StringComparison.OrdinalIgnoreCase))
        {
            return command.CommandBody[9..];
        }

        if (command.CommandBody.StartsWith("auth_allow_", StringComparison.OrdinalIgnoreCase))
        {
            var authRequestId = command.CommandBody[11..];
            return $"{authRequestId}:Allow";
        }

        if (command.CommandBody.StartsWith("auth_deny_", StringComparison.OrdinalIgnoreCase))
        {
            var authRequestId = command.CommandBody[10..];
            return $"{authRequestId}:Deny";
        }

        return null;
    }

    private static string? ExtractFromData(Dictionary<string, object>? data, string key)
    {
        return data?.TryGetValue(key, out var value) == true ? value?.ToString() : null;
    }

    private async Task<BotApiResponseAppModel> HandleChatCreated(ProcessBotXCommandCommand command, CancellationToken cancellationToken)
    {
        var userInfo = string.IsNullOrEmpty(command.UserHuid) ? "системное событие" : $"пользователь {command.UserHuid}";
        _logger.LogInformation("Обработка события создания чата с ботом: {UserInfo} в чате {ChatId}", 
            userInfo, command.GroupChatId);

        try
        {
            var chatId = command.GroupChatId ?? "private";
            
            // Отправляем приветственное сообщение с информацией о чате
            var welcomeMessage = $"""
                🎉 **Добро пожаловать в чат с ExpressBot!**
                
                📋 **Информация о чате:**
                • Chat ID: {chatId}
                • Chat Type: {command.ChatType ?? "не указан"}
                • Host: {command.Host ?? "не указан"}
                • Protocol Version: {command.ProtoVersion}
                
                🤖 **Доступные команды:**
                • `/start` - получить ваши данные пользователя
                
                Нажмите кнопку ниже, чтобы начать!
                """;

            var keyboard = new List<List<InlineKeyboardButtonModel>>
            {
                new List<InlineKeyboardButtonModel>
                {
                    new InlineKeyboardButtonModel("🚀 Начать работу", "/start")
                }
            };

            var success = await _botXApiService.SendMessageWithInlineKeyboardAsync(
                chatId, 
                welcomeMessage, 
                keyboard, 
                cancellationToken);

            if (success)
            {
                _logger.LogInformation("Приветственное сообщение отправлено в чат {ChatId} (событие: {UserInfo})", 
                    chatId, userInfo);
            }
            else
            {
                _logger.LogWarning("Не удалось отправить приветственное сообщение в чат {ChatId} (событие: {UserInfo})", 
                    chatId, userInfo);
            }

            return new BotApiResponseAppModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события создания чата {ChatId} (событие: {UserInfo})", 
                command.GroupChatId, userInfo);
            return new BotApiResponseAppModel();
        }
    }
    
    private async Task<BotApiResponseAppModel> HandleSystemCommand(ProcessBotXCommandCommand command, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Обработка системной команды: {CommandType} {Body}", command.CommandType, command.CommandBody);
       
        if (command.CommandBody?.Equals("system:chat_created", StringComparison.OrdinalIgnoreCase) == true)
        {
            _logger.LogInformation("Обнаружено событие создания чата: {SyncId} в чате {ChatId}", 
                command.SyncId, command.GroupChatId);
            return await HandleChatCreated(command, cancellationToken);
        }
        
        _logger.LogInformation("Получена неизвестная системная команда: {Body}", command.CommandBody);
        return new BotApiResponseAppModel();
    }
    
    private static bool IsStartCommand(ProcessBotXCommandCommand command)
    {
        return command.CommandBody.Trim().Equals("/start", StringComparison.OrdinalIgnoreCase) ||
               command.CommandBody.Trim().Equals("start", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<BotApiResponseAppModel> HandleStartCommand(ProcessBotXCommandCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Обработка команды /start от пользователя {UserHuid} в чате {ChatId}", 
            command.UserHuid, command.GroupChatId);

        try
        {
            var userData = new UserStartCommandAppModel(
                UserId: command.UserHuid ?? "unknown",
                ChatId: command.GroupChatId ?? "private",
                Username: command.Username,
                FirstName: ExtractFromData(command.CommandData, "first_name"),
                LastName: ExtractFromData(command.CommandData, "last_name"),
                AdLogin: command.AdLogin,
                AdDomain: command.AdDomain,
                ChatType: command.ChatType,
                Timestamp: DateTime.UtcNow,
                Platform: command.Platform,
                AppVersion: command.AppVersion,
                Device: command.Device,
                Locale: command.Locale,
                Metadata: command.CommandMetadata
            );

            // отправка в Multifactor API
            // var success = await _multifactorApiService.SendUserStartCommandDataAsync(userData, cancellationToken);

            var userDataMessage = $"""
                📋 **Данные пользователя /start:**
                
                👤 **Пользователь:**
                • ID: {userData.UserId}
                • Username: {userData.Username ?? "не указан"}
                • Имя: {userData.FirstName ?? "не указано"}
                • Фамилия: {userData.LastName ?? "не указана"}
                
                🏢 **AD данные:**
                • Login: {userData.AdLogin ?? "не указан"}
                • Domain: {userData.AdDomain ?? "не указан"}
                
                💬 **Чат:**
                • Chat ID: {userData.ChatId}
                • Chat Type: {userData.ChatType ?? "не указан"}
                
                📱 **Устройство:**
                • Platform: {userData.Platform ?? "не указана"}
                • Device: {userData.Device ?? "не указано"}
                • App Version: {userData.AppVersion ?? "не указана"}
                • Locale: {userData.Locale ?? "не указана"}
                
                🕐 **Время:** {userData.Timestamp:yyyy-MM-dd HH:mm:ss} UTC
                """;

            var chatId = command.GroupChatId ?? "private";
            var success = await _botXApiService.SendTextMessageAsync(chatId, userDataMessage, cancellationToken);

            if (success)
            {
                _logger.LogInformation("Данные пользователя {UserHuid} успешно отправлены в чат {ChatId}", 
                    command.UserHuid, chatId);
            }
            else
            {
                _logger.LogWarning("Не удалось отправить данные пользователя {UserHuid} в чат {ChatId}", 
                    command.UserHuid, chatId);
            }

            return new BotApiResponseAppModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке команды /start для пользователя {UserHuid}", command.UserHuid);

            return new BotApiResponseAppModel();
        }
    }
}

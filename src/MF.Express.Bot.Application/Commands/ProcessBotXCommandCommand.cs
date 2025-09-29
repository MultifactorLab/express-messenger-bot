using Microsoft.Extensions.Logging;
using MF.Express.Bot.Application.DTOs;
using MF.Express.Bot.Application.Interfaces;

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
public class ProcessBotXCommandHandler : ICommand<ProcessBotXCommandCommand, BotApiResponse>
{
    private readonly ILogger<ProcessBotXCommandHandler> _logger;
    private readonly ICommand<ProcessIncomingMessageCommand, CommandProcessedResponse> _messageHandler;
    private readonly ICommand<ProcessAuthCallbackCommand, CommandProcessedResponse> _callbackHandler;
    private readonly IBotXApiService _botXApiService;
    private readonly IMultifactorApiService _multifactorApiService;

    public ProcessBotXCommandHandler(
        ILogger<ProcessBotXCommandHandler> logger,
        ICommand<ProcessIncomingMessageCommand, CommandProcessedResponse> messageHandler,
        ICommand<ProcessAuthCallbackCommand, CommandProcessedResponse> callbackHandler,
        IBotXApiService botXApiService,
        IMultifactorApiService multifactorApiService)
    {
        _logger = logger;
        _messageHandler = messageHandler;
        _callbackHandler = callbackHandler;
        _botXApiService = botXApiService;
        _multifactorApiService = multifactorApiService;
    }

    public async Task<BotApiResponse> Handle(ProcessBotXCommandCommand command, CancellationToken cancellationToken)
    {
        try
        {
            return command.CommandType.ToLowerInvariant() switch
            {
                "user" => await HandleUserCommand(command, cancellationToken),
                "chat_created" => await HandleChatCreated(command, cancellationToken),
                "system" => await HandleSystemCommand(command, cancellationToken),
                _ => new BotApiResponse()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке BotX команды {SyncId} типа {CommandType}", 
                command.SyncId, command.CommandType);
            
            return new BotApiResponse();
        }
    }

    private async Task<BotApiResponse> HandleUserCommand(ProcessBotXCommandCommand command, CancellationToken cancellationToken)
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

        var messageCommand = new ProcessIncomingMessageCommand(
            ChatId: command.GroupChatId ?? "private",
            UserId: command.UserHuid ?? "unknown",
            Text: command.CommandBody,
            Timestamp: DateTime.UtcNow,
            MessageId: command.SyncId,
            Username: command.Username,
            FirstName: ExtractFromData(command.CommandData, "first_name"),
            LastName: ExtractFromData(command.CommandData, "last_name"),
            Metadata: command.CommandMetadata
        );

        await _messageHandler.Handle(messageCommand, cancellationToken);
        return new BotApiResponse();
    }

    private static bool IsButtonCallback(ProcessBotXCommandCommand command)
    {
        return command.CommandData?.ContainsKey("callback_data") == true ||
               command.CommandData?.ContainsKey("button_data") == true ||
               command.CommandBody.StartsWith("callback:", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<BotApiResponse> HandleButtonCallback(ProcessBotXCommandCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Обработка callback от кнопки: {Body} от {UserHuid}", 
            command.CommandBody, command.UserHuid);

        try
        {
            var callbackData = ExtractCallbackData(command);
            
            if (string.IsNullOrEmpty(callbackData))
            {
                _logger.LogWarning("Не удалось извлечь данные callback'а из команды {SyncId}", command.SyncId);
                return new BotApiResponse();
            }

            var parts = callbackData.Split(':', 2);
            if (parts.Length != 2)
            {
                _logger.LogWarning("Неверный формат callback данных: {CallbackData}", callbackData);
                return new BotApiResponse();
            }

            var authRequestId = parts[0];
            var actionStr = parts[1];

            if (!Enum.TryParse<AuthAction>(actionStr, true, out var action))
            {
                _logger.LogWarning("Неизвестное действие callback'а: {Action}", actionStr);
                return new BotApiResponse();
            }

            var callbackCommand = new ProcessAuthCallbackCommand(
                CallbackId: command.SyncId,
                UserId: command.UserHuid ?? "unknown",
                ChatId: command.GroupChatId ?? "private",
                AuthRequestId: authRequestId,
                Action: action,
                Timestamp: DateTime.UtcNow,
                MessageId: command.SourceSyncId,
                Metadata: command.CommandMetadata
            );

            await _callbackHandler.Handle(callbackCommand, cancellationToken);

            return new BotApiResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке callback'а от кнопки");
            return new BotApiResponse();
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

        return null;
    }

    private static string? ExtractFromData(Dictionary<string, object>? data, string key)
    {
        return data?.TryGetValue(key, out var value) == true ? value?.ToString() : null;
    }

    /// <summary>
    /// Обработка события создания чата с ботом
    /// </summary>
    private async Task<BotApiResponse> HandleChatCreated(ProcessBotXCommandCommand command, CancellationToken cancellationToken)
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

            var keyboard = new List<List<InlineKeyboardButton>>
            {
                new List<InlineKeyboardButton>
                {
                    new InlineKeyboardButton("🚀 Начать работу", "/start")
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

            return new BotApiResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события создания чата {ChatId} (событие: {UserInfo})", 
                command.GroupChatId, userInfo);
            return new BotApiResponse();
        }
    }

    /// <summary>
    /// Обработка системных команд
    /// </summary>
    private async Task<BotApiResponse> HandleSystemCommand(ProcessBotXCommandCommand command, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Обработка системной команды: {CommandType} {Body}", command.CommandType, command.CommandBody);
        
        // Проверяем, является ли это событием создания чата
        if (command.CommandBody?.Equals("system:chat_created", StringComparison.OrdinalIgnoreCase) == true)
        {
            _logger.LogInformation("Обнаружено событие создания чата: {SyncId} в чате {ChatId}", 
                command.SyncId, command.GroupChatId);
            return await HandleChatCreated(command, cancellationToken);
        }
        
        // Обработка других системных команд
        _logger.LogInformation("Получена неизвестная системная команда: {Body}", command.CommandBody);
        return new BotApiResponse();
    }

    /// <summary>
    /// Проверяет, является ли команда командой /start
    /// </summary>
    private static bool IsStartCommand(ProcessBotXCommandCommand command)
    {
        return command.CommandBody.Trim().Equals("/start", StringComparison.OrdinalIgnoreCase) ||
               command.CommandBody.Trim().Equals("start", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Обработка команды /start - отправка данных пользователя обратно в чат (тестовый режим)
    /// </summary>
    private async Task<BotApiResponse> HandleStartCommand(ProcessBotXCommandCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Обработка команды /start от пользователя {UserHuid} в чате {ChatId}", 
            command.UserHuid, command.GroupChatId);

        try
        {
            var userData = new UserStartCommandDataDto(
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

            // ВРЕМЕННО ЗАКОММЕНТИРОВАНО: отправка в Multifactor API
            // var success = await _multifactorApiService.SendUserStartCommandDataAsync(userData, cancellationToken);

            // Формируем сообщение с данными пользователя для отправки в чат
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

            return new BotApiResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке команды /start для пользователя {UserHuid}", command.UserHuid);
            
            try
            {
                var chatId = command.GroupChatId ?? "private";
                await _botXApiService.SendTextMessageAsync(chatId, 
                    "❌ Произошла внутренняя ошибка. Обратитесь к администратору.", cancellationToken);
            }
            catch
            {
                // Игнорируем ошибки отправки сообщения об ошибке
            }

            return new BotApiResponse();
        }
    }
}

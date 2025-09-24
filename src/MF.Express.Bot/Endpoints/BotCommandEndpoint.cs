using MF.Express.Bot.Application.Commands;
using MF.Express.Bot.Application.DTOs;
using Microsoft.Extensions.Options;
using MF.Express.Bot.Infrastructure.Configuration;

namespace MF.Express.Bot.Api.Endpoints;

/// <summary>
/// Bot API v4 endpoint для обработки команд от BotX
/// Согласно документации https://docs.express.ms/chatbots/developer-guide/api/bot-api/command/
/// </summary>
public class BotCommandEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/command", HandleAsync)
            .WithName("HandleBotXCommand")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Bot API v4 - Обработка команд от BotX";
                operation.Description = "Основной endpoint для получения команд от Express.MS BotX сервера";
                return operation;
            })
            .Produces<BotApiResponse>(202)
            .Produces<BotApiErrorResponse>(503)
            .WithTags("Bot API v4");
    }

    private static async Task<IResult> HandleAsync(
        BotXCommandDto command,
        ICommand<ProcessBotXCommandCommand, BotApiResponse> handler,
        IOptions<ExpressBotConfiguration> config,
        ILogger<BotCommandEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            // Проверяем, что команда предназначена для нашего бота
            if (command.BotId != config.Value.BotId)
            {
                logger.LogWarning("Получена команда для другого бота: {CommandBotId}, ожидался: {ConfigBotId}", 
                    command.BotId, config.Value.BotId);
                
                return Results.BadRequest(new { error = "Invalid bot_id" });
            }

            // Логируем полученную команду
            if (config.Value.EnableDetailedLogging)
            {
                logger.LogInformation("Получена команда Bot API v4: {CommandType} {Body} от пользователя {UserHuid} в чате {ChatId}",
                    command.Command.CommandType, 
                    command.Command.Body,
                    command.From.UserHuid ?? "system",
                    command.From.GroupChatId ?? "private");
            }

            // Создаем команду для обработки
            var processingCommand = new ProcessBotXCommandCommand(
                SyncId: command.SyncId,
                SourceSyncId: command.SourceSyncId,
                CommandType: command.Command.CommandType,
                CommandBody: command.Command.Body,
                CommandData: command.Command.Data,
                CommandMetadata: command.Command.Metadata,
                UserHuid: command.From.UserHuid,
                GroupChatId: command.From.GroupChatId,
                ChatType: command.From.ChatType,
                Username: command.From.Username,
                AdLogin: command.From.AdLogin,
                AdDomain: command.From.AdDomain,
                IsAdmin: command.From.IsAdmin,
                IsCreator: command.From.IsCreator,
                Device: command.From.Device,
                DeviceSoftware: command.From.DeviceSoftware,
                Platform: command.From.Platform,
                AppVersion: command.From.AppVersion,
                Locale: command.From.Locale,
                Host: command.From.Host,
                BotId: command.BotId,
                ProtoVersion: command.ProtoVersion
            );

            // Обрабатываем команду
            var result = await handler.Handle(processingCommand, ct);

            logger.LogDebug("Команда Bot API v4 успешно обработана: {SyncId}", command.SyncId);

            // Возвращаем стандартный ответ Bot API v4 - 202 Accepted
            return Results.Accepted();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обработке команды Bot API v4: {SyncId}", command.SyncId);

            // В случае ошибки все равно возвращаем 202 Accepted 
            // согласно спецификации Bot API v4
            return Results.Accepted();
        }
    }
}

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

    public ProcessBotXCommandHandler(
        ILogger<ProcessBotXCommandHandler> logger,
        ICommand<ProcessIncomingMessageCommand, CommandProcessedResponse> messageHandler,
        ICommand<ProcessAuthCallbackCommand, CommandProcessedResponse> callbackHandler)
    {
        _logger = logger;
        _messageHandler = messageHandler;
        _callbackHandler = callbackHandler;
    }

    public async Task<BotApiResponse> Handle(ProcessBotXCommandCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // Обрабатываем только пользовательские команды (user)
            // Системные команды игнорируем согласно упрощенной архитектуре
            return command.CommandType.ToLowerInvariant() switch
            {
                "user" => await HandleUserCommand(command, cancellationToken),
                _ => new BotApiResponse() // Все остальные команды игнорируем
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

        // Проверяем, является ли это callback'ом от кнопки
        if (IsButtonCallback(command))
        {
            return await HandleButtonCallback(command, cancellationToken);
        }

        // Обычное сообщение пользователя - функция 1
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
        // Callback'и от кнопок обычно содержат специальные данные
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
            // Извлекаем данные callback'а
            var callbackData = ExtractCallbackData(command);
            
            if (string.IsNullOrEmpty(callbackData))
            {
                _logger.LogWarning("Не удалось извлечь данные callback'а из команды {SyncId}", command.SyncId);
                return new BotApiResponse();
            }

            // Парсим callback данные (предполагаем формат: "auth_request_id:action")
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

            // Создаем команду для обработки callback'а - функция 3
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
        // Пытаемся извлечь callback данные из разных возможных мест
        if (command.CommandData?.TryGetValue("callback_data", out var callbackObj) == true)
        {
            return callbackObj?.ToString();
        }

        if (command.CommandData?.TryGetValue("button_data", out var buttonObj) == true)
        {
            return buttonObj?.ToString();
        }

        // Если данные в теле команды (формат: "callback:data")
        if (command.CommandBody.StartsWith("callback:", StringComparison.OrdinalIgnoreCase))
        {
            return command.CommandBody[9..]; // Убираем "callback:"
        }

        return null;
    }

    private static string? ExtractFromData(Dictionary<string, object>? data, string key)
    {
        return data?.TryGetValue(key, out var value) == true ? value?.ToString() : null;
    }

}

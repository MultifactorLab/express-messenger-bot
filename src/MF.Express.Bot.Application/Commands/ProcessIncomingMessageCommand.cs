using Microsoft.Extensions.Logging;
using MF.Express.Bot.Application.DTOs;
using MF.Express.Bot.Application.Interfaces;

namespace MF.Express.Bot.Application.Commands;

/// <summary>
/// Команда для обработки входящего сообщения от пользователя
/// </summary>
public record ProcessIncomingMessageCommand(
    string ChatId,
    string UserId,
    string Text,
    DateTime Timestamp,
    string MessageId,
    string? Username = null,
    string? FirstName = null,
    string? LastName = null,
    Dictionary<string, object>? Metadata = null
);

/// <summary>
/// Обработчик команды ProcessIncomingMessage
/// </summary>
public class ProcessIncomingMessageHandler : ICommand<ProcessIncomingMessageCommand, CommandProcessedResponse>
{
    private readonly IMultifactorApiService _multifactorApiService;
    private readonly IBotXApiService _expressBotService;
    private readonly ILogger<ProcessIncomingMessageHandler> _logger;

    public ProcessIncomingMessageHandler(
        IMultifactorApiService multifactorApiService,
        IBotXApiService expressBotService,
        ILogger<ProcessIncomingMessageHandler> logger)
    {
        _multifactorApiService = multifactorApiService;
        _expressBotService = expressBotService;
        _logger = logger;
    }

    public async Task<CommandProcessedResponse> Handle(ProcessIncomingMessageCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Обработка входящего сообщения от пользователя {UserId} в чате {ChatId}", 
                command.UserId, command.ChatId);

            // Создаем информацию о пользователе и чате
            var userChatInfo = new UserChatInfoDto(
                UserId: command.UserId,
                ChatId: command.ChatId,
                Username: command.Username,
                FirstName: command.FirstName,
                LastName: command.LastName,
                FirstContactTime: command.Timestamp,
                LastMessage: command.Text,
                Metadata: command.Metadata
            );

            // Отправляем информацию в Multifactor API
            var success = await _multifactorApiService.SendUserChatInfoAsync(userChatInfo, cancellationToken);

            if (!success)
            {
                _logger.LogWarning("Не удалось отправить информацию о пользователе {UserId} в Multifactor API", command.UserId);
                return new CommandProcessedResponse(false, "Ошибка при отправке данных в Multifactor API");
            }

            // Опционально: отправляем подтверждающее сообщение пользователю
            await _expressBotService.SendTextMessageAsync(
                command.ChatId, 
                "Ваше сообщение получено и обработано.", 
                cancellationToken);

            _logger.LogInformation("Входящее сообщение от пользователя {UserId} успешно обработано", command.UserId);
            return new CommandProcessedResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке входящего сообщения от пользователя {UserId}", command.UserId);
            return new CommandProcessedResponse(false, $"Внутренняя ошибка: {ex.Message}");
        }
    }
}

using MF.Express.Bot.Application.DTOs;
using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MF.Express.Bot.Application.Commands;

/// <summary>
/// Команда отправки сообщения через Express API
/// </summary>
public record SendMessageCommand(
    string ChatId,
    string Text,
    MessageType Type = MessageType.Text,
    string? ReplyToMessageId = null,
    Dictionary<string, object>? Metadata = null
);

/// <summary>
/// Обработчик команды отправки сообщения
/// </summary>
public class SendMessageHandler : ICommand<SendMessageCommand, SendMessageResultDto>
{
    private readonly IExpressBotService _expressBotService;
    private readonly ILogger<SendMessageHandler> _logger;

    public SendMessageHandler(
        IExpressBotService expressBotService,
        ILogger<SendMessageHandler> logger)
    {
        _expressBotService = expressBotService;
        _logger = logger;
    }

    public async Task<SendMessageResultDto> Handle(SendMessageCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Отправка сообщения в чат {ChatId}", command.ChatId);

            // Отправляем сообщение через Express API
            var apiResult = await _expressBotService.SendTextMessageAsync(
                command.ChatId, 
                command.Text, 
                cancellationToken);

            if (apiResult.Success)
            {
                _logger.LogInformation("Сообщение успешно отправлено. MessageId: {MessageId}", apiResult.MessageId);
            }
            else
            {
                _logger.LogWarning("Не удалось отправить сообщение: {Error}", apiResult.ErrorMessage);
            }

            return apiResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке сообщения в чат {ChatId}", command.ChatId);
            return new SendMessageResultDto(string.Empty, false, ex.Message, DateTime.UtcNow);
        }
    }
}

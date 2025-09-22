using MF.Express.Bot.Application.DTOs;
using MF.Express.Bot.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MF.Express.Bot.Application.Commands;

/// <summary>
/// Команда регистрации пользователя в чате с ботом
/// </summary>
public record RegisterUserCommand(
    string ChatId,
    string UserId,
    string? Username = null,
    string? FirstName = null,
    string? LastName = null,
    Dictionary<string, object>? Metadata = null
);

/// <summary>
/// Обработчик команды регистрации пользователя
/// </summary>
public class RegisterUserHandler : ICommand<RegisterUserCommand, RegisterUserResultDto>
{
    private readonly IExpressBotService _expressBotService;
    private readonly ILogger<RegisterUserHandler> _logger;

    public RegisterUserHandler(
        IExpressBotService expressBotService,
        ILogger<RegisterUserHandler> logger)
    {
        _expressBotService = expressBotService;
        _logger = logger;
    }

    public async Task<RegisterUserResultDto> Handle(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Регистрация пользователя {UserId} в чате {ChatId}", command.UserId, command.ChatId);

            // Отправляем приветственное сообщение пользователю
            var welcomeMessage = GenerateWelcomeMessage(command);
            
            var result = await _expressBotService.SendTextMessageAsync(
                command.ChatId,
                welcomeMessage,
                cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("Пользователь {UserId} успешно зарегистрирован в чате {ChatId}", 
                    command.UserId, command.ChatId);
                
                return new RegisterUserResultDto(
                    Success: true,
                    Message: "Пользователь успешно зарегистрирован",
                    Timestamp: DateTime.UtcNow);
            }

            _logger.LogWarning("Ошибка при регистрации пользователя {UserId}: {Error}", 
                command.UserId, result.ErrorMessage);
                
            return new RegisterUserResultDto(
                Success: false,
                Message: "Ошибка при отправке приветственного сообщения",
                ErrorCode: "SEND_MESSAGE_FAILED",
                Timestamp: DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при регистрации пользователя {UserId} в чате {ChatId}", 
                command.UserId, command.ChatId);
                
            return new RegisterUserResultDto(
                Success: false,
                Message: ex.Message,
                ErrorCode: "REGISTRATION_ERROR",
                Timestamp: DateTime.UtcNow);
        }
    }

    private static string GenerateWelcomeMessage(RegisterUserCommand command)
    {
        var displayName = !string.IsNullOrEmpty(command.FirstName) 
            ? $"{command.FirstName} {command.LastName}".Trim()
            : command.Username ?? "пользователь";

        return $"""
            👋 Добро пожаловать, {displayName}!
            
            Вы успешно зарегистрированы в системе MultiFactor.
            
            Теперь вы будете получать уведомления о запросах на авторизацию через этого бота.
            
            Для получения справки введите команду /help
            """;
    }
}


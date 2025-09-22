namespace MF.Express.Bot.Application.DTOs;

/// <summary>
/// DTO для запроса регистрации пользователя в чате
/// </summary>
public record RegisterUserRequestDto(
    string ChatId,
    string UserId,
    string? Username = null,
    string? FirstName = null,
    string? LastName = null,
    Dictionary<string, object>? Metadata = null
);


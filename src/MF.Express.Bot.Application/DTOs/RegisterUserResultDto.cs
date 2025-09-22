namespace MF.Express.Bot.Application.DTOs;

/// <summary>
/// DTO результата регистрации пользователя
/// </summary>
public record RegisterUserResultDto(
    bool Success,
    string? Message = null,
    string? ErrorCode = null,
    DateTime Timestamp = default
)
{
    public RegisterUserResultDto() : this(false) 
    {
        Timestamp = DateTime.UtcNow;
    }
};


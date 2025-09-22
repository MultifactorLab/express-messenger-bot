namespace MF.Express.Bot.Application.DTOs;

/// <summary>
/// DTO информации о боте
/// </summary>
public record BotInfoDto(
    string Id,
    string Name,
    string Username,
    bool IsActive = true,
    DateTime? LastSeen = null
);


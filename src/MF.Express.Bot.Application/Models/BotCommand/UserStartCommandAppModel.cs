namespace MF.Express.Bot.Application.Models.BotCommand;

public record UserStartCommandAppModel(
    string UserId,
    string ChatId,
    string? Username,
    string? FirstName,
    string? LastName,
    string? AdLogin,
    string? AdDomain,
    string? ChatType,
    DateTime Timestamp,
    string? Platform,
    string? AppVersion,
    string? Device,
    string? Locale,
    Dictionary<string, object>? Metadata = null
);


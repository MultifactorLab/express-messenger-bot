namespace MF.Express.Bot.Application.Models.Auth;

public record UserChatInfoAppModel(
    string UserId,
    string ChatId,
    string? Username,
    string? FirstName,
    string? LastName,
    DateTime FirstContactTime,
    string? LastMessage = null,
    Dictionary<string, object>? Metadata = null
);


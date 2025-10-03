using MF.Express.Bot.Application.Models.BotCommand;

namespace MF.Express.Bot.Application.Models.Auth;

public record AuthorizationResultAppModel(
    string AuthRequestId,
    string UserId,
    AuthAction Action,
    DateTime ProcessedAt,
    string? AdditionalInfo = null
);

